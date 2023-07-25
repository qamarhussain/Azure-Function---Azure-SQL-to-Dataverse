using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Jenmar.FunctionApp.SQLDB.Receive.Models
{
    public static class DataverseHelper
    {
        static string PREFER_HEADER = "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"";

        public static string BulkOperationsAPI(List<SMIG_ProductSalesHistory> sMIG_ProductSalesHistories, SQLProperties sQLProperties, ILogger logger)
        {
            try
            {
                var dateTime = DateTime.Now;
                //string entityname = "sig_smig_productsaleshistories";
                string entityname = "sig_easyrxproductsaleshistories";
                string crmRestQuery = sQLProperties.ApiUrl + "$batch";
                string batchName = "batch_" + dateTime.ToString("MMddyyyy");
                string changeSetVar = "changeset_" + dateTime.ToString("MMddyyyy");
                int coRelId = 1;
                string changeSet1 = null;
                string requestBody = "--" + batchName + Environment.NewLine
                                 + "Content-Type:multipart/mixed;boundary=" + changeSetVar + Environment.NewLine + Environment.NewLine + Environment.NewLine;

                //Creating one record
                foreach (var item in sMIG_ProductSalesHistories)
                {
                    changeSet1 = PrepareReuqestBody(changeSetVar, coRelId++, entityname, item, sQLProperties, logger, item.sig_rowid.ToString());

                    if (!string.IsNullOrEmpty(changeSet1))
                        requestBody += changeSet1 + Environment.NewLine;
                }

                requestBody += "--" + changeSetVar + "--" + Environment.NewLine;
                requestBody += "--" + batchName + "--";

                //format the body URLs
                requestBody = requestBody.Replace("\\/", "/");

                //call the batch Operations
                return BatchQueryCall(crmRestQuery, batchName, requestBody, sQLProperties, logger);
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }

        private static string PrepareReuqestBody(string changeset, int contentId, string entityName, object entity, SQLProperties sQLProperties,
            ILogger logger, string recordid)
        {
            try
            {

                string x = string.Empty;
                using (MemoryStream streamOpportunitySerialize = new MemoryStream())
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(entity.GetType());
                    ser.WriteObject(streamOpportunitySerialize, entity);
                    streamOpportunitySerialize.Position = 0;
                    StreamReader srOpportunity = new StreamReader(streamOpportunitySerialize);
                    string objectJSON = srOpportunity.ReadToEnd();
                    objectJSON = objectJSON.Replace("_odata_bind", "@odata.bind");

                    x = "--" + changeset + Environment.NewLine
                        + "Content-Type: application/http" + Environment.NewLine
                        + "Content-Transfer-Encoding: binary" + Environment.NewLine
                        + "Content-ID: " + contentId + Environment.NewLine + Environment.NewLine
                        + "PATCH " + sQLProperties.ApiUrl + entityName + "(sig_rowid=" + recordid + ") HTTP/1.1" + Environment.NewLine
                        + "Content-Type: application/json;type=entry" + Environment.NewLine
                        + "Accept:application/json" + Environment.NewLine + Environment.NewLine
                        + objectJSON + Environment.NewLine + Environment.NewLine;
                }

                return x;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
           
        }



        private static string BatchQueryCall(string crmRestQuery, string batchName, string requestBody, SQLProperties sQLProperties,
            ILogger logger)
        {
            string funResponse = string.Empty;
            try
            {
                var token = AccessTokenGenerator(sQLProperties, logger).Result;
                if (!string.IsNullOrEmpty(token))
                {
                    using (HttpClient _httpClient = new HttpClient())
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, crmRestQuery);
                        //add header parameters
                        request.Headers.Add("Authorization", "Bearer " + token);
                        request.Headers.Add("Accept", "application/json");
                        request.Headers.Add("Prefer", PREFER_HEADER);
                        request.Headers.Add("OData-MaxVersion", "4.0");
                        request.Headers.Add("OData-Version", "4.0");
                        request.Content = new StringContent(requestBody);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed;boundary=" + batchName);

                        HttpResponseMessage response = _httpClient.SendAsync(request).Result;
                        string responseString = response.Content.ReadAsStringAsync().Result;

                        //logger.LogInformation("FUNC_SQLDB_Receive requestBody: " + requestBody);

                        //logger.LogInformation("FUNC_SQLDB_Receive response: " + responseString);

                        logger.LogInformation("FUNC_SQLDB_Receive response.IsSuccessStatusCode: " + response.IsSuccessStatusCode);

                        if (response.IsSuccessStatusCode)
                        {
                            funResponse = responseString;
                        }
                        else
                        {
                            logger.LogError($"Transaction in error: API Error message: {responseString}, API Request: {requestBody}");
                            funResponse = responseString;
                        }
                    }
                }
                return funResponse;

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
          
        }

        private static async Task<string> AccessTokenGenerator(SQLProperties sQLProperties, ILogger logger)
        {
            try
            {
                ClientCredential credentials
                    = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(sQLProperties.ClientAppId, sQLProperties.ClientSecretId);
                var authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(sQLProperties.authority);
                var result = await authContext.AcquireTokenAsync(sQLProperties.CrmUrl, credentials);
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
