using FunderMaps.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace FunderMaps.AspNetCore.Controllers
{
    /// <summary>
    ///     API error handler.
    /// </summary>
    [AllowAnonymous]
    public class OopsController : ControllerBase
    {
        private static readonly Dictionary<Type, HttpStatusCode> _dictionary = new()
        {
            { typeof(AuthenticationException), HttpStatusCode.Unauthorized },
            { typeof(AuthorizationException), HttpStatusCode.Forbidden },
            { typeof(EntityNotFoundException), HttpStatusCode.NotFound },
            { typeof(EntityReadOnlyException), HttpStatusCode.Locked },
            { typeof(InvalidCredentialException), HttpStatusCode.Forbidden },
            { typeof(InvalidIdentifierException), HttpStatusCode.BadRequest },
            { typeof(OperationAbortedException), HttpStatusCode.BadRequest },
            { typeof(ProcessException), HttpStatusCode.InternalServerError },
            { typeof(QueueOverflowException), HttpStatusCode.InternalServerError },
            { typeof(ReferenceNotFoundException), HttpStatusCode.NotFound },
            { typeof(ServiceUnavailableException), HttpStatusCode.NotAcceptable },
            { typeof(StateTransitionException), HttpStatusCode.NotFound },
            { typeof(StorageException), HttpStatusCode.InternalServerError },
            { typeof(UnhandledTaskException), HttpStatusCode.InternalServerError },
        };

        // GET: oops
        /// <summary>
        ///     Returns a <see cref="ProblemDetails"/> from the <see cref="ControllerBase.HttpContext"/>.
        /// </summary>
        /// <returns>Instance of <see cref="ProblemDetails"/>.</returns>
        [Route("oops")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Error([FromServices] IWebHostEnvironment webHostEnvironment, [FromServices] ILogger<OopsController> logger)
        {
            var error = HttpContext.Features.Get<IExceptionHandlerFeature>();

            // If the error message is set and we can find it in our map, return specific problem.
            if (error is not null && _dictionary.ContainsKey(error.Error.GetType()))
            {
                var detail = error.Error.Message;
                var innerException = error.Error.InnerException;
                while (innerException is not null)
                {
                    if (!String.IsNullOrEmpty(innerException.Message)) {
                        detail += $" {innerException.GetType().Name}: {innerException.Message}";
                    }
                    innerException = innerException.InnerException;
                }

                return Problem(
                    title: ((FunderMapsCoreException)error.Error).Title,
                    statusCode: (int)_dictionary.GetValueOrDefault(error.Error.GetType(), HttpStatusCode.InternalServerError),
                    detail: detail);

            }

            logger.LogWarning($"Cannot return configured error message from exception, return generic problem");

            return Problem(
                     title: "Application was unable to process the request.",
                     statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
