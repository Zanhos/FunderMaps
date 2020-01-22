﻿using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Repositories;
using FunderMaps.Extensions;
using FunderMaps.Helpers;
using FunderMaps.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FunderMaps.Controllers.Api
{
    /// <summary>
    /// Endpoint controller for sample operations.
    /// </summary>
    [Authorize(Policy = Constants.OrganizationMemberPolicy)]
    [Route("api/sample")]
    [ApiController]
    public class SampleController : BaseApiController
    {
        private readonly ISampleRepository _sampleRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IAddressService _addressService;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public SampleController(
            ISampleRepository sampleRepository,
            IReportRepository reportRepository,
            IAddressService addressService)
        {
            _sampleRepository = sampleRepository;
            _reportRepository = reportRepository;
            _addressService = addressService;
        }

        // GET: api/sample
        /// <summary>
        /// Get all samples filtered either by organization or as public data.
        /// </summary>
        /// <param name="offset">Offset into the list.</param>
        /// <param name="limit">Limit the output.</param>
        /// <returns>List of samples.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int offset = 0, [FromQuery] int limit = 25)
            => Ok(await _sampleRepository.ListAllAsync(User.GetOrganizationId(), new Navigation(offset, limit)));

        // GET: api/sample/report/{id}
        /// <summary>
        /// Get all samples filtered by report.
        /// </summary>
        /// <param name="id">Report identifier, see <see cref="Report.Id"/>.</param>
        /// <param name="offset">Offset into the list.</param>
        /// <param name="limit">Limit the output.</param>
        /// <returns>List of samples, see <see cref="Report"/>.</returns>
        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetAllAsync(int id, [FromQuery] int offset = 0, [FromQuery] int limit = 25)
            => Ok(await _sampleRepository.ListAllReportAsync(id, User.GetOrganizationId(), new Navigation(offset, limit)));

        // GET: api/sample/stats
        /// <summary>
        /// Return entity statistics.
        /// </summary>
        /// <returns>EntityStatsOutputModel.</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStatsAsync()
            => Ok(new EntityStatsOutputModel
            {
                Count = await _sampleRepository.CountAsync(User.GetOrganizationId())
            });

        // POST: api/sample
        /// <summary>
        /// Create a new sample for a report.
        /// </summary>
        /// <param name="input">See <see cref="Sample"/>.</param>
        /// <returns>See <see cref="Sample"/>.</returns>
        [HttpPost]
        [Authorize(Policy = Constants.OrganizationMemberWritePolicy)]
        public async Task<IActionResult> PostAsync([FromBody] SampleInputOutputModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var report = await _reportRepository.GetByIdAsync(input.Report.Value);
            if (report == null)
            {
                return ResourceNotFound();
            }

            var sample = new Sample
            {
                Report = report.Id,
                FoundationType = input.FoundationType,
                MonitoringWell = input.MonitoringWell,
                Cpt = input.Cpt,
                WoodLevel = input.WoodLevel,
                GroundLevel = input.GroundLevel,
                GroundwaterLevel = input.GroundwaterLevel,
                FoundationRecoveryAdviced = input.FoundationRecoveryAdviced,
                FoundationDamageCause = input.FoundationDamageCause,
                BuiltYear = input.BuiltYear,
                FoundationQuality = input.FoundationQuality,
                EnforcementTerm = input.EnforcementTerm,
                Substructure = input.Substructure,
                Note = input.Note,
                BaseMeasurementLevel = input.BaseMeasurementLevel,
                Address = await _addressService.GetOrCreateAddressAsync(new Address
                {
                    StreetName = input.Address.StreetName,
                    BuildingNumber = (short)input.Address.BuildingNumber,
                    Bag = input.Address.Bag,
                })
            };

            var id = await _sampleRepository.AddAsync(sample);

            // TODO: Fire event and set ReportStatus => ReportStatus.Pending

            return Ok(await _sampleRepository.GetByIdAsync(id));
        }

        // GET: api/sample/{id}
        /// <summary>
        /// Retrieve the sample by identifier. The sample is returned
        /// if the the record is public or if the organization user has
        /// access to the record.
        /// </summary>
        /// <param name="id">Sample identifier.</param>
        /// <returns>Report.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var sample = await _sampleRepository.GetPublicAndByIdAsync(id, User.GetOrganizationId());
            if (sample == null)
            {
                return ResourceNotFound();
            }

            return Ok(sample);
        }

        // PUT: api/sample/{id}
        /// <summary>
        /// Update sample if the organization user has access to the record.
        /// </summary>
        /// <param name="id">Sample identifier.</param>
        /// <param name="input">Sample data.</param>
        [HttpPut("{id}")]
        [Authorize(Policy = Constants.OrganizationMemberWritePolicy)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SampleInputOutputModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var sample = await _sampleRepository.GetByIdAsync(id, User.GetOrganizationId());
            if (sample == null)
            {
                return ResourceNotFound();
            }

            sample.FoundationType = input.FoundationType;
            sample.MonitoringWell = input.MonitoringWell;
            sample.Cpt = input.Cpt;
            sample.WoodLevel = input.WoodLevel;
            sample.GroundLevel = input.GroundLevel;
            sample.GroundwaterLevel = input.GroundwaterLevel;
            sample.FoundationRecoveryAdviced = input.FoundationRecoveryAdviced;
            sample.FoundationDamageCause = input.FoundationDamageCause;
            sample.BuiltYear = input.BuiltYear;
            sample.FoundationQuality = input.FoundationQuality;
            sample.EnforcementTerm = input.EnforcementTerm;
            sample.Substructure = input.Substructure;
            sample.Note = input.Note;
            sample.Address = await _addressService.GetOrCreateAddressAsync(new Address
            {
                StreetName = input.Address.StreetName,
                BuildingNumber = (short)input.Address.BuildingNumber,
                Bag = input.Address.Bag,
            });

            await _sampleRepository.UpdateAsync(sample);

            return NoContent();
        }

        // DELETE: api/sample/{id}
        /// <summary>
        /// Soft delete the sample if the organization user has access to the record.
        /// </summary>
        /// <param name="id">Sample identifier.</param>
        [HttpDelete("{id}")]
        [Authorize(Policy = Constants.OrganizationMemberWritePolicy)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var sample = await _sampleRepository.GetByIdAsync(id, User.GetOrganizationId());
            if (sample == null)
            {
                return ResourceNotFound();
            }

            await _sampleRepository.DeleteAsync(sample);

            return NoContent();
        }
    }
}
