﻿using FunderMaps.Core.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace FunderMaps.Webservice.Controllers
{
    /// <summary>
    ///     Controller for handling error responses.
    /// </summary>
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public ErrorController(ILogger<ErrorController> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        ///     Returns a <see cref="ProblemDetails"/> based on the <see cref="IExceptionHandlerFeature"/>
        ///     which is present in the current <see cref="ControllerBase.HttpContext"/>.
        /// </summary>
        /// <returns><see cref="ProblemDetails"/></returns>
        [Route("/error")]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IErrorMessage>();

            if (feature == null)
            {
                // If we don't have the feature, log and return a generic problem.
                _logger.LogWarning($"Could not get {nameof(IErrorMessage)} from http context, returning generic problem");
                return Problem(title: "Internal application error", statusCode: (int)HttpStatusCode.InternalServerError);
            }

            return Problem(title: feature.Message, statusCode: feature.StatusCode);
        }
    }
}