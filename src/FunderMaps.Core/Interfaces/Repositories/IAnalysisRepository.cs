﻿using FunderMaps.Core.Types.Products;
using System.Threading.Tasks;

namespace FunderMaps.Core.Interfaces.Repositories
{
    /// <summary>
    ///     Operations for the analysis repository.
    /// </summary>
    public interface IAnalysisRepository
    {
        /// <summary>
        ///     Gets an analysis product by its internal building id.
        /// </summary>
        /// <param name="id">Internal building id.</param>
        Task<AnalysisProduct> GetByIdAsync(string id);

        /// <summary>
        ///     Gets an analysis product by its internal building id.
        /// </summary>
        /// <param name="id">Internal building id.</param>
        Task<AnalysisProduct2> GetById2Async(string id);

        /// <summary>
        ///     Gets an analysis product by its external building id.
        /// </summary>
        /// <param name="id">External building id.</param>
        Task<AnalysisProduct> GetByExternalIdAsync(string id);

        /// <summary>
        ///     Gets an analysis product by its external building id.
        /// </summary>
        /// <param name="id">External building id.</param>
        Task<AnalysisProduct2> GetByExternalId2Async(string id);

        /// <summary>
        ///     Gets an analysis product by its external address id.
        /// </summary>
        /// <param name="id">External address id.</param>
        Task<AnalysisProduct> GetByAddressExternalIdAsync(string id);

        /// <summary>
        ///     Gets an analysis product by its external address id.
        /// </summary>
        /// <param name="id">External address id.</param>
        Task<AnalysisProduct2> GetByAddressExternalId2Async(string id);

        /// <summary>
        ///     Gets the risk index by its internal building id.
        /// </summary>
        /// <param name="id">Internal building id.</param>
        Task<bool> GetRiskIndexByIdAsync(string id);

        /// <summary>
        ///     Gets the risk index by its external building id and source.
        /// </summary>
        /// <param name="id">Internal building id.</param>
        Task<bool> GetRiskIndexByExternalIdAsync(string id);

        /// <summary>
        ///     Gets the risk index by its external building id and source.
        /// </summary>
        /// <param name="id">Internal building id.</param>
        Task<bool> GetRiskIndexByAddressExternalIdAsync(string id);
    }
}
