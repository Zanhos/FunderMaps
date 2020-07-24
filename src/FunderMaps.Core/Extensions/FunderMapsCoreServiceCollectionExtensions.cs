﻿using FunderMaps.Core.Authentication;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Managers;
using FunderMaps.Core.Services;
using FunderMaps.Core.UseCases;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Provides extension methods for services from this assembly.
    /// </summary>
    public static class FunderMapsCoreServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the core services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddFunderMapsCoreServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // TODO: AddScoped -> Transient?

            // Register core service fillers in DI container.
            services.AddScoped<IFileStorageService, NullFileStorageService>();
            services.AddScoped<IGeocoderService, NullGeocoderService>();
            services.AddScoped<INotificationService, NullNotificationService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // Register core use cases in DI container.
            services.AddScoped<GeocoderUseCase>();
            services.AddScoped<IncidentUseCase>();
            services.AddScoped<InquiryUseCase>();
            services.AddScoped<ProjectUseCase>();
            services.AddScoped<RecoveryUseCase>();

            services.AddScoped<UserManager>();
            services.AddScoped<OrganizationManager>();


            return services;
        }

        // TODO: Want to move this to another assemly?
        public static IServiceCollection AddFunderMapsCoreAuthentication(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddFunderMapsCoreAuthentication(setupAction: null);

            return services;
        }

        // TODO: Want to move this to another assemly?
        public static IServiceCollection AddFunderMapsCoreAuthentication(this IServiceCollection services, Action<AuthenticationOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // TODO: AddScoped -> Transient?s

            services.AddScoped<AuthManager>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
