using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodenameYami.Models
{
    /// <summary>
    /// Model for the returned file by Rest Api
    /// </summary>
    public class RestFileModel
    {
        public byte[] Data { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Used by json deserializer for rest api error model
    /// </summary>
    public class ErrorRootModel
    {
        public ErrorModel Error { get; set; }
    }

    /// <summary>
    /// Rest Api Error Model entry
    /// </summary>
    public class ErrorModel
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Wrapper that contains the name of picked device and the rest api error model
    /// </summary>
    public class RestErrorModel
    {
        public string Name { get; set; }
        public ErrorModel Error { get; set; }
    }

}