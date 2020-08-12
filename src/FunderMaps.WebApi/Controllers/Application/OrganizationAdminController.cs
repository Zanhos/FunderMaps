﻿using AutoMapper;
using FunderMaps.Controllers;
using FunderMaps.Core.Entities;
using FunderMaps.Core.Managers;
using FunderMaps.WebApi.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

#pragma warning disable CA1062 // Validate arguments of public methods
namespace FunderMaps.WebApi.Controllers.Application
{
    /// <summary>
    ///     Endpoint controller for organization administration.
    /// </summary>
    /// <remarks>
    ///     This controller provides organization administration.
    ///     <para>
    ///         For the variant based on the current session see 
    ///         <see cref="OrganizationController"/>.
    ///     </para>
    /// </remarks>
    [Authorize(Policy = "AdministratorPolicy")]
    [ApiController, Route("api/organization")]
    public class OrganizationAdminController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly OrganizationManager _organizationManager;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public OrganizationAdminController(IMapper mapper, OrganizationManager organizationManager)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _organizationManager = organizationManager ?? throw new ArgumentNullException(nameof(organizationManager));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            // Act.
            Organization organization = await _organizationManager.GetAsync(id);

            // Map.
            var output = _mapper.Map<OrganizationDto>(organization);

            // Return.
            return Ok(output);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] OrganizationDto input)
        {
            // Map.
            var organization = _mapper.Map<Organization>(input);
            organization.Id = id;

            // Act.
            await _organizationManager.UpdateAsync(organization);

            // Return.
            return NoContent();
        }
    }
}
#pragma warning restore CA1062 // Validate arguments of public methods
