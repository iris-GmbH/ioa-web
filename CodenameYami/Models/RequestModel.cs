using System;
using System.Collections.Generic;

namespace CodenameYami.Models
{
    /// <summary>
    /// Data Structure that is used when a logged in user sends data back to server over POST from main UI
    /// </summary>
    public class RequestModel
    {
        public List<DateTime> DateList { get; set; }
        public List<string> DeviceList { get; set; }
        public string AccountName { get; set; }
        public string FileExType { get; set; }
        public string RequestType { get; set; }
    }
}