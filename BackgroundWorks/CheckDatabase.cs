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
                            //SalarySheet_PeroidsListRequest
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
                            //AvailableDocTypesRequest
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
                            //OTP_Out
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
                            //SalarySheet_LinkRequest
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
                            //ActiveOrderNoteRequest
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
                            //CancellationDocRequest
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
                            //RestOfVacationRequest
                            case "RestOfVacationRequest":
                                var restOfVacationRequestModel = new RestOfVacationRequestModel()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<RestOfVacationRequestModelTasks>
                                        {

                                            new RestOfVacationRequestModelTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "user_info",
                                                payload = new RestOfVacationRequestModelPayloads
                                                {
                                                    phone = phoneNumber,
                                                    date = taskbodyItems[4]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(restOfVacationRequestModel, options);
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
                                                RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/RestOfVacationRequest"),
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
                            //ConditionAndPlaceOfWorkRequest
                            case "ConditionAndPlaceOfWorkRequest":
                                var conditionAndPlaceOfWorkRequest = new SapGetModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<GetTasks>
                                    {

                                        new GetTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "user_info",
                                            payload = new Payloads
                                            {
                                                phone = phoneNumber
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(conditionAndPlaceOfWorkRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/ConditionAndPlaceOfWorkRequest"),
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
                            //OrderNote_PeriodsListRequest
                            case "OrderNote_PeriodsListRequest":
                                var orderNote_PeriodsListRequest = new SapGetModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<GetTasks>
                                    {

                                        new GetTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_available_employment_history_periods",
                                            payload = new Payloads
                                            {
                                                phone = phoneNumber
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(orderNote_PeriodsListRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_PeriodsListRequest"),
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
                            //OrderNote_2NDFLRequest 
                            case "get_2ndfl_sheet":
                                var orderNote_2NDFLRequest = new OrderNote_2NDFLRequestModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<OrderNote_2NDFLRequestModelTasks>
                                    {

                                        new OrderNote_2NDFLRequestModelTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_2ndfl_sheet",
                                            payload = new OrderNote_2NDFLRequestModelPayloads
                                            {
                                                phone = phoneNumber,
                                                period = taskbodyItems[4],
                                                count = taskbodyItems[5],
                                                location = taskbodyItems[6]
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(orderNote_2NDFLRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_2NDFLRequest"),
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
                            //OrderNote_PlaceListRequest
                            case "get_available_locations":
                                var orderNote_PlaceListRequest = new OrderNote_PlaceListRequestModel()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<OrderNote_PlaceListRequestTasks>
                                        {

                                            new OrderNote_PlaceListRequestTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "get_available_locations",
                                                payload = new OrderNote_PlaceListRequestPayloads
                                                {
                                                    phone = phoneNumber,
                                                    doc_type = taskbodyItems[4]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(orderNote_PlaceListRequest, options);
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
                                                RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_PlaceListRequest"),
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
                            //OrderNote_FromJobRequest
                            case "get_working_place_sheet":
                                var OrderNote_FromJobRequest = new OrderNote_FromJobRequestModel()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<OrderNote_FromJobRequestModelTasks>
                                        {

                                            new OrderNote_FromJobRequestModelTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "get_working_place_sheet",
                                                payload = new OrderNote_FromJobRequestModelPayloads
                                                {
                                                    phone = phoneNumber,
                                                    count=taskbodyItems[4],
                                                    location=taskbodyItems[5],
                                                    vacation=Convert.ToBoolean(taskbodyItems[6]),
                                                    vacation_period=taskbodyItems[7],
                                                    salary=Convert.ToBoolean(taskbodyItems[6]),
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_FromJobRequest, options);
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
                                                RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_FromJobRequest"),
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
                            //OrderNote_SeniorityRequest
                            case "get_szv_sheet":
                                var OrderNote_SeniorityRequest = new SapGetModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<GetTasks>
                                    {

                                        new GetTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_szv_sheet",
                                            payload = new Payloads
                                            {
                                                phone = phoneNumber
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_SeniorityRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_SeniorityRequest"),
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
                            //OrderNote_VacationRequest
                            case "get_vacations_sheet":
                                var OrderNote_VacationRequest = new OrderNote_2NDFLRequestModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<OrderNote_2NDFLRequestModelTasks>
                                    {

                                        new OrderNote_2NDFLRequestModelTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_vacations_sheet",
                                            payload = new OrderNote_2NDFLRequestModelPayloads
                                            {
                                                phone = phoneNumber,
                                                period = taskbodyItems[4],
                                                count = taskbodyItems[5],
                                                location = taskbodyItems[6]
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_VacationRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_VacationRequest"),
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
                            //OrderNote_WorkBookExtractionRequest
                            case "get_employment_history_part_copy":
                                var OrderNote_WorkBookExtractionRequest = new OrderNote_2NDFLRequestModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<OrderNote_2NDFLRequestModelTasks>
                                    {

                                        new OrderNote_2NDFLRequestModelTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_employment_history_part_copy",
                                            payload = new OrderNote_2NDFLRequestModelPayloads
                                            {
                                                phone = phoneNumber,
                                                period = taskbodyItems[4],
                                                count = taskbodyItems[5],
                                                location = taskbodyItems[6]
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_WorkBookExtractionRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_WorkBookExtractionRequest"),
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
                            //OrderNote_WorkBookCopyRequest
                            case "get_employment_history_copy":
                                var OrderNote_WorkBookCopyRequest = new OrderNote_WorkBookCopyRequestModel
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<OrderNote_WorkBookCopyRequestTasks>
                                    {

                                        new OrderNote_WorkBookCopyRequestTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_employment_history_copy",
                                            payload = new OrderNote_WorkBookCopyRequestTasksPayloads
                                            {
                                                phone = phoneNumber,
                                                count = taskbodyItems[5],
                                                location = taskbodyItems[6],
                                                goal = taskbodyItems[4]

                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_WorkBookCopyRequest, options);
                                
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
                                            RequestUri = new Uri("http://sappo1ci.sap.metinvest.com:50000/RESTAdapter/Portal/OrderNote_WorkBookCopyRequest"),
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
