﻿using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Interfaces.Repositories;
using FunderMaps.Core.Types;
using FunderMaps.Data.Extensions;
using FunderMaps.Data.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunderMaps.Data.Repositories
{
    /// <summary>
    /// Recovery sample repository.
    /// </summary>
    internal class RecoverySampleRepository : RepositoryBase<RecoverySample, int>, IRecoverySampleRepository
    {
        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="dbProvider">Database provider.</param>
        public RecoverySampleRepository(DbProvider dbProvider) : base(dbProvider) { }

        /// <summary>
        /// Create new <see cref="RecoverySample"/>.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        /// <returns>Created <see cref="RecoverySample"/>.</returns>
        public override async ValueTask<int> AddAsync(RecoverySample entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var sql = @"
                INSERT INTO report.recovery_sample(
                    recovery,
                    address,
                    note,
                    status,
                    type,
                    pile_type,
                    contractor,
                    facade,
                    permit,
                    permit_date,
                    recovery_date)
                VALUES (
                    @recovery,
                    @address,
                    @note,
                    @status,
                    @type,
                    @pile_type,
                    @contractor,
                    @facade,
                    @permit,
                    @permit_date,
                    @recovery_date)
                RETURNING id;
            ";

            await using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            await using var cmd = DbProvider.CreateCommand(sql, connection);
            cmd.AddParameterWithValue("recovery", entity.Recovery);
            cmd.AddParameterWithValue("address", entity.Address);
            cmd.AddParameterWithValue("note", entity.Note);
            cmd.AddParameterWithValue("status", entity.Status);
            cmd.AddParameterWithValue("type", entity.Type);
            cmd.AddParameterWithValue("pile_type", entity.PileType);
            cmd.AddParameterWithValue("contractor", entity.Contractor);
            cmd.AddParameterWithValue("facade", entity.Facade);
            cmd.AddParameterWithValue("permit", entity.Permit);
            cmd.AddParameterWithValue("permit_date", entity.PermitDate);
            cmd.AddParameterWithValue("recovery_date", entity.RecoveryDate);
            return await cmd.ExecuteScalarIntAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve number of entities.
        /// </summary>
        /// <returns>Number of entities.</returns>
        public override ValueTask<ulong> CountAsync()
        {
            var sql = @"
                SELECT  COUNT(*)
                FROM    report.recovery_sample";

            return ExecuteScalarUnsignedLongCommandAsync(sql);
        }

        /// <summary>
        /// Delete <see cref="RecoverySample"/>.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        public override async ValueTask DeleteAsync(int id)
        {
            var sql = @"
                DELETE
                FROM    report.recovery_sample
                WHERE   id = @id";

            await using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            await using var cmd = DbProvider.CreateCommand(sql, connection);
            cmd.AddParameterWithValue("id", id);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve <see cref="RecoverySample"/> by id.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <returns><see cref="RecoverySample"/>.</returns>
        public override async ValueTask<RecoverySample> GetByIdAsync(int id)
        {
            var sql = @"
                SELECT  id,
                        recovery,
                        address,
                        create_date,
                        update_date,
                        delete_date,
                        note,
                        status,
                        type,
                        pile_type,
                        contractor,
                        facade,
                        permit,
                        permit_date,
                        recovery_date
                FROM    report.recovery_sample
                WHERE   id = @id
                LIMIT   1";

            await using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            await using var cmd = DbProvider.CreateCommand(sql, connection);
            cmd.AddParameterWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            return new RecoverySample
            {
                Id = reader.GetInt(0),
                Recovery = reader.GetInt(1),
                Address = reader.SafeGetString(2),
                CreateDate = reader.GetDateTime(3),
                UpdateDate = reader.GetSafeDateTime(4),
                DeleteDate = reader.GetSafeDateTime(5),
                Note = reader.SafeGetString(6),
                Status = reader.GetFieldValue<RecoveryStatus>(7),
                Type = reader.GetFieldValue<RecoveryType>(8),
                PileType = reader.GetFieldValue<PileType>(9),
                Contractor = reader.GetFieldValue<Guid?>(10),
                Facade = reader.GetFieldValue<Facade[]>(11),
                Permit = reader.SafeGetString(12),
                PermitDate = reader.GetDateTime(13),
                RecoveryDate = reader.GetDateTime(14),
            };
        }

        /// <summary>
        /// Retrieve all <see cref="RecoverySample"/>.
        /// </summary>
        /// <returns>List of <see cref="RecoverySample"/>.</returns>
        public override async IAsyncEnumerable<RecoverySample> ListAllAsync(INavigation navigation)
        {
            if (navigation == null)
            {
                throw new ArgumentNullException(nameof(navigation));
            }

            var sql = @"
                SELECT  id,
                        recovery,
                        address,
                        create_date,
                        update_date,
                        delete_date,
                        note,
                        status,
                        type,
                        pile_type,
                        contractor,
                        facade,
                        permit,
                        permit_date,
                        recovery_date
                FROM    report.recovery_sample";

            ConstructNavigation(ref sql, navigation);

            await using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            await using var cmd = DbProvider.CreateCommand(sql, connection);

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                yield return new RecoverySample
                {
                    Id = reader.GetInt(0),
                    Recovery = reader.GetInt(1),
                    Address = reader.SafeGetString(2),
                    CreateDate = reader.GetDateTime(3),
                    UpdateDate = reader.GetSafeDateTime(4),
                    DeleteDate = reader.GetSafeDateTime(5),
                    Note = reader.SafeGetString(6),
                    Status = reader.GetFieldValue<RecoveryStatus>(7),
                    Type = reader.GetFieldValue<RecoveryType>(8),
                    PileType = reader.GetFieldValue<PileType>(9),
                    Contractor = reader.GetFieldValue<Guid>(10),
                    Facade = reader.GetFieldValue<Facade[]>(11),
                    Permit = reader.SafeGetString(12),
                    PermitDate = reader.GetDateTime(13),
                    RecoveryDate = reader.GetDateTime(14),
                };
            }
        }

        public override async ValueTask UpdateAsync(RecoverySample entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var sql = @"
                    UPDATE  report.inquiry_sample
                    SET     recovery = @recovery,
                            address = @address,
                            note = @note,
                            status = @status,
                            type = @type,
                            pile_type = @pile_type,
                            contractor = @contractor,
                            facade = @facade,
                            permit = @permit,
                            permit_date = @permit_date,
                            recovery_date = @recovery_date,
                    WHERE   id = @id";

            using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            using var cmd = DbProvider.CreateCommand(sql, connection);
            cmd.AddParameterWithValue("recovery", entity.Recovery);
            cmd.AddParameterWithValue("address", entity.Address);
            cmd.AddParameterWithValue("note", entity.Note);
            cmd.AddParameterWithValue("status", entity.Status);
            cmd.AddParameterWithValue("type", entity.Type);
            cmd.AddParameterWithValue("pile_type", entity.PileType);
            cmd.AddParameterWithValue("contractor", entity.Contractor);
            cmd.AddParameterWithValue("facade", entity.Facade);
            cmd.AddParameterWithValue("permit", entity.Permit);
            cmd.AddParameterWithValue("permit_date", entity.PermitDate);
            cmd.AddParameterWithValue("recovery_date", entity.RecoveryDate);
            cmd.AddParameterWithValue("id", entity.Id);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}