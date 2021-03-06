﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Rigger.Extensions;
using IConfigurationSource = Rigger.Configuration.IConfigurationSource;

namespace Drone.Configuration.Sources
{
    /// <summary>
    /// DotNet Core / ASP.NET AppSettings source for the Rigger Configuration Service.
    /// 
    /// </summary>
    public class AppSettingsConfigurationSource : IConfigurationSource
    {
        public static string DEFAULT_APP_SETTINGS_FILENAME = "appsettings.json";

        private String filename;
        /// <summary>
        /// Read an appsettings json configuration file with the name set in
        /// DEFAULT_APP_SETTINGS_FILENAME
        /// </summary>
        public AppSettingsConfigurationSource() : this(DEFAULT_APP_SETTINGS_FILENAME)
        {

        }

        /// <summary>
        /// Read an appsettings json configuration file.
        /// </summary>
        /// <param name="filename">The path and filename of the file to load.</param>
        public AppSettingsConfigurationSource(string filename)
        {
            this.filename = filename;
        }

        public int Priority { get; set; }
        public IEnumerable<string> GetAllKeys() => ConfigurationManager.AppSettings.AllKeys.ToList();

        public Dictionary<string, object> Fetch()
        {
            try
            {
            
                var builder = new ConfigurationBuilder()
                    .AddJsonFile(filename);

                var configuration = builder.Build();

                var dictionary = new Dictionary<string, object>();

                configuration
                    .AsEnumerable()
                    .ForEach(k => dictionary.Add(k.Key, k.Value));

                return dictionary;
            } catch (Exception e)
            {
                return new Dictionary<string, object>();
            }
        }
    }
}