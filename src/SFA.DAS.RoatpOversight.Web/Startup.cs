using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.Extensions.Http;
using Refit;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.RoatpOversight.Domain.Interfaces;
using SFA.DAS.RoatpOversight.Web.Extensions;
using SFA.DAS.RoatpOversight.Web.HealthChecks;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Infrastructure.Handlers;
using SFA.DAS.RoatpOversight.Web.ModelBinders;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.StartupExtensions;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web;

[ExcludeFromCodeCoverage]
public class Startup
{
    private const string Culture = "en-GB";

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public IWebConfiguration ApplicationConfiguration { get; set; }

    public Startup(IConfiguration configuration, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        _env = env;

        var config = new ConfigurationBuilder().AddConfiguration(configuration);

        config.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            }
        );

        _configuration = config.Build();
        ApplicationConfiguration = _configuration.Get<WebConfiguration>();
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
            options.SupportedCultures = new List<CultureInfo> { new CultureInfo(Culture) };
            options.RequestCultureProviders.Clear();
        });

        services.AddMvc(options =>
        {
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            options.ModelBinderProviders.Insert(0, new StringTrimmingModelBinderProvider());
        });

        services.AddValidatorsFromAssembly(typeof(AppealPostRequestValidator).Assembly);

        services.AddSession(opt => { opt.IdleTimeout = TimeSpan.FromHours(1); });

        services.AddCache(ApplicationConfiguration, _env);
        services.AddDataProtection(ApplicationConfiguration, _env);

        services.AddTransient<ICacheStorageService, CacheStorageService>();

        AddAntiforgery(services);

        services.AddApplicationInsightsTelemetry();
        services.AddTelemetryRegistration(_configuration);
        services.AddDasHealthChecks(ApplicationConfiguration, _env.IsDevelopment());
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        ConfigureHttpClients(services);
        ConfigureDependencyInjection(services);
    }

    private void AddAuthentication(IServiceCollection services)
    {
        services.AddAndConfigureDfESignInAuthentication(_configuration,
            "SFA.DAS.AdminService.Web.Auth",
            typeof(CustomServiceRole),
            ClientName.RoatpServiceAdmin,
            "/SignOut",
            "");
    }

    private void AddAntiforgery(IServiceCollection services)
    {
        services.AddAntiforgery(options => options.Cookie = new CookieBuilder() { Name = ".RoatpOversight.Staff.AntiForgery", HttpOnly = false });
    }

    private void ConfigureHttpClients(IServiceCollection services)
    {
        var handlerLifeTime = TimeSpan.FromMinutes(5);

        services.AddRefitClient<IApplyApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(ApplicationConfiguration.ApplyApiAuthentication.ApiBaseAddress))
            .AddHttpMessageHandler(() => new InnerApiAuthenticationHeaderHandler(new AzureClientCredentialHelper(_configuration), ApplicationConfiguration.ApplyApiAuthentication.Identifier))
            .SetHandlerLifetime(handlerLifeTime)
            .AddPolicyHandler(GetRetryPolicy());

        services.AddRefitClient<IRoatpRegisterApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(ApplicationConfiguration.RoatpRegisterApiAuthentication.ApiBaseAddress))
            .AddHttpMessageHandler(() => new InnerApiAuthenticationHeaderHandler(new AzureClientCredentialHelper(_configuration), ApplicationConfiguration.RoatpRegisterApiAuthentication.Identifier))
            .SetHandlerLifetime(handlerLifeTime)
            .AddPolicyHandler(GetRetryPolicy()); ;

        services
           .AddRefitClient<IRoatpOversightOuterApiClient>()
           .ConfigureHttpClient(c => c.BaseAddress = new Uri(ApplicationConfiguration.RoatpOversightOuterApi.BaseUrl))
           .AddHttpMessageHandler(() => new OuterApiAuthenticationHeadersHandler(ApplicationConfiguration.RoatpOversightOuterApi.SubscriptionKey));
    }

    private void ConfigureDependencyInjection(IServiceCollection services)
    {
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

        services.AddTransient(x => ApplicationConfiguration);

        services.AddTransient<IApplicationOutcomeOrchestrator, ApplicationOutcomeOrchestrator>();
        services.AddTransient<IOversightOrchestrator, OversightOrchestrator>();
        services.AddSingleton<IPdfValidatorService, PdfValidatorService>();
        services.AddSingleton<IMultipartFormDataService, MultipartFormDataService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        app.UseRouting();
        app.UseSession();
        app.UseRequestLocalization();
        app.UseStatusCodePagesWithReExecute("/ErrorPage/{0}");
        app.UseSecurityHeaders();
        app.Use(async (context, next) =>
        {
            if (!context.Response.Headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
            {
                context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", new StringValues("none"));
            }
            await next();
        });
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseDasHealthChecks();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
