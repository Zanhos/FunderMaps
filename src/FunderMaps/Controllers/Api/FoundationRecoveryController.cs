﻿using FunderMaps.Authorization.Requirement;
using FunderMaps.Core.Entities.Fis;
using FunderMaps.Core.Repositories;
using FunderMaps.Data.Authorization;
using FunderMaps.Extensions;
using FunderMaps.Helpers;
using FunderMaps.Interfaces;
using FunderMaps.Providers;
using FunderMaps.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FunderMaps.Controllers.Api
{
    // TODO:
    // - Make this stuff safer... add input validation checks. Do asserts on some of the stuff

    /// <summary>
    /// Endpoint for recovery operations.
    /// </summary>
    [Authorize]
    [Route("api/foundationrecovery")]
    [ApiController]
    public class FoundationRecoveryController : BaseApiController
    {
        private readonly DbProvider _dbProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFoundationRecoveryRepository _recoveryRepository;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public FoundationRecoveryController(
            DbProvider dbProvider,
            IAuthorizationService authorizationService,
            IFoundationRecoveryRepository recoveryRepository
            )
        {
            _dbProvider = dbProvider;
            _authorizationService = authorizationService;
            _recoveryRepository = recoveryRepository;
        }

        // functions as a read all method
        /// <summary>
        /// Get all the foundation recovery data based on the organisation id
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        // GET: api/foundationrecovery
        [HttpGet]
        [ProducesResponseType(typeof(FoundationRecovery), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]

        public async Task<IActionResult> GetAllAsync([FromQuery] int offset = 0, [FromQuery] int limit = 25)
        {
            // check the user
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            if (attestationOrganizationId == null)
            {
                return ResourceForbid();
            }

            // Hardcoded because every record has this attribution id
            attestationOrganizationId = "18729";

            // Administrator can query anything
            if (User.IsInRole(Constants.AdministratorRole))
            {
                return Ok(await _recoveryRepository.ListAllAsync(new Navigation(offset, limit)));
            }

            // return EVERYTHING listed in de foundation_recovery table based on the id of the organisation
            return Ok(await _recoveryRepository.ListAllAsync(int.Parse(attestationOrganizationId), new Navigation(offset, limit)));
        }


        // get all the data of an foundation recovery report based on the ID given in the get request
        // functions as a read something method
        // GET: api/foundationrecovery/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FoundationRecovery), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            #region check the user
            // check the user
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // Administrator can query anything
            if (attestationOrganizationId == null)
            {
                return ResourceForbid();
            }
            #endregion

            return Ok(await _recoveryRepository.GetByIdAsync(id));
        }

        // TODO: met of zonder de "deleted" reports

        // hit this endpoint to retrieve the amount of foundation recovery reports
        // GET: api/foundationrecovery/stats
        [HttpGet("stats")]
        [ProducesResponseType(typeof(EntityStatsOutputModel), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> GetStatsAsync()
        {
            // check the user
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // if its not able to convert it to an integer
            // this also catches it if the attestationOrganizationId equals null
            if (!int.TryParse(attestationOrganizationId, out int organisationId))
            {
                return ResourceForbid();
            }

            // Administrator can query anything
            if (User.IsInRole(Constants.AdministratorRole))
            {
                //yeet back to the admin
                return Ok(new EntityStatsOutputModel
                {
                    Count = await _recoveryRepository.CountAsync()
                });
            }

            // yeet back to the user based on the organization id
            return Ok(new EntityStatsOutputModel
            {
                Count = await _recoveryRepository.CountAsync(organisationId)
            });
        }

        // this is like a create method. This pushes the foundation recovery info into the database
        // create a new foundation recovery report.
        // POST: api/foundationrecovery
        [HttpPost]
        [ProducesResponseType(typeof(FoundationRecovery), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> PostAsync([FromBody]FoundationRecovery input)
        {
            // Check the user
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // NOTE: If it's not able to convert it to an integer
            //       this also catches it if the attestationOrganizationId equals null
            if (!int.TryParse(attestationOrganizationId, out int organisationId))
            {
                return ResourceForbid();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, organisationId, OperationsRequirement.Create);
            if (authorizationResult.Succeeded)
            {
                var recovery = new FoundationRecovery
                {
                    AccessPolicy = input.AccessPolicy,
                    Address = input.Address,
                    AddressNavigation = input.AddressNavigation,
                    Attribution = input.Attribution,
                    AttributionNavigation = input.AttributionNavigation,
                    FoundationRecoveryEvidence = input.FoundationRecoveryEvidence,
                    FoundationRecoveryRepair = input.FoundationRecoveryRepair,
                    Id = input.Id,
                    Note = input.Note,
                    Type = input.Type,
                    Year = input.Year
                };
                await _recoveryRepository.AddAsync(recovery);

                return Ok(recovery);
            }
            // Yeet the user if authorisation failed
            return ResourceForbid();
        }


        // Update info about the fundation recovery
        // PUT: api/foundationrecovery/id
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] FoundationRecovery input)
        {
            #region check the user
            // check the user
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // if its not able to convert it to an integer
            // this also catches it if the attestationOrganizationId equals null
            if (!int.TryParse(attestationOrganizationId, out int organisationId))
            {
                return ResourceForbid();
            }
            #endregion

            if (id != input.Id)
            {
                return BadRequest(0, "Identifiers do not match entity");
            }

            var recovery = new FoundationRecovery
            {
                AccessPolicy = input.AccessPolicy,
                Address = input.Address,
                AddressNavigation = input.AddressNavigation,
                Attribution = input.Attribution,
                AttributionNavigation = input.AttributionNavigation,
                FoundationRecoveryEvidence = input.FoundationRecoveryEvidence,
                FoundationRecoveryRepair = input.FoundationRecoveryRepair,
                Id = input.Id,
                Note = input.Note,
                Type = input.Type,
                Year = input.Year
            };

            await _recoveryRepository.UpdateAsync(recovery);

            return NoContent();
        }

        // Set a report as deleted wehn hitting this endpoint
        // DELETE: api/foundationrecovery/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id, [FromBody] FoundationRecovery input)
        {
            #region check the user
            // check the user
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            if (!int.TryParse(attestationOrganizationId, out int organisationId))
            {
                return ResourceForbid();
            }
            #endregion

            // check if the url id matches the input id
            if (id != input.Id)
            {
                return BadRequest(0, "Identifiers do not match entity");
            }

            // retrieve the report based on the id
            var report = await _recoveryRepository.GetByIdAsync(id);
            if (report == null)
            {
                return ResourceNotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, organisationId, OperationsRequirement.Create);
            if (authorizationResult.Succeeded)
            {
                await _recoveryRepository.DeleteAsync(report);

                return NoContent();
            }

            return ResourceForbid();
        }
    }
}