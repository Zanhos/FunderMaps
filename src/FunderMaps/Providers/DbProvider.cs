﻿using Dapper;
using FunderMaps.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace FunderMaps.Providers
{
    /// <summary>
    /// Database provider.
    /// </summary>
    public class DbProvider
    {
        private readonly DbProviderOptions _options;
        private readonly string connectionString;

        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Static initializer.
        /// </summary>
        static DbProvider()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="options">Configuration options.</param>
        public DbProvider(IConfiguration configuration, IOptions<DbProviderOptions> options)
        {
            Configuration = configuration;
            _options = options?.Value;
            connectionString = Configuration.GetConnectionStringFallback(_options.ConnectionStringName);
        }

        /// <summary>
        /// Create a new connection instance.
        /// </summary>
        /// <returns><see cref="IDbConnection"/> instance.</returns>
        public IDbConnection ConnectionScope() => new NpgsqlConnection(connectionString);
    }
}
