using System;
using System.Collections.Generic;
using System.Text;

namespace Jenmar.FunctionApp.SQLDB.Receive
{
    public class SQLProperties
    {
        public string SqlConnectionString { get; set; }
        public string CommandType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CrmUrl { get; set; }
        public string ClientAppId { get; set; }
        public string ClientSecretId { get; set; }
        public string authority { get; set; }
        public string ApiUrl { get; set; }
        public int DaysToAddOrSub { get; set; }

    }

}
