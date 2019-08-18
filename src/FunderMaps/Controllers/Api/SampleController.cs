﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FunderMaps.Interfaces;
using FunderMaps.Authorization.Requirement;
using FunderMaps.Extensions;
using FunderMaps.Data.Authorization;
using FunderMaps.Core.Entities.Fis;
using FunderMaps.Core.Repositories;
using FunderMaps.ViewModels;
using FunderMaps.Helpers;

namespace FunderMaps.Controllers.Api
{
    /// <summary>
    /// Endpoint controller for sample operations.
    /// </summary>
    [Authorize]
    [Route("api/sample")]
    [ApiController]
    public class SampleController : BaseApiController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISampleRepository _sampleRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IAddressRepository _addressRepository;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public SampleController(
            IAuthorizationService authorizationService,
            ISampleRepository sampleRepository,
            IReportRepository reportRepository,
            IAddressRepository addressRepository)
        {
            _authorizationService = authorizationService;
            _sampleRepository = sampleRepository;
            _reportRepository = reportRepository;
            _addressRepository = addressRepository;
        }

        // GET: api/sample
        /// <summary>
        /// Get all samples filtered either by organization or as public data.
        /// </summary>
        /// <param name="offset">Offset into the list.</param>
        /// <param name="limit">Limit the output.</param>
        /// <returns>List of samples.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Sample>), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> GetAllAsync([FromQuery] int offset = 0, [FromQuery] int limit = 25)
        {
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // Administrator can query anything
            if (User.IsInRole(Constants.AdministratorRole))
            {
                return Ok(await _sampleRepository.ListAllAsync(new Navigation(offset, limit)));
            }

            if (attestationOrganizationId == null)
            {
                return ResourceForbid();
            }

            return Ok(await _sampleRepository.ListAllAsync(int.Parse(attestationOrganizationId), new Navigation(offset, limit)));
        }

        // GET: api/sample/report/{id}
        /// <summary>
        /// Get all samples filtered by report.
        /// </summary>
        /// <param name="id">Report identifier, see <see cref="Report.Id"/>.</param>
        /// <param name="offset">Offset into the list.</param>
        /// <param name="limit">Limit the output.</param>
        /// <returns>List of samples, see <see cref="Report"/>.</returns>
        [HttpGet("report/{id}")]
        [ProducesResponseType(typeof(List<Sample>), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> GetAllAsync(int id, [FromQuery] int offset = 0, [FromQuery] int limit = 25)
        {
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // Administrator can query anything
            if (User.IsInRole(Constants.AdministratorRole))
            {
                return Ok(await _sampleRepository.ListAllReportAsync(id, new Navigation(offset, limit)));
            }

            if (attestationOrganizationId == null)
            {
                return ResourceForbid();
            }

            return Ok(await _sampleRepository.ListAllReportAsync(id, int.Parse(attestationOrganizationId), new Navigation(offset, limit)));
        }

        // GET: api/sample/stats
        /// <summary>
        /// Return entity statistics.
        /// </summary>
        /// <returns>EntityStatsOutputModel.</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(EntityStatsOutputModel), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> GetStatsAsync()
        {
            var attestationOrganizationId = User.GetClaim(FisClaimTypes.OrganizationAttestationIdentifier);

            // Administrator can query anything
            if (User.IsInRole(Constants.AdministratorRole))
            {
                // TODO: Move into repo
                var _sql = @"SELECT COUNT(*)
                            FROM   report.sample AS samp
                                    INNER JOIN report.address AS addr ON samp.address = addr.id
                                    INNER JOIN report.report AS reprt ON samp.report = reprt.id
                                    INNER JOIN report.attribution AS attr ON reprt.attribution = attr.id
                            WHERE  samp.delete_date IS NULL";

                using (var connection = _dbProvider.ConnectionScope())
                {
                    return Ok(new EntityStatsOutputModel
                    {
                        Count = await connection.QuerySingleAsync<int>(_sql)
                    });
                }
            }

            if (attestationOrganizationId == null)
            {
                return ResourceForbid();
            }

            using (var connection = _dbProvider.ConnectionScope())
            {
                return Ok(new EntityStatsOutputModel
                {
                    Count = await _sampleRepository.CountAsync(int.Parse(attestationOrganizationId), connection)
                });
            }
        }

        // POST: api/sample
        /// <summary>
        /// Create a new sample for a report.
        /// </summary>
        /// <param name="input">See <see cref="Sample"/>.</param>
        /// <returns>See <see cref="Sample"/>.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Sample), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> PostAsync([FromBody] Sample2 input)
        {
            var sql = @"SELECT reprt.id,
                               reprt.document_id,
                               reprt.inspection,
                               reprt.joint_measurement, 
                               reprt.floor_measurement,
                               reprt.note,
                               reprt.status,
                               reprt.type,
                               reprt.document_date,
                               reprt.document_name,
                               reprt.access_policy,
                               attr.owner AS attribution
                        FROM   report.report AS reprt
                               INNER JOIN report.attribution AS attr ON reprt.attribution = attr.id
                        WHERE  reprt.delete_date IS NULL
                               AND reprt.id = @id
                        LIMIT  1 ";

            using (var connection = _dbProvider.ConnectionScope())
            {
                var result = await connection.QueryAsync<Report2>(sql, new { Id = input.Report });

                if (result.Count() == 0)
                {
                    return ResourceNotFound();
                }

                var report = result.First();

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, report.Attribution, OperationsRequirement.Create);
                if (authorizationResult.Succeeded)
                {
                    // Check if we can add new samples to the report
                    if (report.Status != ReportStatus.Todo && report.Status != ReportStatus.Pending)
                    {
                        return Forbid(0, "Resource modification forbidden with current status");
                    }

                    // TODO: Add address, foundation_type, foundation_damage_cause
                    var _sql = @"INSERT INTO report.sample AS samp
                                            (report,
                                             monitoring_well,
                                             cpt,
                                             note,
                                             wood_level,
                                             groundlevel,
                                             groundwater_level,
                                             foundation_recovery_adviced,
                                             built_year,
                                             foundation_quality,
                                             enforcement_term,
                                             substructure,
                                             base_measurement_level,
                                             access_policy,
                                             address)
                                VALUES      (@Report,
                                             @MonitoringWell,
                                             @Cpt,
                                             @Note,
                                             @WoodLevel,
                                             @GroundLevel,
                                             @GroundwaterLevel,
                                             @FoundationRecoveryAdviced,
                                             @BuiltYear,
                                             @FoundationQuality,
                                             @EnforcementTerm,
                                             @Substructure,
                                             (enum_range(NULL::report.base_measurement_level))[@BaseMeasurementLevel + 1],
                                             (enum_range(NULL::attestation.access_policy_type))[@AccessPolicy + 1],
                                             @_Address)
                                RETURNING id";

                    var _sql2 = @"UPDATE report.report AS reprt SET status = 'pending' WHERE reprt.id = @id";

                    input._Address = input.Address.Id;

                    input.Id = await connection.ExecuteScalarAsync<int>(_sql, input);
                    await connection.ExecuteAsync(_sql2, input.Report);

                    var sql3 = @"SELECT samp.id,
                               samp.report,
                               samp.foundation_type,
                               attr.owner AS attribution,
                               samp.monitoring_well,
                               samp.cpt,
                               samp.create_date, 
                               samp.update_date,
                               samp.note,
                               samp.wood_level,
                               samp.groundwater_level,
                               samp.groundlevel,
                               samp.foundation_recovery_adviced,
                               samp.foundation_damage_cause,
                               samp.built_year,
                               samp.foundation_quality,
                               samp.access_policy,
                               samp.enforcement_term,
                               samp.base_measurement_level,
                               addr.*
                        FROM   report.sample AS samp
                               INNER JOIN report.address AS addr ON samp.address = addr.id
                               INNER JOIN report.report AS reprt ON samp.report = reprt.id
                               INNER JOIN report.attribution AS attr ON reprt.attribution = attr.id
                        WHERE  samp.delete_date IS NULL
                               AND samp.id = @Id
                        LIMIT  1";

                    var result2 = await connection.QueryAsync<Sample2, Address2, Sample2>(sql: sql3, map: (sampleEntity, addressEntity) =>
                    {
                        sampleEntity.Address = addressEntity;
                        return sampleEntity;
                    }, splitOn: "id", param: input);

                    if (result2.Count() == 0)
                    {
                        return ResourceNotFound();
                    }

                    return Ok(result2.First());
                }

                return ResourceForbid();
            }
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
        [ProducesResponseType(typeof(Sample), 200)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> GetAsync(int id)
        {
            var sql = @"SELECT samp.id,
                               samp.report,
                               samp.foundation_type,
                               attr.owner AS attribution,
                               samp.monitoring_well,
                               samp.cpt,
                               samp.create_date, 
                               samp.update_date,
                               samp.note,
                               samp.wood_level,
                               samp.groundwater_level,
                               samp.groundlevel,
                               samp.foundation_recovery_adviced,
                               samp.foundation_damage_cause,
                               samp.built_year,
                               samp.foundation_quality,
                               samp.access_policy,
                               samp.enforcement_term,
                               samp.base_measurement_level,
                               addr.*
                        FROM   report.sample AS samp
                               INNER JOIN report.address AS addr ON samp.address = addr.id
                               INNER JOIN report.report AS reprt ON samp.report = reprt.id
                               INNER JOIN report.attribution AS attr ON reprt.attribution = attr.id
                        WHERE  samp.delete_date IS NULL
                               AND samp.id = @Id
                        LIMIT  1";

            using (var connection = _dbProvider.ConnectionScope())
            {
                var result = await connection.QueryAsync<Sample2, Address2, Sample2>(sql: sql, map: (sampleEntity, addressEntity) =>
                {
                    sampleEntity.Address = addressEntity;
                    return sampleEntity;
                }, param: new { Id = id });

                if (result.Count() == 0)
                {
                    return ResourceNotFound();
                }

                var sample = result.First();
                if (sample.AccessPolicy == AccessPolicy.Public) // sample.IsPublic() && sample.ReportNavigation.IsPublic()
                {
                    return Ok(sample);
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, sample.Attribution, OperationsRequirement.Read);
                if (authorizationResult.Succeeded)
                {
                    return Ok(sample);
                }

                return ResourceForbid();
            }
        }

        class BaseLevelHandler : SqlMapper.TypeHandler<BaseLevel>
        {
            public override BaseLevel Parse(object value)
            {
                return BaseLevel.NAP;
            }

            public override void SetValue(IDbDataParameter parameter, BaseLevel value)
            {
                parameter.DbType = DbType.String;
                parameter.Value = value.ToString().ToLower();
            }
        }

        // PUT: api/sample/{id}
        /// <summary>
        /// Update sample if the organization user has access to the record.
        /// </summary>
        /// <param name="id">Sample identifier.</param>
        /// <param name="input">Sample data.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 400)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] Sample2 input)
        {
            var sql = @"SELECT samp.id,
                               samp.report,
                               samp.foundation_type,
                               attr.owner AS attribution,
                               samp.monitoring_well,
                               samp.cpt,
                               samp.create_date, 
                               samp.update_date,
                               samp.note,
                               samp.wood_level,
                               samp.groundwater_level,
                               samp.groundlevel,
                               samp.foundation_recovery_adviced,
                               samp.foundation_damage_cause,
                               samp.built_year,
                               samp.foundation_quality,
                               samp.access_policy,
                               samp.enforcement_term,
                               samp.base_measurement_level,
                               addr.*
                        FROM   report.sample AS samp
                               INNER JOIN report.address AS addr ON samp.address = addr.id
                               INNER JOIN report.report AS reprt ON samp.report = reprt.id
                               INNER JOIN report.attribution AS attr ON reprt.attribution = attr.id
                        WHERE  samp.delete_date IS NULL
                               AND samp.id = @Id
                        LIMIT  1";

            if (id != input.Id)
            {
                return BadRequest(0, "Identifiers do not match entity");
            }

            using (var connection = _dbProvider.ConnectionScope())
            {
                var result = await connection.QueryAsync<Sample2, Address2, Sample2>(sql: sql, map: (sampleEntity, addressEntity) =>
                {
                    sampleEntity.Address = addressEntity;
                    return sampleEntity;
                }, param: new { Id = id });

                if (result.Count() == 0)
                {
                    return ResourceNotFound();
                }

                var sample = result.First();

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, sample.Attribution, OperationsRequirement.Update);
                if (authorizationResult.Succeeded)
                {
                    // TODO: Add address, foundation_type, foundation_damage_cause, access_policy
                    var _sql = @"UPDATE report.sample AS samp
                                SET    monitoring_well = @MonitoringWell,
                                       cpt = @Cpt,
                                       note = @Note,
                                       wood_level = @WoodLevel,
                                       groundlevel = @GroundLevel,
                                       groundwater_level = @GroundwaterLevel,
                                       foundation_recovery_adviced = @FoundationRecoveryAdviced,
                                       built_year = @BuiltYear,
                                       foundation_quality = @FoundationQuality,
                                       enforcement_term = @EnforcementTerm,
                                       substructure = @Substructure
                                       -- foundation_type = @FoundationType,
                                       -- foundation_damage_cause = @FoundationDamageCause,
                                       -- access_policy = @AccessPolicy
                                WHERE  samp.delete_date IS NULL
                                       AND samp.id = @Id";

                    await connection.ExecuteAsync(_sql, input);

                    return NoContent();
                }

                return ResourceForbid();
            }
        }

        // DELETE: api/sample/{id}
        /// <summary>
        /// Soft delete the sample if the organization user has access to the record.
        /// </summary>
        /// <param name="id">Sample identifier.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorOutputModel), 404)]
        [ProducesResponseType(typeof(ErrorOutputModel), 401)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var sql = @"SELECT samp.id,
                               samp.report,
                               samp.foundation_type,
                               attr.owner AS attribution,
                               samp.monitoring_well,
                               samp.cpt,
                               samp.create_date, 
                               samp.update_date,
                               samp.note,
                               samp.wood_level,
                               samp.groundwater_level,
                               samp.groundlevel,
                               samp.foundation_recovery_adviced,
                               samp.foundation_damage_cause,
                               samp.built_year,
                               samp.foundation_quality,
                               samp.access_policy,
                               samp.enforcement_term,
                               samp.base_measurement_level,
                               addr.*
                        FROM   report.sample AS samp
                               INNER JOIN report.address AS addr ON samp.address = addr.id
                               INNER JOIN report.report AS reprt ON samp.report = reprt.id
                               INNER JOIN report.attribution AS attr ON reprt.attribution = attr.id
                        WHERE  samp.delete_date IS NULL
                               AND samp.id = @Id
                        LIMIT  1";

            using (var connection = _dbProvider.ConnectionScope())
            {
                var result = await connection.QueryAsync<Sample2, Address2, Sample2>(sql: sql, map: (sampleEntity, addressEntity) =>
                {
                    sampleEntity.Address = addressEntity;
                    return sampleEntity;
                }, param: new { Id = id });

                if (result.Count() == 0)
                {
                    return ResourceNotFound();
                }

                var sample = result.First();

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, sample.Attribution, OperationsRequirement.Delete);
                if (authorizationResult.Succeeded)
                {
                    var _sql = @"UPDATE report.sample AS samp
                                SET    delete_date = CURRENT_TIMESTAMP
                                WHERE  samp.delete_date IS NULL
                                       AND samp.id = @Id";

                    await connection.ExecuteAsync(_sql, new { Id = id });

                    return NoContent();
                }

                return ResourceForbid();
            }
        }
    }
}
