using System;
using System.Data;
using System.Globalization;
using System.Linq;
using Jenmar.FunctionApp.SQLDB.Receive.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Jenmar.FunctionApp.SQLDB.Receive
{
    public static class FUNC_SQLDB_Receive
    {
        private static readonly string _sqlConnectionString;
        private static readonly string _username;
        private static readonly string _password;

        private static readonly string _crmUrl;
        private static readonly string _clientAppId;
        private static readonly string _clientSecretId;
        private static readonly string _scheduleTriggerTime;
        private static readonly string _instrumentationKey;
        private static readonly int _daysToAddOrSub = 0;

        static FUNC_SQLDB_Receive()
        {
            _sqlConnectionString = System.Environment.GetEnvironmentVariable("SqlConnectionString");
            _username = System.Environment.GetEnvironmentVariable("Username");
            _password = System.Environment.GetEnvironmentVariable("Password");
            _crmUrl = System.Environment.GetEnvironmentVariable("CrmUrl");
            _clientAppId = System.Environment.GetEnvironmentVariable("ClientAppId");
            _clientSecretId = System.Environment.GetEnvironmentVariable("ClientSecretId");
            _scheduleTriggerTime = System.Environment.GetEnvironmentVariable("ScheduleTriggerTime");
            _instrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            var daysParam = System.Environment.GetEnvironmentVariable("DaysToAddOrSub");
            if (!string.IsNullOrEmpty(daysParam))
                _daysToAddOrSub = int.Parse(daysParam);
        }

        [Timeout("02:00:00")]
        [FunctionName("FUNC_SQLDB_Receive")]
        public static void Run([TimerTrigger("%ScheduleTriggerTime%")]TimerInfo myTimer, ILogger logger)
       {
            logger.LogInformation("FUNC_SQLDB_Receive Started");
            try
            {
                var sqlProperties = new SQLProperties()
                {
                    SqlConnectionString = _sqlConnectionString,
                    Username = _username,
                    Password = _password,
                    CrmUrl = _crmUrl,
                    ClientAppId = _clientAppId,
                    ClientSecretId = _clientSecretId,
                    CommandType = "SP",
                    ApiUrl = System.Environment.GetEnvironmentVariable("apiUrl"),
                    authority = System.Environment.GetEnvironmentVariable("authority"),
                    DaysToAddOrSub = _daysToAddOrSub
                };

                var data = FunctionHelper.ReceiveDataFromSQLAsync(sqlProperties, 60, logger).Result;
                var dataSet = (DataSet)data["Data"];

                var productList = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new SMIG_ProductSalesHistory
                    {
                        sig_rowid = dataRow.Field<int>("RowId"),
                        sig_businessunitid = dataRow.Field<string>("BusinessUnitId"),
                        sig_businessunitname = dataRow.Field<string>("BusinessUnitName"),
                        //sig_BusinessUnit_odata_bind = "/businessunits(99f1a2fa-9e0d-ec11-b6e6-000d3a84eb0c)",
                        sig_orderid = dataRow.Field<string>("OrderId"),
                        sig_ordernumber = dataRow.Field<string>("OrderNumber"),
                        sig_datecreated = dataRow.Field<DateTime>("DateCreated").ToString("yyyy-MM-dd HH:mm:ss,fff"),
                        sig_doctorname = dataRow.Field<string>("DoctorName"),
                        sig_postalcode = dataRow.Field<string>("PostalCode"),
                        sig_accountname = dataRow.Field<string>("AccountName"),
                        sig_dateinvoiced = dataRow.Field<DateTime>("DateInvoiced").ToString("yyyy-MM-dd HH:mm:ss,fff"),
                        sig_product = dataRow.Field<string>("Product"),
                        sig_department = dataRow.Field<string>("Department"),
                        sig_units = dataRow.Field<decimal>("Units"),
                        sig_remakeunits = dataRow.Field<decimal>("RemakeUnits"),
                        sig_totalcharge = dataRow.Field<decimal>("TotalCharge"),
                        sig_totaltax = dataRow.Field<decimal>("TotalTax"),
                        sig_remakedollars = dataRow.Field<decimal>("RemakeDollars"),
                        sig_remakereason = dataRow.Field<string>("RemakeReason"),
                        sig_manufacturername = dataRow.Field<string>("ManufacturerName"),
                        sig_doctorid = dataRow.Field<string>("DoctorId"),
                        sig_accountid = dataRow.Field<string>("AccountId"),
                        sig_productioncost = dataRow.Field<decimal>("ProductionCost"),
                        sig_salespersonid = dataRow.Field<string>("SalesPersonId"),
                        sig_salesperson = dataRow.Field<string>("SalesPerson"),
                        sig_reportgroupname = dataRow.Field<string>("ReportGroupName"),
                        sig_corporatename = dataRow.Field<string>("CorporateName"),
                        sig_workflow = dataRow.Field<string>("WorkFlow"),
                        sig_groupname = dataRow.Field<string>("GroupName"),
                        sig_producttype = dataRow.Field<string>("ProductType"),
                        sig_routename = dataRow.Field<string>("RouteName"),
                        sig_accountadjustmentid = dataRow.Field<string>("AccountAdjustmentId")
                    }).ToList();

                logger.LogInformation("FUNC_SQLDB_Receive Total rows: " + productList.Count);

                if (productList.Any())
                {
                    int s = 0;
                    var productChunks = FunctionHelper.ChunkBy(productList, 999);
                    foreach (var chunk in productChunks)
                    {
                        s++;
                        var response = DataverseHelper.BulkOperationsAPI(chunk, sqlProperties, logger);
                        if (string.IsNullOrEmpty(response))
                        {
                            logger.LogError($"Somme error occured : {response}");
                            break;
                        }
                        logger.LogInformation("FUNC_SQLDB_Receive Chunk number completed : " + s);
                    }

                }

            

            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
 
        }

    }
}
