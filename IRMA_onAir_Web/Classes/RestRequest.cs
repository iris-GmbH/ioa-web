using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;
using IrmaWeb.Models;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using IrmaWeb.Classes;
using IrmaWeb.Classes.Helper;
using IrmaWeb.Models.Helper;
using System.Threading.Tasks;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace IrmaWeb.Classes
{
    // Note to Developers: This Class dosen't contain Deserialisation Requests. 
    // When you try to develop those: don't forget to use following Handler for JSON Serialisation  
    // _client.AddHandler("application/json", NewtonsoftJsonSerializer.Default);

    /// <summary>
    /// Class that handels/Creates REST Requests to a REST API. Depends on Framework RESTSharp!
    /// </summary>
    public class IrisRestRequest
    {
        private int _restTimeout = RuntimeSettings.YamiStatic.Settings.TimeoutREST;
        private DecompressionMethods _restCompressionLevel = RuntimeSettings.YamiStatic.Settings.GetRestCompressionLevel;
        private ILogger _log = LogHelper.CreateLogger();

        private RestUriBuilder _uriBuilder = new RestUriBuilder();
        private IRestClient _client;
        private Uri _uri;

        private string _username;
        private string _password;

        /// <summary>
        /// Creates a instance that is capable to send requests to REST API
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        public IrisRestRequest(string Username, string Password)
        {
            //Optional: Better validation possible here
            if (Username == null || Password == null) throw new ArgumentNullException();
            _username = Username;
            _password = Password;

            _log.LogTrace("Rest request init. a new instance for username: {0}", Username);
        }

        /// <summary>
        /// Initial Connection to REST API Server (Returns Account data informations when valid)
        /// </summary>
        /// <param name="httpauth">TODO</param>
        /// <returns></returns>
        public IRestResponse<AccountRoot> RequestAccount()
        {
            //Account URI
            _uri = _uriBuilder.BuildUri(RuntimeSettings.YamiStatic.SettingsEx.User).Uri;

            _client = new RestClient(_uri)
            {
                Authenticator = new HttpBasicAuthenticator(_username, _password),
                Timeout = (_restTimeout * 1000), //timeouts are in ms, but in config in seconds
                ReadWriteTimeout = (_restTimeout * 1000)
            };

            IRestRequest request = new RestRequest("", Method.POST);
            request.AddDecompressionMethod(_restCompressionLevel);
            IRestResponse<AccountRoot> response = _client.Execute<AccountRoot>(request);

            //When connection cannot be established HttpErrorCode 503 (and not default 0 code)
            if (response.StatusCode == 0) response.StatusCode = HttpStatusCode.ServiceUnavailable;
            //TODO: Response is OK, but invalid content (Okay Http response for example)

            _log.LogDebug("Got a RequestAccount response: {0},{1}", response.ResponseStatus, response.StatusCode);
            return response;
        }

        /// <summary>
        /// Requests Automated Passenger Counter Data over REST API
        /// </summary>
        /// <param name="httpauth"></param>
        public IRestResponse RequestApcData(string account, string type, DateTime date, string device)
        {
            //Excel Reporting is on other URI Namespace in REST API
            if (type == "application/vnd.ms-excel")
                _uri = _uriBuilder.BuildUri(RuntimeSettings.YamiStatic.SettingsEx.Reports).Uri;
            else
                _uri = _uriBuilder.BuildUri(RuntimeSettings.YamiStatic.SettingsEx.Stops).Uri;

            _client = new RestClient(_uri)
            {
                Authenticator = new HttpBasicAuthenticator(_username, _password),
                Timeout = (_restTimeout * 1000), //timeouts are in ms, but in config in seconds
                ReadWriteTimeout = (_restTimeout * 1000)
            };

            IRestRequest request = new RestRequest(account + "?vehicleId=" + device + "&opdate=" + date.ToString("yyyy-MM-dd"), Method.POST);
            request.AddDecompressionMethod(_restCompressionLevel);
            request.AddHeader("Accept", type); //ex. application/json, application/xml or text/csv

            IRestResponse response = _client.Execute(request);

            _log.LogDebug("Got a RequestApcData response: {0},{1},{2}", response.ResponseStatus, response.StatusCode);
            return response;
        }

    }
}