using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using IrmaWeb.Models;
using Microsoft.Extensions.Options;
using static IrmaWeb.Models.RuntimeSettings;
using Serilog;
using Serilog.Events;
using Serilog.Context;
using Microsoft.Extensions.Logging;

namespace IrmaWeb.Classes.Helper
{
    public class WebHelper
    {
        /// <summary>
        /// Determines whether the specified HTTP request is an AJAX request.
        /// </summary>
        /// 
        /// <returns>
        /// true if the specified HTTP request is an AJAX request; otherwise, false.
        /// </returns>
        /// <param name="request">The HTTP request.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> parameter is null (Nothing in Visual Basic).</exception>
        public static bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return false;
        }
    }

    /// <summary>
    /// Middleware Service that is executed at each request
    /// </summary>
    public class HttpMiddleware
    {
        private readonly RequestDelegate next;

        public HttpMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context, ILogger<HttpMiddleware> log)
        {
            if (RuntimeSettings.YamiStatic.Settings != null)
            {
                log.LogInformation("------");

                //Infos a few informations about the user that invokes
                log.LogInformation("User-Agent {@0}", context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown");
                log.LogInformation("Connection state - LocalIpAddress:{0} - RemoteIpAddress:{1}", 
                    context.Connection.LocalIpAddress, context.Connection.RemoteIpAddress);

                await this.next.Invoke(context);

                log.LogInformation("------");
            }
            else
            {
                //Lockup the Application, when the Settings are missing
                //Shouldn't happen, because of hardcoded fallback values
                string message = "Application settings are missing! Fallback couldn't be loaded... Application halted!";

                context.Response.ContentType = "text/html";
                log.LogCritical(message);
                await context.Response.WriteAsync(message);
            }
        }
    }
}