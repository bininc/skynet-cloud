﻿namespace UWay.Skynet.Cloud.Nacos
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Net.Http;
    using UWay.Skynet.Cloud.Nacos.Config;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacos(this IServiceCollection services, Action<NacosClientConfiguration> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.Configure(configure);

            services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });
                
            services.TryAddSingleton<ILocalConfigInfoProcessor, MemoryLocalConfigInfoProcessor>();
            services.AddSingleton<INacosNamingClient, NacosNamingClient>();
            services.AddSingleton<INacosConfigClient, NacosConfigClient>();

            return services;
        }

        public static IServiceCollection AddNacos(this IServiceCollection services, IConfiguration configuration, string sectionName = "nacos")
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<NacosClientConfiguration>(configuration.GetSection(sectionName));            

            services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });
                
            services.TryAddSingleton<ILocalConfigInfoProcessor, MemoryLocalConfigInfoProcessor>();
            services.AddSingleton<INacosConfigClient, NacosConfigClient>();
            services.AddSingleton<INacosNamingClient, NacosNamingClient>();

            return services;
        }

        public static IServiceCollection AddNacos(this IServiceCollection services, Action<NacosClientConfiguration> configure, Action<HttpClient> httpClientAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.Configure(configure);

            services.AddHttpClient(ConstValue.ClientName)
                .ConfigureHttpClient(httpClientAction)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });
                
            services.TryAddSingleton<ILocalConfigInfoProcessor, MemoryLocalConfigInfoProcessor>();
            services.AddSingleton<INacosConfigClient, NacosConfigClient>();
            services.AddSingleton<INacosNamingClient, NacosNamingClient>();

            return services;
        }

        public static IServiceCollection AddNacos(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction, string sectionName = "UWay.Skynet.Cloud.Nacos")
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<NacosClientConfiguration>(configuration.GetSection(sectionName));   

            services.AddHttpClient(ConstValue.ClientName)
                .ConfigureHttpClient(httpClientAction)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });

            services.TryAddSingleton<ILocalConfigInfoProcessor, MemoryLocalConfigInfoProcessor>();
            services.AddSingleton<INacosConfigClient, NacosConfigClient>();
            services.AddSingleton<INacosNamingClient, NacosNamingClient>();


            return services;
        }
    }
}