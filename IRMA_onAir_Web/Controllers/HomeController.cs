using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp;
using IrmaWeb.Models.Helper;
using IrmaWeb.Classes;
using IrmaWeb.Classes.Helper;
using IrmaWeb.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IrmaWeb.Controllers
{
    /// <summary>
    /// Main Controller Class
    /// </summary>
    public class HomeController : Controller
    {
        readonly ILogger<HomeController> _log;
        public HomeController(ILogger<HomeController> log)
        {
            _log = log;
        }

        /// <summary>
        /// Returns "Login" ActionResult to Webbrowser
        /// When Session is dead (or never existed) returns HTTP Error code to client (client will redirect application to login)
        /// </summary>
        [AllowAnonymous]
        public ActionResult Index()
        {
            //Faster as a instance prop. that calls at every Action execution
            Claim username = HttpContext.User.FindFirst(ClaimHelper.Username);
            Claim password = HttpContext.User.FindFirst(ClaimHelper.Password);

            // Session is dead/dosen't exist
            if (username == null || password == null)
            {
                _log.LogInformation("Error occured at login try (user/session {0} dosen't exist)", username);
                return View("ApplicationLogin"); // Return normal login View
            }

            try
            {
                AccountRoot accountData;
                if (RuntimeSettings.YamiStatic.Settings.SessionCache)
                {
                    // Load the cached version of MetaData
                    Claim metaData = HttpContext.User.FindFirst(ClaimHelper.MetaData);
                    accountData = JsonConvert.DeserializeObject<AccountRoot>(metaData.Value);
                }
                else
                {
                    // Refresh the MetaData at each call with valid data
                    IrisRestRequest restRequest = new IrisRestRequest(username.Value, password.Value);
                    IRestResponse<AccountRoot> accountDataRaw = restRequest.RequestAccount();

                    if (accountDataRaw.ErrorException == null && accountDataRaw.ErrorMessage == null && accountDataRaw.StatusCode == HttpStatusCode.OK)
                        accountData = JsonConvert.DeserializeObject<AccountRoot>(accountDataRaw.Content);
                    else // Can happen when data changes in database and the cached one is not anymore valid
                        throw new Exception("Cached valid data is not valid anymore!");
                }

                return PartialView("ApplicationMainRedirect", accountData);
            }
            catch (Exception e)
            {
                _log.LogError("Error occured at Session Metadata (user/session - {0}) - {1} - Return no Login screen...", username, e);
                return View("ApplicationLogin");
            }
        }

        /// <summary>
        /// Checks submited POST Data from Login Formular over the REST API. Returns PartialView when everything is okay. If not returns a appropriate HTTP Code.
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public IActionResult Index(AccountModel loginModel)
        {
            if (ModelState.IsValid && WebHelper.IsAjaxRequest(Request))
            {
                if (loginModel.Username == null || loginModel.PasswordPlain == null)
                {
                    _log.LogError("Error occured at login try - login fields can't be empty...");
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }

                IrisRestRequest restRequest = new IrisRestRequest(loginModel.Username, loginModel.PasswordPlain);
                IRestResponse<AccountRoot> accountData = restRequest.RequestAccount();

                if (accountData.ErrorException == null && accountData.ErrorMessage == null)
                {
                    if (accountData.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var claims = new List<Claim>
                                {
                                    new Claim(ClaimHelper.Username, loginModel.Username),
                                    new Claim(ClaimHelper.Password, loginModel.PasswordPlain),
                                };

                            //NOTE: This will not work when it contains too much data (use a real database)
                            if (RuntimeSettings.YamiStatic.Settings.SessionCache)
                                claims.Add(new Claim(ClaimHelper.MetaData, JsonConvert.SerializeObject(accountData.Data)));

                            var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            var authProperties = new AuthenticationProperties();
                            if (loginModel.RememberMe)
                                authProperties.IsPersistent = true;

                            HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);

                            _log.LogInformation("User: {0} - auth. successful!", loginModel.Username);
                        }
                        catch (Exception e)
                        {
                            _log.LogError("Error at auth. routine - {0}", e);
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }
                        return PartialView("ApplicationMain", accountData.Data);
                    }
                    else
                    {
                        _log.LogInformation("Answer recieved for User {0} from REST API, but with an StatusCode {1}",
                            loginModel.Username, accountData.StatusCode);
                        return new StatusCodeResult(StatusCodes.Status403Forbidden);
                    }
                }
                else
                {
                    _log.LogInformation("Some error occured for {0} - Error Code {1} ",
                        loginModel.Username, accountData.StatusCode);
                    _log.LogInformation("ErrorException: {0}", accountData.ErrorException);
                    _log.LogInformation("ErrorMessage: {0} ", accountData.ErrorMessage);
                    _log.LogInformation("Note: Error 503 (ServiceUnavailable) appers when a Connection to the REST Server cannot be established");
                    return new StatusCodeResult((int)accountData.StatusCode);
                }
            }
            else
            {
                _log.LogWarning("{0} has not send an AJAX Post login request (Bad Request, prop. from a BOT)", loginModel.Username);
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>
        /// Evaluates submited Values from WebClient to create a REST Request and return those to user.
        /// </summary>
        /// <param name="userSessionRequest"></param>
        /// <returns></returns>
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult DataRequest([FromBody]RequestModel userSessionRequest)
        {
            if (ModelState.IsValid)
            {
                //Faster as a instance prop. that calls at every Action execution
                Claim username = HttpContext.User.FindFirst(ClaimHelper.Username);
                Claim password = HttpContext.User.FindFirst(ClaimHelper.Password);

                // When Session dead (or never existed) return HTTP Error code to client (client will redirect application to login)
                if (username == null || password == null)
                {
                    _log.LogInformation("Session timeout for a user {0} detected!", userSessionRequest.AccountName);
                    return new StatusCodeResult(StatusCodes.Status408RequestTimeout);
                }

                if (!ModelState.IsValid)
                {
                    _log.LogInformation("Validation error! ModelState is invalid! - Request by {0}", userSessionRequest.AccountName);
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }

                LogHelper.RequestLogModule(_log, userSessionRequest);
                List<RestFileModel> fileCollection = new List<RestFileModel>();
                List<RestErrorModel> fileSkipCollection = new List<RestErrorModel>();

                if (userSessionRequest.DeviceList != null)
                {
                    foreach (String device in userSessionRequest.DeviceList)
                    {
                        int dayLimitValidation = 0;
                        foreach (DateTime pickedDate in userSessionRequest.DateList)
                        {
                            //Skip if more than configured day limit
                            if (dayLimitValidation >= RuntimeSettings.YamiStatic.Settings.DayLimit)
                            {
                                _log.LogError("Validation problem detected for user {0} - Too many days requests, over the limit in settings!", userSessionRequest.AccountName);
                                _log.LogError("Day/s request was stopped at {0}", pickedDate.ToShortDateString());
                                break;
                            }

                            IrisRestRequest restRequest = new IrisRestRequest(username.Value, password.Value);
                            IRestResponse response = restRequest.RequestApcData(
                                userSessionRequest.AccountName, userSessionRequest.RequestType, pickedDate, device);
                            string filename = pickedDate.ToString("yyyy-MM-dd") + "_" + userSessionRequest.AccountName + "_" + device + "." + userSessionRequest.FileExType;

                            if (response == null || response.RawBytes == null || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                LogHelper.DebugLogMethod(_log, response);
                                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
                            }
                            else if (response.StatusCode == HttpStatusCode.OK)
                            {
                                _log.LogTrace("File {0} is OK, add to temp. collection...", filename);
                                fileCollection.Add(new RestFileModel
                                {
                                    Name = filename,
                                    Data = response.RawBytes
                                });
                            }
                            else //Skip file if error in response
                            {
                                _log.LogTrace("File {0} is empty, add to empty file log and skip...", filename);
                                ErrorModel errorModel;
                                try
                                {
                                    errorModel = JsonConvert.DeserializeObject<ErrorRootModel>(response.Content).Error;
                                }
                                catch (Exception e)
                                {
                                    errorModel = new ErrorModel
                                    {
                                        Code = "0",
                                        Description = "Unknown Error",
                                        Message = "Unknown Error"
                                    };
                                    _log.LogCritical("Problem with Rest Error deserialisation... {0}", e);
                                }

                                fileSkipCollection.Add(new RestErrorModel
                                {
                                    Name = filename,
                                    Error = errorModel
                                });
                            }

                            _log.LogTrace("File {0} is completed!", dayLimitValidation);
                            dayLimitValidation++;
                        }
                    }
                }

                _log.LogTrace("Final return blob (application/zip) is ready! Contains {0} files... | {1} files were skipped...",
                    fileCollection.Count, fileSkipCollection.Count);

                //Filename is not needed because of blob download in client (name gets generated there)
                return File(new ZipResponse(fileCollection, fileSkipCollection).GenerateContainer(), "application/zip");
            }
            else
            {
                //Modelstate is invalid
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
        }

        [Authorize]
        public ActionResult Logout()
        {
            HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}