﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FunderMaps.Models.Identity;
using FunderMaps.Identity;
using FunderMaps.Extensions;
using FunderMaps.Helpers;
using FunderMaps.Data;
using FunderMaps.Data.Authorization;
using FunderMaps.ViewModels;

namespace FunderMaps.Controllers.Api
{
    /// <summary>
    /// Authentication endpoint.
    /// </summary>
    [Authorize]
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : BaseApiController
    {
        private readonly FunderMapsDbContext _context;
        private readonly UserManager<FunderMapsUser> _userManager;
        private readonly SignInManager<FunderMapsUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        /// <summary>
        /// Create new instance.
        /// </summary>
        public AuthenticationController(
            FunderMapsDbContext context,
            UserManager<FunderMapsUser> userManager,
            SignInManager<FunderMapsUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        public class PrincipalOutputModel
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public IList<string> Roles { get; set; }
            public IList<Claim> Claims { get; set; }
        }

        public sealed class UserOutputModel : PrincipalOutputModel
        {
            public bool? TwoFactorEnabled { get; set; }
            public bool? EmailConfirmed { get; set; }
            public bool? LockoutEnabled { get; set; }
            public bool? PhoneNumberConfirmed { get; set; }
            public int AccessFailedCount { get; set; }
            public DateTimeOffset? LockoutEnd { get; set; }
            public int? AttestationPrincipalId { get; set; }
        }

        /// <summary>
        /// Map identity errors to error output model.
        /// </summary>
        /// <param name="errors">List of errors.</param>
        /// <returns>ErrorOutputModel.</returns>
        protected ErrorOutputModel IdentityErrorResponse(IEnumerable<IdentityError> errors)
        {
            var errorModel = new ErrorOutputModel();
            foreach (var item in errors)
            {
                errorModel.AddError(0, item.Description);
            }

            return errorModel;
        }

        // GET: api/authentication
        /// <summary>
        /// Get authentication principal properties related to
        /// the current authenticated object.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UserOutputModel), 200)]
        public async Task<IActionResult> GetAsync()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return ResourceNotFound();
            }

            return Ok(new UserOutputModel
            {
                Id = user.Id,
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,
                LockoutEnd = user.LockoutEnd,
                Email = user.Email,
                AttestationPrincipalId = user.AttestationPrincipalId,
                Roles = await _userManager.GetRolesAsync(user),
                Claims = await _userManager.GetClaimsAsync(user),
            });
        }

        public sealed class AuthenticationOutputModel
        {
            public PrincipalOutputModel Principal { get; set; }

            /// <summary>
            /// Token as encoded string.
            /// </summary>
            public string Token { get; set; }

            /// <summary>
            /// Token validity in seconds.
            /// </summary>
            public int TokenValid { get; set; }
        }

        /// <summary>
        /// Generate authentication token for user.
        /// </summary>
        /// <param name="user">See <see cref="FunderMapsUser"/>.</param>
        /// <returns>AuthenticationOutputModel.</returns>
        private async Task<AuthenticationOutputModel> GenerateSecurityToken(FunderMapsUser user)
        {
            var token = new JwtTokenIdentityUser<FunderMapsUser, Guid>(user, _configuration.GetJwtSignKey())
            {
                Issuer = _configuration.GetJwtIssuer(),
                Audience = _configuration.GetJwtAudience(),
                TokenValid = _configuration.GetJwtTokenExpirationInMinutes(),
            };

            token.AddRoleClaims(await _userManager.GetRolesAsync(user));
            token.AddClaim(FisClaimTypes.UserAttestationIdentifier, user.AttestationPrincipalId);

            var organizationUser = await _context.OrganizationUsers
                .AsNoTracking()
                .Include(s => s.Organization)
                .Include(s => s.OrganizationRole)
                .Select(s => new { s.UserId, s.Organization, s.OrganizationRole })
                .SingleOrDefaultAsync(q => q.UserId == user.Id);

            if (organizationUser != null)
            {
                token.AddClaim(FisClaimTypes.OrganizationUserRole, organizationUser.OrganizationRole.Name);
                token.AddClaim(FisClaimTypes.OrganizationAttestationIdentifier, organizationUser.Organization.AttestationOrganizationId);
            }

            return new AuthenticationOutputModel
            {
                Principal = new PrincipalOutputModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user),
                    Claims = token.Claims,
                },
                Token = token.WriteToken(),
                TokenValid = token.TokenValid,
            };
        }

        // POST: api/authentication/authenticate
        /// <summary>
        /// Authenticate user object.
        /// </summary>
        /// <param name="input">See <see cref="UserInputModel"/>.</param>
        /// <returns>See <see cref="AuthenticationOutputModel"/>.</returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(AuthenticationOutputModel), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> SignInAsync([FromBody] UserInputModel input)
        {
            // Find user for authnetication, if the user object cannot be found then return an
            // authentication faillure since the client is not allowed to guess the credentials.
            var user = await _userManager.Users.SingleOrDefaultAsync(s => s.Email == input.Email);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed, unknown object {user}", input.Email);
                return Unauthorized(103, "Invalid credentials provided");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, input.Password, true);
            if (result.IsLockedOut)
            {
                return Unauthorized(101, "Principal is locked out, contact the administrator");
            }
            else if (result.IsNotAllowed)
            {
                return Unauthorized(102, "Principal is not allowed to login");
            }
            else if (result.Succeeded)
            {
                _logger.LogInformation("Authentication successful, returning security token");

                return Ok(await GenerateSecurityToken(user));
            }

            // FUTURE: RequiresTwoFactor

            return Unauthorized(103, "Invalid credentials provided");
        }

        // GET: api/authentication/refresh
        /// <summary>
        /// Refresh authentication token.
        /// </summary>
        [HttpGet("refresh")]
        [ProducesResponseType(typeof(AuthenticationOutputModel), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> RefreshSignInAsync()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return ResourceNotFound();
            }

            if (await _signInManager.CanSignInAsync(user))
            {
                _logger.LogInformation("Authentication refreshed, returning security token");

                return Ok(await GenerateSecurityToken(user));
            }

            return Unauthorized(102, "Principal is not allowed to login");
        }

        public sealed class ChangePasswordInputModel
        {
            /// <summary>
            /// New password.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            /// <summary>
            /// Old (current) password.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }
        }

        // POST: api/authentication/change_password
        /// <summary>
        /// Change user password.
        /// </summary>
        /// <param name="input">Password input model.</param>
        [HttpPost("change_password")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 400)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordInputModel input)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return ResourceNotFound();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return BadRequest(IdentityErrorResponse(changePasswordResult.Errors));
            }

            return NoContent();
        }

        // POST: api/authentication/set_password
        /// <summary>
        /// Set the password for a user. Only an administrator can do this.
        /// </summary>
        /// <param name="input">User password input model.</param>
        [Authorize(Roles = Constants.AdministratorRole)]
        [HttpPost("set_password")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 400)]
        public async Task<IActionResult> SetPasswordAsync([FromBody] UserInputModel input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
            {
                return ResourceNotFound();
            }

            return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                // Create everything at once, or nothing at all if an error occurs.
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    if (await _userManager.HasPasswordAsync(user))
                    {
                        await _userManager.RemovePasswordAsync(user);
                    }

                    var addPasswordResult = await _userManager.AddPasswordAsync(user, input.Password);
                    if (!addPasswordResult.Succeeded)
                    {
                        transaction.Rollback();
                        return BadRequest(IdentityErrorResponse(addPasswordResult.Errors));
                    }

                    transaction.Commit();

                    return NoContent();
                }
            });
        }
    }
}
