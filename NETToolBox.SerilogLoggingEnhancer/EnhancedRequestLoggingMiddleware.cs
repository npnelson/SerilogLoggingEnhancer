using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Threading.Tasks;

namespace NETToolBox.SerilogLoggingEnhancer
{
    public class EnhancedRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticContext _diagnosticContext;
        private readonly string _envName;
        public EnhancedRequestLoggingMiddleware(RequestDelegate next, IWebHostEnvironment env, IDiagnosticContext diagnosticContext)
        {
            _diagnosticContext = diagnosticContext;
            _envName = env.EnvironmentName;
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            _diagnosticContext.Set("EnvironmentName", _envName);
            await _next(httpContext);
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class EnhancedRequestLoggingMiddlewareExtensions
    {
        /// <summary>
        /// Adds several opinionated properties to the SerilogRequestLoggingMiddleware
        /// </summary>
        /// <typeparam name="T">Any type from the assembly version that to be reported in ApplicationVersion, typically the type of your startup class</typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseEnhancedRequestLogging<T>(this IApplicationBuilder builder)
        {
            return builder.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogHelper<T>.EnrichFromRequest).UseMiddleware<EnhancedRequestLoggingMiddleware>();
        }

    }
}
