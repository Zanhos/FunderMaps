using FunderMaps.Core.Authentication;
using FunderMaps.Core.Components;
using FunderMaps.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace FunderMaps.AspNetCore
{
    /// <summary>
    ///     ASP.NET Core <see cref="Core.AppContext"/> factory.
    /// </summary>
    public sealed class AspAppContextFactory : AppContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public AspAppContextFactory(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        /// <summary>
        ///     Create the <see cref="Core.AppContext"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <remarks>
        ///     The HTTP context accessor is a singleton provided by the ASP.NET framework. The singleton
        ///     offers access to the <see cref="HttpContext"/> within the current scope. There does *not*
        ///     have to be an active scope, in which case the accessor returns null on the
        ///     <see cref="HttpContext"/> request. If the aforementioned HTTP context accessor is null then
        ///     we'll return an empty <see cref="Core.AppContext"/>.
        /// </remarks>
        public override Core.AppContext Create()
        {
            if (_httpContextAccessor.HttpContext is not HttpContext httpContext)
            {
                return new();
            }

            if (PrincipalProvider.IsSignedIn(httpContext.User))
            {
                var (user, tenant) = PrincipalProvider.GetUserAndTenant<User, Organization>(httpContext.User);
                return new()
                {
                    CancellationToken = httpContext.RequestAborted,
                    Items = new(httpContext.Items),
                    User = user,
                    Tenant = tenant,
                };
            }

            return new()
            {
                CancellationToken = httpContext.RequestAborted,
                Items = new(httpContext.Items)
            };
        }
    }
}
