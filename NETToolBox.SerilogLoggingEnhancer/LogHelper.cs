using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NETToolBox.LinuxVersion;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;


namespace NETToolBox.SerilogLoggingEnhancer
{
    internal static class LogHelper<T>
    {
        private static readonly string? _version = typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;

            // Set all the common properties available for every request
            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);
            diagnosticContext.Set("CLRVersion", RuntimeInformation.FrameworkDescription);
            diagnosticContext.Set("RemoteIP", httpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.FirstOrDefault(x => x.Key == "User-Agent").Value);
            diagnosticContext.Set("OSVersion", RuntimeInformation.OSDescription);
            diagnosticContext.Set("ApplicationVersion", _version);
            diagnosticContext.Set("MachineName", Environment.MachineName);
            var userName = request.HttpContext.User.FindFirst("preferred_username");
            if (userName != null)
            {
                diagnosticContext.Set("UserName", userName.Value);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                diagnosticContext.Set("LinuxVersion", GetLinuxVersion.GetLinuxVersionInfo().VersionString);
            }
            // Only set it if available. You're not sending sensitive data in a querystring right?!
            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            // Set the content-type of the Response at this point
            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            // Retrieve the IEndpointFeature selected for the request
            var endpoint = httpContext.GetEndpoint();
            if (endpoint is object) // endpoint != null
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }
    }
}
