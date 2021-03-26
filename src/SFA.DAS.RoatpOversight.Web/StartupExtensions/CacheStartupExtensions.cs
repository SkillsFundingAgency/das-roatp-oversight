﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.RoatpOversight.Web.Settings;

namespace SFA.DAS.RoatpOversight.Web.StartupExtensions
{
    public static class CacheStartupExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IWebConfiguration configuration, IHostingEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.SessionRedisConnectionString;
                });
            }

            return services;
        }
    }
}
