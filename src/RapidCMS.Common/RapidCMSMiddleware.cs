﻿using System;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RapidCMS.Common.ActionHandlers;
using RapidCMS.Common.Authorization;
using RapidCMS.Common.Data;
using RapidCMS.Common.Helpers;
using RapidCMS.Common.Models;
using RapidCMS.Common.Models.Config;
using RapidCMS.Common.Providers;
using RapidCMS.Common.Services;
using RapidCMS.Common.Services.SidePane;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RapidCMSMiddleware
    {
        public static IServiceCollection AddRapidCMS(this IServiceCollection services, Action<CmsConfig>? config = null)
        {
            var rootConfig = new CmsConfig();
            config?.Invoke(rootConfig);

            if (rootConfig.AllowAnonymousUsage)
            {
                services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
            }

            services.AddSingleton(rootConfig);

            // providers for delivering config objects
            services.AddScoped<ICustomRegistrationProvider, CustomRegistrationProvider>();
            services.AddScoped<ICollectionProvider, CollectionProvider>();
            services.AddScoped<IMetadataProvider, MetadataProvider>();

            //  UI + Repository services
            services.AddTransient<IDataProviderService, DataProviderService>();
            services.AddTransient<IEditContextService, EditContextService>();
            services.AddTransient<IEditorService, EditorService>();
            services.AddTransient<ITreeService, TreeService>();

            // Data exchange services
            services.AddScoped<ISidePaneService, SidePaneService>();
            services.AddScoped<IMessageService, MessageService>();

            // Button handlers
            services.AddTransient<DefaultButtonActionHandler>();
            services.AddTransient(typeof(OpenPaneButtonActionHandler<>));

            // Debug helpers
            services.AddScoped<IExceptionHelper, ExceptionHelper>();

            // Stock data providers
            services.AddSingleton(typeof(EnumDataProvider<>), typeof(EnumDataProvider<>));

            // UI requirements
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();

            services.AddHttpClient();
            services.AddScoped<HttpClient>();

            // Scoped semaphore for repositories
            services.AddScoped(serviceProvider => new SemaphoreSlim(rootConfig.SemaphoreMaxCount, rootConfig.SemaphoreMaxCount));

            services.AddMemoryCache();

            return services;
        }

        public static IApplicationBuilder UseRapidCMS(this IApplicationBuilder app, bool isDevelopment = false)
        {
            app.ApplicationServices.GetService<CmsConfig>().IsDevelopment = isDevelopment;

            return app;
        }
    }
}