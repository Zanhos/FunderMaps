﻿using AutoMapper;
using FunderMaps.AspNetCore.DataTransferObjects;
using FunderMaps.Core.DataAnnotations;
using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Interfaces.Repositories;
using FunderMaps.Helpers;
using FunderMaps.WebApi.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1062 // Validate arguments of public methods
namespace FunderMaps.WebApi.Controllers.Report
{
    /// <summary>
    ///     Endpoint controller for incident operations.
    /// </summary>
    [Route("incident")]
    public class IncidentController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly Core.AppContext _appContext;
        private readonly IContactRepository _contactRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IBlobStorageService _fileStorageService;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public IncidentController(
            IMapper mapper,
            Core.AppContext appContext,
            IContactRepository contactRepository,
            IIncidentRepository incidentRepository,
            IAddressRepository addressRepository,
            IBlobStorageService fileStorageService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
            _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync([Incident] string id)
        {
            // Act.
            var incident = await _incidentRepository.GetByIdAsync(id);
            incident.ContactNavigation = await _contactRepository.GetByIdAsync(incident.Email);

            // Map.
            var output = _mapper.Map<IncidentDto>(incident);

            // Return.
            return Ok(output);
        }

        /// <summary>
        ///     Upload document to the backstore.
        /// </summary>
        /// <param name="input">See <see cref="IFormFile"/>.</param>
        /// <returns>See <see cref="DocumentDto"/>.</returns>
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocumentAsync([Required] IFormFile input)
        {
            // FUTURE: Replace with validator?
            var virtualFile = new ApplicationFileWrapper(input, Constants.AllowedFileMimes);
            if (!virtualFile.IsValid)
            {
                throw new ArgumentException(); // TODO
            }

            // Act.
            var storeFileName = Core.IO.Path.GetUniqueName(input.FileName);
            await _fileStorageService.StoreFileAsync(
                containerName: "incident-report",
                fileName: storeFileName,
                contentType: input.ContentType,
                stream: input.OpenReadStream());

            var output = new DocumentDto
            {
                Name = storeFileName,
            };

            // Return.
            return Ok(output);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationDto pagination)
        {
            // Act.
            var incidentList = new List<Incident>();
            await foreach (var incident in _incidentRepository.ListAllAsync(pagination.Navigation))
            {
                incident.ContactNavigation = await _contactRepository.GetByIdAsync(incident.Email);
                incidentList.Add(incident);
            }

            // Map.
            var result = _mapper.Map<IList<IncidentDto>>(incidentList);

            // Return.
            return Ok(result);
        }

        /// <summary>
        ///     Post a new incident to the backend.
        /// </summary>
        /// <param name="input"><see cref="IncidentDto"/></param>
        /// <returns><see cref="OkObjectResult"/></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateAsync([FromBody] IncidentDto input)
        {
            // Map.
            var incident = _mapper.Map<Incident>(input);

            incident.Meta = new
            {
                SessionUser = _appContext.UserId,
                SessionOrganization = _appContext.TenantId,
                Gateway = Constants.IncidentGateway,
            };

            // Act.
            // There does not have to be a contact, but if it exists we'll save it.
            if (incident.ContactNavigation != null)
            {
                await _contactRepository.AddAsync(incident.ContactNavigation);
            }

            // FUTURE: Works for now, but may not be the best solution to check
            //         if input data is valid
            await _addressRepository.GetByIdAsync(incident.Address);

            var id = await _incidentRepository.AddAsync(incident);
            incident = await _incidentRepository.GetByIdAsync(id);
            incident.ContactNavigation = await _contactRepository.GetByIdAsync(incident.Email);

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
            await _incidentRepository.UpdateAsync(incident);

            // Return.
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([Incident] string id)
        {
            // Act.
            await _incidentRepository.DeleteAsync(id);

            // Return.
            return NoContent();
        }
    }
}
#pragma warning restore CA1062 // Validate arguments of public methods
