﻿using AutoMapper;
using FunderMaps.Controllers;
using FunderMaps.Core.Authentication;
using FunderMaps.Core.DataAnnotations;
using FunderMaps.Core.Entities;
using FunderMaps.Core.UseCases;
using FunderMaps.Helpers;
using FunderMaps.WebApi.DataTransferObjects;
using FunderMaps.WebApi.InputModels;
using FunderMaps.WebApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1062 // Validate arguments of public methods
namespace FunderMaps.WebApi.Controllers.Report
{
    /// <summary>
    ///     Endpoint controller for incident operations.
    /// </summary>
    [Route("incident")]
    public class IncidentController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly AuthManager _authManager;
        private readonly IncidentUseCase _incidentUseCase;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public IncidentController(IMapper mapper, AuthManager authManager, IncidentUseCase incidentUseCase)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
            _incidentUseCase = incidentUseCase ?? throw new ArgumentNullException(nameof(incidentUseCase));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([Incident] string id)
        {
            var incident = await _incidentUseCase.GetAsync(id);

            // Map.
            var output = _mapper.Map<IncidentDto>(incident);

            // Return.
            return Ok(output);
        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocumentAsync(IFormFile input)
        {
            // FUTURE: Replace with validator?
            var virtualFile = new ApplicationFileWrapper(input, Constants.AllowedFileMimes);
            if (!virtualFile.IsValid)
            {
                throw new ArgumentException(); // TODO
            }

            // Act.
            var fileName = await _incidentUseCase.StoreDocumentAsync(
                input.OpenReadStream(),
                input.FileName,
                input.ContentType);

            var output = new DocumentDto
            {
                Name = fileName,
            };

            // Return.
            return Ok(output);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationModel pagination)
        {
            // Act.
            IAsyncEnumerable<Incident> incidentList = _incidentUseCase.GetAllAsync(pagination.Navigation);

            // Map.
            var result = await _mapper.MapAsync<IList<IncidentDto>, Incident>(incidentList);

            // Return.
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IncidentDto input)
        {
            // Map.
            var incident = _mapper.Map<Incident>(input);

            User sessionUser = await _authManager.GetUserAsync(User);
            Organization sessionOrganization = await _authManager.GetOrganizationAsync(User);

            incident.Meta = new
            {
                SessionUser = sessionUser.Id,
                sessionOrganization = sessionOrganization.Id,
                Gateway = Constants.IncidentGateway,
            };

            // Act.
            incident = await _incidentUseCase.CreateAsync(incident);

            // Map.
            var output = _mapper.Map<IncidentDto>(incident);

            // Return.
            return Ok(output);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([Incident] string id, [FromBody] IncidentDto input)
        {
            // Map.
            var incident = _mapper.Map<Incident>(input);
            incident.Id = id;

            // Act.
            await _incidentUseCase.UpdateAsync(incident);

            // Return.
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([Incident] string id)
        {
            // Act.
            await _incidentUseCase.DeleteAsync(id);

            // Return.
            return NoContent();
        }

        /// <summary>
        ///     Submit a new incident to our backend.
        /// </summary>
        /// <param name="input"><see cref="IncidentInputModel"/></param>
        /// <param name="incidentUseCase"><see cref="IncidentUseCase"/></param>
        /// <param name="mapper"><see cref="IMapper"/></param>
        /// <returns><see cref="OkObjectResult"/></returns>
        [AllowAnonymous]
        [HttpPost("submit")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SubmitAsync([FromBody] IncidentInputModel input, [FromServices] IncidentUseCase incidentUseCase, [FromServices] IMapper mapper)
        {
            // Map.
            // TODO Clean up?
            var incident = mapper.Map<Incident>(input);
            incident.ContactNavigation = new Contact
            {
                Email = input.Email,
                Name = input.Name,
                PhoneNumber = input.Phonenumber
            };

            // Act.
            await incidentUseCase.CreateAsync(incident);

            // Return.
            return NoContent();
        }
    }
}
#pragma warning restore CA1062 // Validate arguments of public methods
