using Jenmar.FunctionApp.SQLDB.Receive.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenmar.FunctionApp.SQLDB.Receive
{
    public static class FunctionHelper
    {
        public static async Task<Dictionary<string, object>> ReceiveDataFromSQLAsync(SQLProperties properties, int sqlConTimeOut, ILogger logger)
        {
            try
            {
                var data = DateTime.UtcNow;
                if (properties.DaysToAddOrSub != 0)
                    data = data.AddDays(properties.DaysToAddOrSub);

                logger.LogInformation($"Receiving datetime: {data}");

                string sqlQuery = $"SELECT * FROM SMIG_ProductSalesHistory where Cast(DateCreated as date) > Cast('{data}' as date)";


                var sqlResponse = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(properties.SqlConnectionString))
                {
                    using (var conn = new SqlConnection(properties.SqlConnectionString))
                    {
                        var commGetDetail = new SqlCommand(sqlQuery, conn);
                        var dataSet = new DataSet();

                        commGetDetail.CommandType = CommandType.Text;
                        commGetDetail.CommandTimeout = sqlConTimeOut;

                        try
                        {
                            conn.Open();
                            var da = new SqlDataAdapter(commGetDetail);
                            da.Fill(dataSet);
                            da.Dispose();

                            sqlResponse.Add("Data", dataSet);
                            sqlResponse.Add("Status", "true");
                            conn.Close();
                            conn.Dispose();

                        }
                        catch (Exception ex)
                        {
                            var s = ex.Message;
                            throw ex;
                        }
                    }
                }
                return sqlResponse;
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
          
        }

        public static List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

    }
}
