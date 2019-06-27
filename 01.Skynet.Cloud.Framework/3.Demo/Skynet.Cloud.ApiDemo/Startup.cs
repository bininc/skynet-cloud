﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UWay.Skynet.Cloud.Data;
using UWay.Skynet.Cloud.Helpers;
using UWay.Skynet.Cloud.Extensions;
using UWay.Skynet.Cloud.IoC;
using UWay.Skynet.Cloud.WebCore;
using UWay.Skynet.Cloud.Mvc;
using Microsoft.AspNetCore.Http;

namespace UWay.Skynet.Cloud.ApiDemo
{


    public class Startup
    {
        public static ILoggerRepository Repository { get; set; }

        private bool userNacos;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            RegistryCookie(services);
            RegistrySwagger(services);
            //
            return InitIoC(services);
        }


        public void RegistryCookie(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void RegistrySwagger(IServiceCollection services)
        {
            services.AddMySwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ConfigureEnviroment(app, env);
            ConfigureSwagger(app);

            ConfigureMvc(app);
        }


        /**
         * Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
         * 
         * */
        public void ConfigureEnviroment(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
        }

        public void ConfigureSwagger(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUi3();
        }

        public void ConfigureMvc(IApplicationBuilder app)
        {
            app.UseMvc();
        }


        /// <summary>
        /// IoC初始化
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private IServiceProvider InitIoC(IServiceCollection services)
        {
            services.UseOracle(Configuration);
            
            return AspectCoreContainer.BuildServiceProvider(services);//接入AspectCore.Injector
        }



       
    }
}