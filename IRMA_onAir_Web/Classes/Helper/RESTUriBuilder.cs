using System;
using System.Collections.Generic;

using IrmaWeb.Models;
using Microsoft.Extensions.Logging;

namespace IrmaWeb.Classes.Helper
{
    /// <summary>
    /// Class that is used for Building REST API URIs from Config Values
    /// </summary>
    public class RestUriBuilder
    {
        RuntimeSettings.Yami actualSettings = RuntimeSettings.YamiStatic;
        ILogger _log = LogHelper.CreateLogger();

        /// <summary>
        /// Private Base Method for building Request URIs for the REST API Server. Settings are loaded from Settings file.
        /// </summary>
        /// <returns></returns>
        private UriBuilder BuildUriBase()
        {
            UriBuilder tempURI = new UriBuilder
            {
                Scheme = actualSettings.Settings.Scheme,
                Host = actualSettings.Settings.Host,
                Path = actualSettings.Settings.Path + "/"
            };

            tempURI.Path += actualSettings.Settings.Version + "/" + actualSettings.Settings.Release;

            if (actualSettings.Settings.Port != String.Empty)
            {
                try
                {
                    tempURI.Port = Convert.ToInt32(actualSettings.Settings.Port); //16Bit? Ports are 0-65535
                }
                catch (Exception e)
                {
                    _log.LogWarning("Warning: Port in settings is invalid! Using default..." + e);
                }
            }

            return tempURI;
        }

        /// <summary>
        /// Public Method for building Request URIs for the REST API Server. Settings are loaded from XML Settings file.
        /// </summary>
        /// <returns></returns>
        public UriBuilder BuildUri(string Path)
        {
            UriBuilder tempURI = BuildUriBase();
            tempURI.Path += "/" + Path;

            _log.LogTrace("URI was created:" + tempURI);
            return tempURI;
        }

    }
}