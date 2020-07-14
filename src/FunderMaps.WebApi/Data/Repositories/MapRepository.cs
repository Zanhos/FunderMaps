﻿namespace FunderMaps.Data.Repositories
{
#if DISABLED
    /// <summary>
    /// Sample repository.
    /// </summary>
    public class MapRepository : IMapRepository
    {
        private readonly DbProvider _dbProvider;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="dbProvider">Database provider.</param>
        public MapRepository(DbProvider dbProvider) => _dbProvider = dbProvider;

        /// <summary>
        /// Get all address points by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressPoint"/>.</returns>
        public async Task<IReadOnlyList<AddressPoint>> GetByOrganizationIdAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT addr.street_name,
                        addr.building_number,
                        samp.report,
                        st_x(addr.geopoint) AS x,
                        st_y(addr.geopoint) AS y,
                        st_z(addr.geopoint) AS z
                FROM   application.sample AS samp
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                WHERE  addr.geopoint IS NOT NULL
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressPoint>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        // TODO: Not used?
        /// <summary>
        /// Get all address points not in recovery and by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressPoint"/>.</returns>
        public async Task<IReadOnlyList<AddressPoint>> GetFounationRecoveryByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT addr.street_name,
                        addr.building_number,
                        samp.report,
                        st_x(addr.geopoint) AS x,
                        st_y(addr.geopoint) AS y,
                        st_z(addr.geopoint) AS z
                FROM   application.sample AS samp
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id,
                        org
                WHERE  addr.geopoint IS NOT NULL
                        AND EXISTS (SELECT 1 FROM application.foundation_recovery AS recv WHERE recv.address = addr.id)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressPoint>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get wood address points not in recovery and by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFounationTypeWoodByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  addr.street_name AS street,
	                    addr.building_number AS number,
	                    st_asgeojson(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
	                    AND (samp.foundation_type = 'wood'
                            OR samp.foundation_type = 'wood_amsterdam'
                            OR samp.foundation_type = 'wood_rotterdam')
                        AND NOT EXISTS (SELECT 1 FROM application.foundation_recovery AS recv WHERE recv.address = addr.id)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressGeoJson>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get concrete address points not in recovery and by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFounationTypeConcreteByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  addr.street_name AS street,
	                    addr.building_number AS number,
	                    st_asgeojson(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
	                    AND (samp.foundation_type = 'concrete')
                        AND NOT EXISTS (SELECT 1 FROM application.foundation_recovery AS recv WHERE recv.address = addr.id)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressGeoJson>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get nopile address points not in recovery and by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFounationTypeNoPileByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  addr.street_name AS street,
                        addr.building_number AS number,
                        st_asgeojson(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
                     AND (samp.foundation_type = 'no_pile'
                            OR samp.foundation_type = 'no_pile_masonry'
                            OR samp.foundation_type = 'no_pile_strips'
                            OR samp.foundation_type = 'no_pile_bearing_floor'
                            OR samp.foundation_type = 'no_pile_concrete_floor'
                            OR samp.foundation_type = 'no_pile_slit')
                        AND NOT EXISTS (SELECT 1 FROM application.foundation_recovery AS recv WHERE recv.address = addr.id)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressGeoJson>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get wood charger address points not in recovery and by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFounationTypeWoodChargerByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  addr.street_name AS street,
                        addr.building_number AS number,
                        st_asgeojson(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
                        AND (samp.foundation_type = 'wood_charger')
                        AND NOT EXISTS (SELECT 1 FROM application.foundation_recovery AS recv WHERE recv.address = addr.id)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressGeoJson>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get other address points not in recovery and by organization filter.
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFounationTypeOtherByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  addr.street_name AS street,
                        addr.building_number AS number,
                        st_asgeojson(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
                     AND (samp.foundation_type = 'weighted_pile'
                            OR samp.foundation_type = 'combined'
                            OR samp.foundation_type = 'steel_pile'
                            OR samp.foundation_type = 'other'
                            OR samp.foundation_type = 'unknown')
                        AND NOT EXISTS (SELECT 1 FROM application.foundation_recovery AS recv WHERE recv.address = addr.id)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressGeoJson>(sql, new { Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get address points between build years not in recovery and by organization filter.
        /// </summary>
        /// <param name="rangeStart">Start offset in years.</param>
        /// <param name="rangeEnd">End limit in years.</param>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByEnforcementTermByOrganizationAsync(int rangeStart, int rangeEnd, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT addr.street_name,
                        addr.building_number,
                        samp.report,
                        st_asgeojson(prem.geom) AS geojson
                FROM   application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
	                    AND samp.enforcement_term IS NOT NULL
                        AND (EXTRACT(YEAR FROM age(application.add_enforcement_term(reprt.document_date, samp.enforcement_term), now()))
                            BETWEEN @Start AND @End)
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var result = await connection.QueryAsync<AddressGeoJson>(sql, new { Start = rangeStart, End = rangeEnd, Owner = orgId });
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get address points by foundation quality not in recovery and by organization filter.
        /// </summary>
        /// <param name="foundationQuality">Foundation quality to filer on.</param>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFoundationQualityByOrganizationAsync(FoundationQuality foundationQuality, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT addr.street_name,
                        addr.building_number,
                        samp.report,
                        ST_AsGeoJSON(prem.geom) AS geojson
                FROM   application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
                        AND samp.foundation_quality = @FoundationQuality::application.foundation_quality
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("FoundationQuality", foundationQuality.ToString().ToSnakeCase());
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get premise foundation subsidence.
        /// </summary>
        /// <param name="rangeStart">Start offset in milimeters.</param>
        /// <param name="rangeEnd">End limit in milimeters.</param>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFoundationSubsidenceByOrganizationAsync(double rangeStart, double rangeEnd, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
	                LIMIT 1
                )
                SELECT ST_AsGeoJSON(prem.geom) AS geojson
                FROM   geospatial.premise AS prem
		               INNER JOIN public.subsidence AS sub ON sub.identifica = prem.id,
		               org
                WHERE  ABS(sub.velocity) BETWEEN @Start AND @End
                       AND org.fence IS NOT NULL
		               AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Start", rangeStart);
            dynamicParameters.Add("End", rangeEnd);
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rangeStart">Start offset in years.</param>
        /// <param name="rangeEnd">End limit in years.</param>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetByFounationTypendicativeByOrganizationAsync(int rangeStart, int rangeEnd, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
	                LIMIT 1
                )
                SELECT  ST_AsGeoJSON(prem.geom) AS geojson
                FROM    geospatial.premise AS prem,
		                org
                WHERE   prem.built_year BETWEEN @Start AND @End
		                AND prem.id NOT IN (
			                SELECT      prem.id
                                FROM    application.sample AS samp
                                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                                        INNER JOIN application.address AS addr ON samp.address = addr.id
                                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object
                                WHERE   addr.bag IS NOT NULL
                                        AND (attr.owner = @Owner
                                            OR reprt.access_policy = 'public')
		                )
		                AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Start", rangeStart);
            dynamicParameters.Add("End", rangeEnd);
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rangeStart">Start offset in years.</param>
        /// <param name="rangeEnd">End limit in years.</param>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetPremiseYearByOrganizationAsync(int rangeStart, int rangeEnd, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
	                LIMIT 1
                )
                SELECT ST_AsGeoJSON(prem.geom) AS geojson
                FROM   geospatial.premise AS prem,
		               org
                WHERE  prem.built_year BETWEEN @Start AND @End
                       AND org.fence IS NOT NULL
		               AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Start", rangeStart);
            dynamicParameters.Add("End", rangeEnd);
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetReportTypeByOrganizationAsync(InquiryType type, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  ST_AsGeoJSON(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
                        AND reprt.type = @ReportType::application.report_type
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("ReportType", type.ToString().ToSnakeCase());
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rangeStart">Start offset in years.</param>
        /// <param name="rangeEnd">End limit in years.</param>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetDocumentYearByOrganizationAsync(int rangeStart, int rangeEnd, Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
                    LIMIT 1
                )
                SELECT  ST_AsGeoJSON(prem.geom) AS geojson
                FROM    application.sample AS samp
                        INNER JOIN application.report AS reprt ON samp.report = reprt.id
                        INNER JOIN application.attribution AS attr ON reprt.attribution = attr.id
                        INNER JOIN application.address AS addr ON samp.address = addr.id
                        INNER JOIN geospatial.residential_object AS reso ON addr.bag = reso.designation
                        INNER JOIN geospatial.premise AS prem ON reso.id = prem.residential_object,
                        org
                WHERE   addr.bag IS NOT NULL
                        AND reprt.document_date BETWEEN to_date(@Start::text, 'YYYY') AND to_date(@End::text, 'YYYY')
                        AND (attr.owner = @Owner
                            OR reprt.access_policy = 'public')
                        AND org.fence IS NOT NULL
		                AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Start", rangeStart);
            dynamicParameters.Add("End", rangeEnd);
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId">Organization identifier.</param>
        /// <returns>List of <see cref="AddressGeoJson"/>.</returns>
        public async Task<IReadOnlyList<AddressGeoJson>> GetPremiseByOrganizationAsync(Guid orgId)
        {
            using var connection = _dbProvider.ConnectionScope();

            var sql = @"
                WITH org AS (
	                SELECT id, name, fence
	                FROM application.organization AS org
	                WHERE id=@Owner
	                LIMIT 1
                )
                SELECT ST_AsGeoJSON(prem.geom) AS geojson
                FROM   geospatial.premise AS prem
	                   INNER JOIN public.bag_vlak_eig_rdam AS eig ON eig.bag_id = prem.id,
	                   org
                WHERE  eig.eigendom LIKE '%' || org.name || '%'
	                   AND org.fence IS NOT NULL
	                   AND ST_Contains(org.fence, prem.geom)";

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Owner", orgId);

            var result = await connection.QueryAsync<AddressGeoJson>(sql, dynamicParameters);
            if (!result.Any())
            {
                return null;
            }

            return result.ToArray();
        }
    }
#endif
}
