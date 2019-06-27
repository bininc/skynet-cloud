﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Configuration.NacosServerBase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using UWay.Skynet.Cloud.Nacos;

namespace UWay.Skynet.Cloud.WebCore
{
    public static class NacosConfigurationBuilderExtensionsCore
    {
        
        public static IConfigurationBuilder AddConfigNacosServer(this IConfigurationBuilder configurationBuilder, IHostingEnvironment environment, IHttpClientFactory clientFactory, ILocalConfigInfoProcessor processor, ILoggerFactory logFactory = null)
        {

            if (configurationBuilder == null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }

            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            var settings = new ConfigNacosClientSettings()
            {
                Name = environment.ApplicationName,
                Environment = environment.EnvironmentName
            };


            return configurationBuilder.AddNacosConfigServer(settings, clientFactory, processor,logFactory);
        }

    }
}