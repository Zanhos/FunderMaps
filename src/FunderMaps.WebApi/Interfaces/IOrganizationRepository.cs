﻿using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunderMaps.Interfaces
{
    /// <summary>
    /// Organization repository.
    /// </summary>
    public interface IOrganizationRepository : IAsyncRepository<Organization, Guid>
    {
        /// <summary>
        /// Retrieve entity by name.
        /// </summary>
        /// <param name="name">Organization name.</param>
        /// <returns><see cref="Organization"/> on success, null on error.</returns>
        Task<Organization> GetByNormalizedNameAsync(string name);

        // TODO: Move to somehere else.
        /// <summary>
        /// Retrieve all contractor.
        /// </summary>
        /// <returns>Entities.</returns>
        Task<IReadOnlyList<Organization>> ListAllContractorsAsync(INavigation navigation);
    }
}
