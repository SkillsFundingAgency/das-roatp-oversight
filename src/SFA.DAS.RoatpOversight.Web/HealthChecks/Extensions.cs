using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.RoatpOversight.Web.Settings;

namespace SFA.DAS.RoatpOversight.Web.HealthChecks
{
    public static class Extensions
    {
        public static IServiceCollection AddDasHealthChecks(this IServiceCollection services,
            IWebConfiguration config, bool isDevelopment)
        {
            return services;
        }

        public static IApplicationBuilder UseDasHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/ping", new HealthCheckOptions
            {
                Predicate = (_) => false,
                ResponseWriter = (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("");
                }
            });

            return app;
        }
    }
}
