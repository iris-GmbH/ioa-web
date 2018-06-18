using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IrmaWeb.Models;
using RestSharp;

namespace IrmaWeb.Classes.Helper
{
    /// <summary>
    /// Helper Class used for Logging purposes.
    /// </summary>
    public class LogHelper
    {
        private static ILoggerFactory _loggerFactory = null;
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    throw new Exception("Logger is not correctly initialized...");
                }
                return _loggerFactory;
            }
            set { _loggerFactory = value; }
        }

        public static ILogger CreateLogger() => LoggerFactory.CreateLogger("");

        /// <summary>
        /// Special Method that Logs a whole REST Response when its Result is negative (e.x. a Exception occurs)
        /// </summary>
        /// <param name="responseObject"></param>
        public static void DebugLogMethod(ILogger logger, IRestResponse responseObject)
        {
            try
            {
                logger.LogWarning("### Begin Log Entry ###");
                logger.LogWarning("Error Exception: " + responseObject.ErrorException.ToString());
                logger.LogWarning("Error Message: " + responseObject.ErrorMessage.ToString());
                logger.LogWarning("Error StatusDescription: " + responseObject.StatusDescription.ToString());
                logger.LogWarning("Error ResponseStatus: " + responseObject.ResponseStatus.ToString());
                logger.LogWarning("Error StatusCode: " + responseObject.StatusCode.ToString());
                logger.LogWarning("Error ContentType: " + responseObject.ContentType.ToString());
                logger.LogWarning("Error ContentEncoding: " + responseObject.ContentEncoding.ToString());
                logger.LogWarning("Error ContentLength: " + responseObject.ContentLength.ToString());
                logger.LogWarning("Error Content:");
                logger.LogWarning(responseObject.Content.ToString());
                logger.LogWarning("---");
                logger.LogWarning("Error RAW REST Response:");
                logger.LogWarning(System.Text.Encoding.UTF8.GetString(responseObject.RawBytes));
                logger.LogWarning("### End Log Entry ###");
            }
            catch (Exception e) //NullPointerException for example
            {
                logger.LogError("---");
                logger.LogError("Data in responseObject missing!" + e);
                logger.LogError("### End Log Entry ###");
            }
        }

        /// <summary>
        /// Special Method, that Logs every request that was sended from Webbrowser to our Server right before the REST Request will be builded.
        /// </summary>
        /// <param name="RequestData"></param>
        public static void RequestLogModule(ILogger logger, RequestModel RequestData)
        {
            try
            {
                logger.LogInformation("###");
                logger.LogInformation("User " + RequestData.AccountName + " Requested DATA Over POST to REST API");
                logger.LogInformation("RequestType:" + RequestData.RequestType);
                logger.LogInformation("FileExType:" + RequestData.FileExType);

                foreach (var deviceLog in RequestData.DeviceList)
                { logger.LogInformation("Picked Device:" + deviceLog); }

                foreach (var dateLog in RequestData.DateList)
                { logger.LogInformation("Picked Date:" + dateLog); }

                logger.LogInformation("###");
            }
            catch (Exception e) //NullPointerException for example
            {
                logger.LogCritical("Logger at POST Request Crashed? " + e);
            }
        }
    }
}
