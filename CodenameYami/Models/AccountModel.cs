using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;
using CodenameYami.Models.Helper;

namespace CodenameYami.Models
{
    /// <summary>
    /// Class, that contains Models, that is used at the Login process.
    /// </summary>
    public class AccountModel
    {
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string PasswordPlain { get; set; }
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Part of IRMA onAir REST API model
    /// </summary>
    public class AccountRoot
    {
        [JsonProperty("mydevices")]
        public MyDevices MyDevices { get; set; }
    }

    /// <summary>
    /// Part of IRMA onAir REST API model
    /// </summary>
    public class MyDevices
    {
        [JsonProperty("operators")]
        public Operators Operators { get; set; }
    }

    /// <summary>
    /// Part of IRMA onAir REST API model
    /// </summary>
    public class Operators
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("shortName")]
        public string ShortName { get; set; }
        [JsonProperty("longName")]
        public string LongName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }
        [JsonProperty("devices")]
        [JsonConverter(typeof(SingleOrArrayConverter<Devices>))]
        public List<Devices> Devices { get; set; }
    }

    /// <summary>
    /// Part of IRMA onAir REST API model
    /// </summary>
    public class Devices
    {
        [JsonProperty("serialnumber")]
        public string SerialNumber { get; set; }
        [JsonProperty("vehicleid")]
        public string VehicleId { get; set; }
    }

}