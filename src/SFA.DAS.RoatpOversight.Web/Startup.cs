using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.Extensions.Http;
using RestEase.HttpClientFactory;
using SFA.DAS.AdminService.Common;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.RoatpOversight.Domain.Interfaces;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.HealthChecks;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using SFA.DAS.RoatpOversight.Web.Infrastructure.Handlers;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.Validators;
using SFA.DAS.Validation.Mvc.Filters;
using SFA.DAS.RoatpOversight.Web.StartupExtensions;
using SFA.DAS.RoatpOversight.Web.ModelBinders;

namespace SFA.DAS.RoatpOversight.Web
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string ServiceName = "SFA.DAS.RoatpOversight";
        private const string Version = "1.0";
        private const string Culture = "en-GB";

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<Startup> _logger;

        public IWebConfiguration ApplicationConfiguration { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            _env = env;
            _logger = logger;
            
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory());
#if DEBUG
            if (!configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddJsonFile("appsettings.json", true)
                    .AddJsonFile("appsettings.Development.json", true);
            }
#endif
            config.AddEnvironmentVariables();

            if (!configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddAzureTableStorage(options =>
                    {
                        options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                        options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                        options.EnvironmentName = configuration["EnvironmentName"];
                        options.PreFixConfigurationKeys = false;
                    }
                );
            }

            _configuration = config.Build();
            ApplicationConfiguration = _configuration.GetSection(nameof(WebConfiguration)).Get<WebConfiguration>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false; // Default is true, make it false
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            AddAuthentication(services);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(Culture);
                options.SupportedCultures = new List<CultureInfo> {new CultureInfo(Culture) };
                options.RequestCultureProviders.Clear();
            });

            services.AddMvc(options =>
            {
                options.Filters.Add<ValidateModelStateFilter>(int.MaxValue);
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.ModelBinderProviders.Insert(0, new StringTrimmingModelBinderProvider());
            })
            // NOTE: Can we move this to 2.2 to match the version of .NET Core we're coding against?
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<OutcomePostRequestValidator>());

            services.AddSession(opt => { opt.IdleTimeout = TimeSpan.FromHours(1); });

            services.AddCache(ApplicationConfiguration, _env);
            services.AddDataProtection(ApplicationConfiguration, _env);

            services.AddTransient<ICacheStorageService, CacheStorageService>();

            AddAntiforgery(services);

            services.AddApplicationInsightsTelemetry();
            services.AddDasHealthChecks(ApplicationConfiguration, _env.IsDevelopment());
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            ConfigureHttpClients(services);
            ConfigureDependencyInjection(services);
        }

        private void AddAuthentication(IServiceCollection services)
        {
            if (ApplicationConfiguration.UseDfeSignIn)
            {
                services.AddAndConfigureDfESignInAuthentication(_configuration,
                    $"{typeof(Startup).Assembly.GetName().Name}.Auth",
                    typeof(CustomServiceRole),
                    ClientName.RoatpServiceAdmin,
                    "/SignOut",
                    "");
            }
            else
            {
                services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = WsFederationDefaults.AuthenticationScheme;
                }).AddWsFederation(options =>
                {
                    options.Wtrealm = ApplicationConfiguration.StaffAuthentication.WtRealm;
                    options.MetadataAddress = ApplicationConfiguration.StaffAuthentication.MetadataAddress;
                    options.TokenValidationParameters.RoleClaimType = Roles.RoleClaimType;
                }).AddCookie();
            }
        }

        private void AddAntiforgery(IServiceCollection services)
        {
            services.AddAntiforgery(options => options.Cookie = new CookieBuilder() { Name = ".RoatpOversight.Staff.AntiForgery", HttpOnly = false });
        }

        private void ConfigureHttpClients(IServiceCollection services)
        {
            var acceptHeaderName = "Accept";
            var acceptHeaderValue = "application/json";
            var handlerLifeTime = TimeSpan.FromMinutes(5);

            services.AddHttpClient<IApplyApiClient, ApplyApiClient>(config =>
            {
                config.BaseAddress = new Uri(ApplicationConfiguration.ApplyApiAuthentication.ApiBaseAddress);
                config.DefaultRequestHeaders.Add(acceptHeaderName, acceptHeaderValue);
            })
            .SetHandlerLifetime(handlerLifeTime)
            .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient<IRoatpRegisterApiClient, RoatpRegisterApiClient>(config =>
            {
                config.BaseAddress = new Uri(ApplicationConfiguration.RoatpRegisterApiAuthentication.ApiBaseAddress);
                config.DefaultRequestHeaders.Add(acceptHeaderName, acceptHeaderValue);
            })
           .SetHandlerLifetime(handlerLifeTime)
           .AddPolicyHandler(GetRetryPolicy());

            AddOuterApi(services, ApplicationConfiguration.RoatpOversightOuterApi);

        }

        private void AddOuterApi(IServiceCollection services, RoatpOversightOuterApi configuration)
        {
            services.AddTransient<IRoatpOversightOuterApi>((_) => configuration);

            services.AddScoped<HeadersHandler>();
            
             services
                .AddRestEaseClient<IRoatpOversightApiClient>(configuration.BaseUrl)
                .AddHttpMessageHandler<HeadersHandler>();
        }

        private void ConfigureDependencyInjection(IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient(x => ApplicationConfiguration);

            services.AddTransient<ISearchTermValidator, SearchTermValidator>();

            services.AddTransient<IRoatpApplicationTokenService, RoatpApplicationTokenService>();
            services.AddTransient<IApplicationOutcomeOrchestrator, ApplicationOutcomeOrchestrator>();
            services.AddTransient<IRoatpRegisterTokenService, RoatpRegisterTokenService>();
            services.AddTransient<IOversightOrchestrator, OversightOrchestrator>();
            services.AddSingleton<IPdfValidatorService, PdfValidatorService>();
            services.AddSingleton<IMultipartFormDataService, MultipartFormDataService>();
          
            DependencyInjection.ConfigureDependencyInjection(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseRequestLocalization();
            app.UseStatusCodePagesWithReExecute("/ErrorPage/{0}");
            app.UseSecurityHeaders();
            app.Use(async (context, next) =>
            {
                if (!context.Response.Headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
                {
                context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", new StringValues("none"));
                }
                await next();
            });
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseDasHealthChecks();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }
    }
}
