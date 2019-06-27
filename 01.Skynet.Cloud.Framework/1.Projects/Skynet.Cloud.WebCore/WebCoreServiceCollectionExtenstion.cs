﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UWay.Skynet.Cloud.Data;
using UWay.Skynet.Cloud.Extensions;
using UWay.Skynet.Cloud.Protocal;
using UWay.Skynet.Cloud;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.CloudFoundry.Connector;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.MySql;
using System.Data.Common;
using Steeltoe.CloudFoundry.Connector.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace UWay.Skynet.Cloud.WebCore
{
    public static class  WebCoreServiceCollectionExtenstion
    {
        private static readonly string SKYNET = "skynet";
        private static readonly string SKYNET_CLOUD = "cloud";
        private static readonly string SKYNET_CLOUD_SERVICE_DB = "db";
        private static readonly string SKYNET_CLOUD_SERVICE_ROUTE = "route";
        private static readonly string SKYNET_CLOUD_SERVICE_INTERFACE = "interface";
        private static readonly string SKYNET_CLOUD_SERVICE_IMPL = "impl";
        private static readonly string SKYNET_CLOUD_SERVICE_ENTITY = "entity";

        public static IServiceCollection UseOracle(this IServiceCollection services, IConfiguration config, string serviceName=null)
        {
            ILoggerFactory loggerFactory = services.BuildAspectCoreServiceProvider().GetService<ILoggerFactory>();
            AddDataBaseInfo(config, "DB_Oracle_ConnStr", DbProviderNames.Oracle_Managed_ODP, serviceName, loggerFactory);
            return RegistryService(config, services);
        }



        private static void AddDataBaseInfo(IConfiguration config, string containerName, string providerName, string serviceName = null, ILoggerFactory loggerFactory = null)
        {
            var section = config.GetSection(SKYNET).GetSection(SKYNET_CLOUD);
            var useMainDb = section.GetValue<string>(SKYNET_CLOUD_SERVICE_DB,"upms");
            var useDbRoute = section.GetValue<bool>(SKYNET_CLOUD_SERVICE_ROUTE, false);
            var moduleAssmbly = section.GetValue<string>(SKYNET_CLOUD_SERVICE_ENTITY, "Skynet.Cloud.Framework");
            //"DB_Oracle_ConnStr"
            var dbConnectionString = config.GetConnectionString(containerName);
            if (dbConnectionString.IsNullOrEmpty())
            {
                switch(providerName)
                {
                    case DbProviderNames.Oracle:
                        dbConnectionString = BuildeOracleConnectionString(config, serviceName);
                        break;
                    case DbProviderNames.MySQL:
                        dbConnectionString = BuildeMysqlConnectionString(config, serviceName);
                        break;
                    case DbProviderNames.SqlServer:
                        dbConnectionString = BuildeSqlServerConnectionString(config, serviceName);
                        break;
                }    
            }

            if (!string.IsNullOrEmpty(useMainDb))
            {
                AddMainDb(containerName, dbConnectionString, providerName, moduleAssmbly, loggerFactory);
            }

            if (useDbRoute)
            {
                AddRouteDb(containerName, dbConnectionString, providerName, moduleAssmbly, loggerFactory);
            }

        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = new PathString("/Sys/User/Login");
                options.AccessDeniedPath = new PathString("/Error/NoAuth");
                options.LogoutPath = new PathString("/Sys/User/LogOut");
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
            });

            return services;
        }

        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs));
            services.AddMvc(option => option.Filters.Add(typeof(HttpGlobalExceptionFilter))).AddJsonOptions(op => op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver());//修改默认首字母为大写
            services.AddMemoryCache();
            services.AddSession();
            return services;
        }

        private static void AddMainDb(string container, string dbConnectionString, string providerName, string entityAssmbly, ILoggerFactory loggerFactory = null)
        {
            //var entityAssmbly = config.GetSection("appSettings").GetValue<string>("ENTITY_ASSMBLY");
            var dbContextOption = GetMainDbContextOption(new DbContextOption
            {
                Container = container,
                ConnectionString = dbConnectionString,
                //ModuleAssemblyName = entityAssmbly,
                Provider = providerName,
                LogggerFactory = loggerFactory
            });
            //dbContextOption.ModuleAssemblyName = entityAssmbly;
            DbConfiguration.Configure(dbContextOption);
            
        }

        private static void AddRouteDb(string defaultContainer, string defaultDbConnectionString, string defaultProviderName, string defaultEntityAssmbly, ILoggerFactory loggerFactory = null)
        {
            var list = GetRouteDbContextOptions(new DbContextOption
            {
                Container = defaultContainer,
                ConnectionString = defaultDbConnectionString,
                //ModuleAssemblyName = defaultEntityAssmbly,
                Provider = defaultProviderName,
                LogggerFactory = loggerFactory
            });

            if (list.Count() > 0)
            {
                //list.ForEach(p => { p.ModuleAssemblyName = defaultEntityAssmbly; });
                DbConfiguration.Configure(list);
            }
        }

        private static string BuildeOracleConnectionString(IConfiguration config, string serviceName = null)
        {
            OracleServiceInfo oracleServiceInfo = string.IsNullOrEmpty(serviceName)
                  ? config.GetSingletonServiceInfo<OracleServiceInfo>()
                  : config.GetRequiredServiceInfo<OracleServiceInfo>(serviceName);
            return BuildOracleODPConnectionString(oracleServiceInfo.Host, oracleServiceInfo.Port, oracleServiceInfo.Path, oracleServiceInfo.UserName, oracleServiceInfo.Password, true, 10, 20);
        }

        private static string BuildeMysqlConnectionString(IConfiguration config, string serviceName = null)
        {
            MySqlServiceInfo info = string.IsNullOrEmpty(serviceName)
                    ? config.GetSingletonServiceInfo<MySqlServiceInfo>()
                    : config.GetRequiredServiceInfo<MySqlServiceInfo>(serviceName);
            MySqlProviderConnectorOptions mySqlConfig = new MySqlProviderConnectorOptions(config);
            MySqlProviderConnectorFactory factory = new MySqlProviderConnectorFactory(info, mySqlConfig, null);
            return factory.CreateConnectionString();            
        }

        private static string BuildeSqlServerConnectionString(IConfiguration config, string serviceName = null)
        {
            SqlServerServiceInfo info = string.IsNullOrEmpty(serviceName)
           ? config.GetSingletonServiceInfo<SqlServerServiceInfo>()
           : config.GetRequiredServiceInfo<SqlServerServiceInfo>(serviceName);

            SqlServerProviderConnectorOptions sqlServerConfig = new SqlServerProviderConnectorOptions(config);

            SqlServerProviderConnectorFactory factory = new SqlServerProviderConnectorFactory(info, sqlServerConfig, null);
            var dbConnectionString = factory.CreateConnectionString();
            return factory.CreateConnectionString();
        }

        public static IServiceCollection UseMysql(this IServiceCollection services, IConfiguration config, string serviceName = null)
        {
            ILoggerFactory loggerFactory = services.BuildAspectCoreServiceProvider().GetService<ILoggerFactory>();
           
            AddDataBaseInfo(config, "DB_MySql_ConnStr", DbProviderNames.MySQL, serviceName, loggerFactory);
            return RegistryService(config, services);
        }


        private static IServiceCollection RegistryService(IConfiguration config, IServiceCollection services)
        {
            var section = config.GetSection(SKYNET).GetSection(SKYNET_CLOUD);
            var serviceInterfaceAssmbly = section.GetValue<string>(SKYNET_CLOUD_SERVICE_INTERFACE);
            var serviceImplAssembly = section.GetValue<string>(SKYNET_CLOUD_SERVICE_IMPL);
            if (!serviceInterfaceAssmbly.IsNullOrEmpty() && !serviceImplAssembly.IsNullOrEmpty())
                services
                    .AddScopedAssembly(serviceInterfaceAssmbly, serviceImplAssembly);
            return services;
        }
        

        public static IServiceCollection UseSqlServer(this IServiceCollection services, IConfiguration config, string serviceName = null)
        {
            ILoggerFactory loggerFactory = services.BuildAspectCoreServiceProvider().GetService<ILoggerFactory>();

            AddDataBaseInfo(config, "DB_SqlServer_ConnStr", DbProviderNames.SqlServer, serviceName, loggerFactory);

            return RegistryService(config, services);
        }

        private static DbContextOption GetMainDbContextOption(DbContextOption dbContextOption)
        {
            
            using (var connContext = new ProtocolDbContext(dbContextOption))
            {
                var containerName = "xxx";
                var conn1 = connContext.Set<ProtocolInfo>().SingleOrDefault(p => p.Description == containerName);
                //var conn2 = connContext.Set<ProtocolInfo>().SingleOrDefault(p => p.ContainerName== containerName);
                var conn = connContext.Set<ProtocolInfo>().Where(p => p.ProtocalType == ProtocalType.DB && p.ContainerName.Equals("upms")).ToList();

                if (conn != null && conn.Count > 0)
                {
                    var item = conn.FirstOrDefault();
                    var connDetail = connContext.Set<ProtocolCfgInfo>().Get(item.CfgID);
                    //var connDetail = details.FirstOrDefault(p => p.CfgID == item.CfgID);
                    if (connDetail != null)
                    {

                        if (connDetail.ProviderName == DbProviderNames.Oracle && !string.IsNullOrWhiteSpace(connDetail.ServerName))
                        {
                            connDetail.Driver = DbProviderNames.Oracle_Managed_ODP;
                            item.DataBaseName = connDetail.ServerName;
                        }
                        var connectStrings = dbContextOption.ConnectionString;
                        if (connDetail.ProviderName == DbProviderNames.Oracle)
                            connectStrings = BuildOracleClientConnectionString(item.DataBaseName, connDetail.DesUserID, connDetail.DesPassword, item.IsConnPool, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                        else if (connDetail.ProviderName == DbProviderNames.Oracle_Managed_ODP)
                            connectStrings = BuildOracleManagedODPConnectionString(connDetail.Url, connDetail.Port, item.DataBaseName, connDetail.DesUserID, connDetail.DesPassword, item.IsConnPool, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                        else if (connDetail.ProviderName == DbProviderNames.Oracle_ODP)
                            connectStrings = BuildOracleODPConnectionString(connDetail.Url, connDetail.Port, item.DataBaseName, connDetail.DesUserID, connDetail.DesPassword, item.IsConnPool, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                        else if (connDetail.ProviderName == DbProviderNames.SqlServer)
                            connectStrings = BuildSqlServerConnectionString(connDetail.Url, connDetail.DesUserID, connDetail.DesPassword, item.DataBaseName, connDetail.Port);
                        else if (connDetail.ProviderName == DbProviderNames.MySQL)
                            connectStrings = BuildMySqlConnectionString(connDetail.Url, connDetail.DesUserID, connDetail.DesPassword, item.DataBaseName, connDetail.Port, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                        return new DbContextOption()
                        {
                            Container = "upms",
                            Provider = connectStrings,
                            ConnectionString = connDetail.ProviderName,
                            ModuleAssemblyName = dbContextOption.ModuleAssemblyName,
                            MappingFile = dbContextOption.MappingFile,
                            LogggerFactory = dbContextOption.LogggerFactory
                        };

                    }
                }

                return new DbContextOption()
                {
                    Container = "upms",
                    Provider = dbContextOption.Provider,
                    ConnectionString = dbContextOption.ConnectionString,
                    ModuleAssemblyName = dbContextOption.ModuleAssemblyName,
                    MappingFile = dbContextOption.MappingFile,
                    LogggerFactory = dbContextOption.LogggerFactory
                };

            }

        }

        private static IEnumerable<DbContextOption> GetRouteDbContextOptions(DbContextOption dbContextOption)
        {
            IDictionary<string, DbContextOption> dbcs = GetDefRouteDbContextOptions(dbContextOption);
            if (dbcs.Count > 0)
            {
                using (var connContext = new ProtocolDbContext(dbContextOption))
                {
                    var conn = connContext.Set<ProtocolInfo>().Where(p => p.ProtocalType == ProtocalType.DB && dbcs.Keys.Contains(p.ContainerName)).ToList();
                    List<int> cfgIds = conn.Select(p => p.CfgID).ToList();
                    var details = connContext.Set<ProtocolCfgInfo>().Where(p => cfgIds.Contains(p.CfgID)).ToList();
                    if (conn != null && conn.Count > 0)
                    {
                        foreach (var item in conn)
                        {
                            if (details != null && details.Count > 0)
                            {
                                var connDetail = details.FirstOrDefault(p => p.CfgID == item.CfgID);
                                if (connDetail != null)
                                {

                                    if (connDetail.ProviderName == DbProviderNames.Oracle && !string.IsNullOrWhiteSpace(connDetail.ServerName))
                                    {
                                        connDetail.Driver = DbProviderNames.Oracle_Managed_ODP;
                                        item.DataBaseName = connDetail.ServerName;
                                    }
                                    var connectStrings = dbContextOption.ConnectionString;
                                    if (connDetail.ProviderName == DbProviderNames.Oracle)
                                        connectStrings = BuildOracleClientConnectionString(item.DataBaseName, connDetail.DesUserID, connDetail.DesPassword, item.IsConnPool, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                                    else if (connDetail.ProviderName == DbProviderNames.Oracle_Managed_ODP)
                                        connectStrings = BuildOracleManagedODPConnectionString(connDetail.Url, connDetail.Port, item.DataBaseName, connDetail.DesUserID, connDetail.DesPassword, item.IsConnPool, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                                    else if (connDetail.ProviderName == DbProviderNames.Oracle_ODP)
                                        connectStrings = BuildOracleODPConnectionString(connDetail.Url, connDetail.Port, item.DataBaseName, connDetail.DesUserID, connDetail.DesPassword, item.IsConnPool, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                                    else if (connDetail.ProviderName == DbProviderNames.SqlServer)
                                        connectStrings = BuildSqlServerConnectionString(connDetail.Url, connDetail.DesUserID, connDetail.DesPassword, item.DataBaseName, connDetail.Port);
                                    else if (connDetail.ProviderName == DbProviderNames.MySQL)
                                        connectStrings = BuildMySqlConnectionString(connDetail.Url, connDetail.DesUserID, connDetail.DesPassword, item.DataBaseName, connDetail.Port, connDetail.CONNET_POOL_MAXACTIVE, connDetail.CONNET_POOL_MAXIDLE);
                                    if (dbcs.ContainsKey(item.ContainerName))
                                    {
                                        dbcs[item.ContainerName].ConnectionString = connectStrings;
                                        dbcs[item.ContainerName].Provider = connDetail.ProviderName;
                                    }
                                    else
                                    {
                                        var key = dbcs.Keys.FirstOrDefault(p => p.StartsWith(item.ContainerName));
                                        if (!string.IsNullOrEmpty(key))
                                        {
                                            dbcs[key].ConnectionString = connectStrings;
                                            dbcs[key].Provider = connDetail.ProviderName;
                                            dbcs[key].LogggerFactory = dbContextOption.LogggerFactory;
                                        }
                                    }


                                }
                            }
                        }


                    }
                }
            }


            return dbcs.Values;

        }

        private static IDictionary<string, DbContextOption> GetDefRouteDbContextOptions(DbContextOption dbContextOption)
        {
            IDictionary<string, DbContextOption> dbcs = new Dictionary<string, DbContextOption>();

            foreach (NetType item in Enum.GetValues(typeof(NetType)))
            {
                foreach (DataBaseType db in Enum.GetValues(typeof(DataBaseType)))
                {
                    string key = db == DataBaseType.Normal ? string.Format("{0}", (int)item) : string.Format("{0}_{1}", (int)item, (int)db);
                    if (!dbcs.ContainsKey(key))
                    {
                        string mappFile = GetMappingFile(item, db);
                        if (!mappFile.IsNullOrEmpty())
                        {
                            dbcs.Add(key, new DbContextOption()
                            {
                                Container = key,
                                Provider = dbContextOption.Provider,
                                ConnectionString = dbContextOption.ConnectionString,
                                MappingFile = GetMappingFile(item, db),
                                ModuleAssemblyName = dbContextOption.ModuleAssemblyName

                            });
                        }

                    }
                }

            }
            return dbcs;
        }

        #region
        private static string GetMappingFile(NetType netType, DataBaseType dataBaseType)
        {

            var path = System.AppDomain.CurrentDomain.BaseDirectory ?? AppDomain.CurrentDomain.RelativeSearchPath;
            var files = Directory.EnumerateFiles(path, string.Format("uway.{0}.{1}.mapping.xml", netType, dataBaseType));
            if (files.Any())
                return string.Format("uway.{0}.{1}.mapping.xml", netType, dataBaseType);
            else
                return string.Empty;
        }


        private static string BuildOracleClientConnectionString(string datasource, string uid, string pwd, bool IsConnPool, int CONNET_POOL_MAXACTIVE, int CONNET_POOL_MAXIDLE)
        {
            return string.Format("data source={0};user id={1};password={2};Pooling={3};Max Pool Size={4};Min Pool Size={5};", datasource, uid, pwd, IsConnPool, CONNET_POOL_MAXIDLE, CONNET_POOL_MAXACTIVE);
        }

        private static string BuildOracleManagedODPConnectionString(string ip, int port, string datasource, string uid, string pwd, bool IsConnPool, int CONNET_POOL_MAXACTIVE, int CONNET_POOL_MAXIDLE)
        {
            return string.Format("data source={0}:{1}/{2};user id={3};password={4};Pooling={5};Max Pool Size={6};Min Pool Size={7};", ip, port, datasource, uid, pwd, IsConnPool, CONNET_POOL_MAXACTIVE, CONNET_POOL_MAXIDLE);
        }

        private static string BuildOracleODPConnectionString(string ip, int port, string datasource, string uid, string pwd, bool IsConnPool, int CONNET_POOL_MAXACTIVE, int CONNET_POOL_MAXIDLE)
        {
            return string.Format(@"(DESCRIPTION =(ADDRESS_LIST =
                                    (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1}))
                                    )
                                    (CONNECT_DATA =
                                    (SERVICE_NAME = {2})
                                   )
                                    );Persist Security Info=True;User ID={3};Password={4};;Pooling={5};Max Pool Size={6};Min Pool Size={7};", ip,
                                    port,
                                     datasource,
                                     uid, pwd, IsConnPool, CONNET_POOL_MAXIDLE, CONNET_POOL_MAXACTIVE);
        }
        #endregion

        #region
        private static string BuildMySqlConnectionString(string url, string uid, string pwd, string database, int port, int CONNET_POOL_MAXACTIVE, int CONNET_POOL_MAXIDLE)
        {
            return string.Format("server = {0}; User Id = {1}; password = {2}; database = {3}; port = {4}; Charset = utf8; Persist Security Info = True",
                url, uid, pwd, database, port);
        }

        private static string BuildSqlServerConnectionString(string url, string uid, string pwd, string database, int port)
        {
            return string.Format("Data Source = {0};Port={4};Initial Catalog = {1};User ID = {2}; Password={3};Integrated Security =false",
                url, database, uid, pwd, port);
        }


        private static string BuildSqlServerConnectionString(string url, string uid, string pwd, string database)
        {
            return string.Format("Data Source = {0};Initial Catalog = {1};User ID = {2}; Password={3};Integrated Security =false",
                url, database, uid, pwd);
        }
        #endregion


    }
}