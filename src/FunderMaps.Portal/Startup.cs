using FunderMaps.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: ApiController]
namespace FunderMaps.Portal
{
    /// <summary>
    ///     Application configuration.
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="configuration">See <see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration configuration) => Configuration = configuration;

        /// <summary>
        ///     Use this method to add services to the container regardless of the environment.
        /// </summary>
        /// <remarks>
        ///     Order is undetermined when configuring services.
        /// </remarks>
        /// <param name="services">See <see cref="IServiceCollection"/>.</param>
        private void StartupConfigureServices(IServiceCollection services)
        {
            // Register components from reference assemblies.
            services.AddFunderMapsInfrastructureServices();
            services.AddFunderMapsDataServices("FunderMapsConnection");
        }

        /// <summary>
        ///     This method gets called by the runtime if no environment is set.
        /// </summary>
        /// <param name="services">See <see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            StartupConfigureServices(services);
        }

        /// <summary>
        ///     This method gets called by the runtime if environment is set to development.
        /// </summary>
        /// <param name="services">See <see cref="IServiceCollection"/>.</param>
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            StartupConfigureServices(services);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowAnyOrigin();
                });
            });
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP
        ///     request pipeline if environment is set to development.
        /// </summary>
        /// <remarks>
        ///     The order in which the pipeline handles request is of importance.
        /// </remarks>
        public static void ConfigureDevelopment(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseCors();

            app.UseFunderMapsExceptionHandler("/oops");

            app.UsePathBase(new("/api"));
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP
        ///     request pipeline if no environment is set.
        /// </summary>
        /// <remarks>
        ///     The order in which the pipeline handles request is of importance.
        /// </remarks>
        public static void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            app.UseExceptionHandler("/oops");

            app.UseFunderMapsExceptionHandler("/oops");

            app.UsePathBase(new("/api"));
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health").WithMetadata(new AllowAnonymousAttribute());
            });
        }
    }
}
