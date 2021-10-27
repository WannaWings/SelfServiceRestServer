using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestService.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RestService.BackgroundWorks
{
    public class CheckDatabase : ICheckDatabase
    {
        // static readonly HttpClient client = new HttpClient();
        private readonly IConfiguration _configuration;

        private readonly ILogger<CheckDatabase> logger;
        
        public CheckDatabase(ILogger<CheckDatabase> logger, IConfiguration configuration)
        {
            this.logger = logger; _configuration = configuration;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                List<string> jsonLists = new List<string>();
                List<Queue> queuesList = new List<Queue>();
                //check queue in DB where status = true
                string query = @"select  * from queue where status='fromAzure'";

                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
                NpgsqlDataReader myReader;
                using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                    {
                        //myCommand.Parameters.AddWithValue("@status", "fromAzure");
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);

                        myReader.Close();
                        myCon.Close();
                    }
                }
                if (table.Rows.Count != 0){
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        string taskid = table.Rows[i]["taskid"].ToString();
                        string taskbodyString = table.Rows[i]["taskbody"].ToString();
                        string[] taskbodyItems = taskbodyString.Split(',');
                        string flag = Convert.ToString(taskbodyItems[2]);
                        var options=new JsonSerializerOptions { WriteIndented = true };
                        string phoneNumber = taskbodyItems[3];

                        string jsonString = "";
                        switch (flag)
                        {
                            case "Расчетный лист":
                                var SapGetModel = new SapGetModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<GetTasks>
                                    {

                                        new GetTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_available_salary_periods",
                                            payload = new Payloads
                                            {
                                                phone = phoneNumber
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetModel, options);
                                
                                try
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue(
                                                "Basic", Convert.ToBase64String(
                                                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                        $"CH_MOBPO1_01:fgd$#456DF")));
                                        var request = new HttpRequestMessage
                                        {
                                            Method = HttpMethod.Get,
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/SalarySheet_PeroidsListRequest"),
                                            Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                                        };
                    
                                        var response = await client.SendAsync(request).ConfigureAwait(false);
                                        response.EnsureSuccessStatusCode();
                    
                                        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    
                                    
                            
                                    }
                                }
                                catch (HttpRequestException e)
                                {
                                    logger.LogInformation($"Error: {e.Message }");
                                }
                                break;
                            case "sendSMS":
                                var verifySMSModel = new SapGetModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<GetTasks>
                                    {

                                        new GetTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_document_types",
                                            payload = new Payloads
                                            {
                                                phone = phoneNumber
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(verifySMSModel, options);
                                
                                try
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue(
                                                "Basic", Convert.ToBase64String(
                                                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                        $"CH_MOBPO1_01:fgd$#456DF")));
                                        var request = new HttpRequestMessage
                                        {
                                            Method = HttpMethod.Get,
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/AvailableDocTypesRequest"),
                                            Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                                        };
                    
                                        var response = await client.SendAsync(request).ConfigureAwait(false);
                                        response.EnsureSuccessStatusCode();
                    
                                        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                    }
                                }
                                catch (HttpRequestException e)
                                {
                                    logger.LogInformation($"Error: {e.Message }");
                                }
                                break;
                            case "sendSMSCode":
                                var sendSMSCode = new SendSMSCode
                                {
                                    phone = taskbodyItems[3],
                                    message = taskbodyItems[4]
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(sendSMSCode, options);
                                
                                try
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue(
                                                "Basic", Convert.ToBase64String(
                                                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                        $"CH_MOBPO1_01:fgd$#456DF")));
                                        var request = new HttpRequestMessage
                                        {
                                            Method = HttpMethod.Get,
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OTP_Out"),
                                            Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                                        };
                    
                                        var response = await client.SendAsync(request).ConfigureAwait(false);
                                        response.EnsureSuccessStatusCode();
                    
                                        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                    }
                                }
                                catch (HttpRequestException e)
                                {
                                    logger.LogInformation($"Error: {e.Message }");
                                }
                                break;
                            case "Расчетный лист отрезок":
                                var SapGetSalarysheetUrl = new SapGetSalarysheetUrl()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<SalarySheetUrlGetTasks>
                                        {

                                            new SalarySheetUrlGetTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "get_available_salary_periods",
                                                payload = new SalarySheetUrlPayloads
                                                {
                                                    phone = phoneNumber,
                                                    period = taskbodyItems[4]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetSalarysheetUrl, options);
                                    jsonLists.Add(jsonString);
                                    try
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            client.DefaultRequestHeaders.Authorization =
                                                new AuthenticationHeaderValue(
                                                    "Basic", Convert.ToBase64String(
                                                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                            $"CH_MOBPO1_01:fgd$#456DF")));
                                            var request = new HttpRequestMessage
                                            {
                                                Method = HttpMethod.Get,
                                                RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/SalarySheet_LinkRequest"),
                                                Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                                            };
                        
                                            var response = await client.SendAsync(request).ConfigureAwait(false);
                                            response.EnsureSuccessStatusCode();
                        
                                            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        
                                        
                                
                                        }
                                    }
                                    catch (HttpRequestException e)
                                    {
                                        logger.LogInformation($"Error: {e.Message }");
                                    }
                                    break; 
                            case "get_active_requests":
                                var SapGetActivityRequest = new SapGetSalarysheetUrl()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<SalarySheetUrlGetTasks>
                                        {

                                            new SalarySheetUrlGetTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "get_active_requests",
                                                payload = new SalarySheetUrlPayloads
                                                {
                                                    phone = phoneNumber,
                                                    period = taskbodyItems[4]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetActivityRequest, options);
                                    jsonLists.Add(jsonString);
                                    try
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            client.DefaultRequestHeaders.Authorization =
                                                new AuthenticationHeaderValue(
                                                    "Basic", Convert.ToBase64String(
                                                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                            $"CH_MOBPO1_01:fgd$#456DF")));
                                            var request = new HttpRequestMessage
                                            {
                                                Method = HttpMethod.Get,
                                                RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/ActiveOrderNoteRequest"),
                                                Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                                            };
                        
                                            var response = await client.SendAsync(request).ConfigureAwait(false);
                                            response.EnsureSuccessStatusCode();
                        
                                            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        
                                        
                                
                                        }
                                    }
                                    catch (HttpRequestException e)
                                    {
                                        logger.LogInformation($"Error: {e.Message }");
                                    }
                                    break;
                            case "get_salary_sheet":
                                var SapGetCancellationDocRequest = new SapGetSalarysheetUrl()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<SalarySheetUrlGetTasks>
                                        {

                                            new SalarySheetUrlGetTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "get_salary_sheet",
                                                payload = new SalarySheetUrlPayloads
                                                {
                                                    phone = phoneNumber,
                                                    period = taskbodyItems[4]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetCancellationDocRequest, options);
                                    jsonLists.Add(jsonString);
                                    try
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            client.DefaultRequestHeaders.Authorization =
                                                new AuthenticationHeaderValue(
                                                    "Basic", Convert.ToBase64String(
                                                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                                                            $"CH_MOBPO1_01:fgd$#456DF")));
                                            var request = new HttpRequestMessage
                                            {
                                                Method = HttpMethod.Get,
                                                RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/CancellationDocRequest"),
                                                Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                                            };
                        
                                            var response = await client.SendAsync(request).ConfigureAwait(false);
                                            response.EnsureSuccessStatusCode();
                        
                                            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        
                                        
                                
                                        }
                                    }
                                    catch (HttpRequestException e)
                                    {
                                        logger.LogInformation($"Error: {e.Message }");
                                    }
                                    break;

                            
                            default:
                                break;
                        }
                        
                        
                        //send sap message
                        
                        
                        //change queue status
                        string updateQeueueQuery = @"update queue set status ='sendedSap' where taskid = @taskid";
                        using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
                        {
                            myCon.Open();
                            using (NpgsqlCommand myCommand = new NpgsqlCommand(updateQeueueQuery, myCon))
                            {
                                myCommand.Parameters.AddWithValue("@taskid", taskid);
                                myReader = myCommand.ExecuteReader();
                                table.Load(myReader);

                                myReader.Close();
                                myCon.Close();
                            }
                        }
                    }
                    
                }
                
                
                
                
                
                
                
                
                logger.LogInformation($"am working");

                
                
                await Task.Delay(1000 * 5);
            }
        }
    }
}
