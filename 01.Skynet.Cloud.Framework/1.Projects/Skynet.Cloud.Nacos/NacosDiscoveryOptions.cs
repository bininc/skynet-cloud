﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UWay.Skynet.Cloud.Nacos
{
    public class NacosDiscoveryOptions
    {
        public const string NACOS_DISCOVERY_CONFIGURATION_PREFIX = "nacos:discovery";
        private string _hostName;
        private string _hostAddress;
        //private string _scheme = "http";

        public NacosDiscoveryOptions()
        {
            _hostName = ResolveHostName();
            _hostAddress = ResolveHostAddress(_hostName);
        }

        public string Cluster { set; get; }

        //public Dictionary<string, string> Metadata { get; set; }

        //public double Wieght { set; get; }


        /// <summary>
        /// Gets or sets a value indicating whether Nacos Discovery client is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;


        /// <summary>
        /// Gets or sets a value indicating whether Nacos Discovery client is enabled
        /// </summary>
        public bool? Ephemeral { get; set; } = false;



        /// <summary>
        /// Gets or sets a value indicating whether Nacos Discovery client is enabled
        /// </summary>
        public bool? Healthy { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Nacos Discovery client is enabled
        /// </summary>
        public string Namespace { get; set; } = "";


        public string ClusterName { set; get; }


        /// <summary>
        /// Gets or sets Hostname to use when accessing server
        /// </summary>
        public string HostName
        {
            get => PreferIpAddress ? _hostAddress : _hostName;
            set => _hostName = value;
        }


        public string Host { set; get; }

        /// <summary>
        /// Gets or sets IP address to use when accessing service (must also set preferIpAddress to use)
        /// </summary>
        public string IpAddress
        {
            get => _hostAddress;
            set => _hostAddress = value;
        }

        /// <summary>
        /// Gets or sets Port to register the service under (defaults to listening port)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets Use ip address rather than hostname
        /// during registration
        /// </summary>
        public bool PreferIpAddress { get; set; } = false;

        /// <summary>
        /// Gets or sets Service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the instance groupt to use during registration
        /// </summary>
        public int TimeOut { get; set; } = 8000;

        /// <summary>
        /// Gets or sets the instance groupt to use during registration
        /// </summary>
        public string GroupName { get; set; }

        ///// <summary>
        ///// Gets or sets the metadata tag name of the zone
        ///// </summary>
        //public string DefaultZoneMetadataName { get; set; } = "zone";


        /// <summary>
        /// Gets or sets a value indicating whether gets or sets Register as a service in Nacos.
        /// </summary>
        public bool Register { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets Deregister automatic de-registration
        /// of service in Nacos.
        /// </summary>
        public bool Deregister { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets Deregister automatic de-registration
        /// of service in Nacos.
        /// </summary>
        public bool IsSecure { get; set; } = false;



        protected virtual string ResolveHostAddress(string hostName)
        {
            string result = null;
            try
            {
                var results = Dns.GetHostAddresses(hostName);
                if (results != null && results.Length > 0)
                {
                    foreach (var addr in results)
                    {
                        if (addr.AddressFamily.Equals(AddressFamily.InterNetwork))
                        {
                            result = addr.ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            return result;
        }

        protected virtual string ResolveHostName()
        {
            string result = null;
            try
            {
                result = Dns.GetHostName();
                if (!string.IsNullOrEmpty(result))
                {
                    var response = Dns.GetHostEntry(result);
                    if (response != null)
                    {
                        return response.HostName;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            return result;
        }
    }
}