using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using CodenameYami.Classes;
using System.IO.Compression;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CodenameYami.Models
{
    /// <summary>
    /// Model for JSON Deserialization
    /// </summary>
    public static class RuntimeSettings
    {
        //Note: No Logger entrys, because the LoggerFactory is initialized after RuntimeSettings usage
        public static Yami YamiStatic { get; set; }
        /// <summary>
        /// Contains the Basic settings of this application
        /// </summary>
        public class Settings
        {

            private int _timeoutREST;
            private int _dayLimit;
            private int _sessionTimeout;

            [JsonProperty("Debug")]
            public bool Debug { get; set; }
            [JsonProperty("Scheme")]
            public string Scheme { get; set; }
            [JsonProperty("Host")]
            public string Host { get; set; }
            [JsonProperty("Port")]
            public string Port { get; set; }
            [JsonProperty("Path")]
            public string Path { get; set; }

            /// <summary>
            /// API Version Identifier
            /// </summary>
            [JsonProperty("Version")]
            public string Version { get; set; }

            /// <summary>
            /// API Release Identifier
            /// </summary>
            [JsonProperty("Release")]
            public string Release { get; set; }

            /// <summary>
            /// ZIP Compression Level Prop. for deserializer. Use GetRestCompressionLevel() to get a enum typed value.
            /// </summary>
            [JsonProperty("ZipCompressionLevel")]
            public string ZipCompressionLevel { get; set; }

            /// <summary>
            /// REST Compression Level Prop. for deserializer. Use GetRestCompressionLevel() to get a enum typed value.
            /// </summary>
            [JsonProperty("RESTCompressionLevel")]
            public string RESTCompressionLevel { get; set; }

            /// <summary>
            /// Timeout value for REST API calls. Force default value (30) when the enterd one is below 0
            /// </summary>
            [JsonProperty("TimeoutREST")]
            public int TimeoutREST
            {
                get { return _timeoutREST; }
                set { if (value < 0) _timeoutREST = 30; else _timeoutREST = value; } //force default value when the enterd one is below 0
            }


            /// <summary>
            /// Day limit for frontend & backend validation. Force default value (31) when the enterd one is below 0
            /// </summary>
            [JsonProperty("DayLimit")]
            public int DayLimit
            {
                get { return _dayLimit; }
                set { if (value < 0) _dayLimit = 31; else _dayLimit = value; } //force default value when the enterd one is below 0
            }

            /// <summary>
            /// Session Timeout value. Force default value (30) when the enterd one is below 0
            /// </summary>
            [JsonProperty("SessionTimeout")]
            public int SessionTimeout
            {
                get { return _sessionTimeout; }
                set { if (value < 0) _sessionTimeout = 30; else _sessionTimeout = value; } //force default value when the enterd one is below 0
            }

            /// <summary>
            /// Enables Session Cache for users (for new vehicle user needs to logoff and login, but has better performance)
            /// </summary>
            [JsonProperty("SessionCache")]
            public bool SessionCache { get; set; }

            /// <summary>
            /// Returns Wanted Compression Level for Rest Requests from Application Settings.
            /// Allowed: Deflate | GZip | None (default value when invalid string)
            /// </summary>
            public DecompressionMethods GetRestCompressionLevel
            {
                get
                {
                    try
                    {
                        return (DecompressionMethods)Enum.Parse(typeof(DecompressionMethods), RESTCompressionLevel);
                    }
                    catch (Exception) //Invalid Value
                    {
                        return DecompressionMethods.None;
                    }

                }
            }

            /// <summary>
            /// Returns Wanted Compression Level for Generated ZIP File from Application Settings.
            /// Allowed: Optimal | Fastest | NoCompression (default value when invalid string)
            /// </summary>
            public CompressionLevel GetZipCompressionLevel
            {
                get
                {
                    try
                    {
                        return (CompressionLevel)Enum.Parse(typeof(CompressionLevel), ZipCompressionLevel);
                    }
                    catch (Exception) //Invalid Value
                    {
                        return CompressionLevel.NoCompression;
                    }

                }
            }
        }

        /// <summary>
        /// Contains extended settings of this application
        /// </summary>
        public class SettingsEx
        {
            /// <summary>
            /// Tracks String for URI Builder
            /// </summary>
            [JsonProperty("User")]
            public string User { get; set; }
            /// <summary>
            /// Stops String for URI Builder
            /// </summary>
            [JsonProperty("Stops")]
            public string Stops { get; set; }
            /// <summary>
            /// Reports String for URI Builder
            /// </summary>
            [JsonProperty("Reports")]
            public string Reports { get; set; }
            /// <summary>
            /// Tracks String for URI Builder
            /// </summary>
            [JsonProperty("Tracks")]
            public string Tracks { get; set; }
        }

        /// <summary>
        /// Root-Element for Config file
        /// </summary>
        public class Yami
        {
            /// <summary>
            /// Basic Settings Container element
            /// </summary>
            [JsonProperty("Settings")]
            public Settings Settings { get; set; }
            /// <summary>
            /// Extended Settings Container element
            /// </summary>
            [JsonProperty("SettingsEx")]
            public SettingsEx SettingsEx { get; set; }
        }

    }
}