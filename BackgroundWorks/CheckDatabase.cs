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
                //delete saved file after 10 minutes
                deleteRowInDB();
                
                
                //List<string> jsonLists = new List<string>();
                List<Queue> queuesList = new List<Queue>();
                //check queue in DB where status = true
                string query = @"select  * from queue where status='fromAzure'";

                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("DBConnect");
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
                        string task_type = table.Rows[i]["task_type"].ToString();
                        string userid = table.Rows[i]["userid"].ToString();
                        string taskbodyString = table.Rows[i]["taskbody"].ToString();
                        string[] taskbodyItems = taskbodyString.Split(',');
                        var options=new JsonSerializerOptions { WriteIndented = true };
                        string phoneNumber = taskbodyItems[0];
                        string urlPath;
                        string jsonString = "";
                        switch (task_type)
                        {
                            //SalarySheet_PeroidsListRequest
                            case "Расчетный лист":
                                urlPath = "/Portal/SalarySheet_PeroidsListRequest";
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
                                
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //AvailableDocTypesRequest
                            case "sendSMS":
                                urlPath = "/Portal/AvailableDocTypesRequest";
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
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //OTP_Out
                            case "sendSMSCode":
                                urlPath = "/OTP_Out";
                                var sendSMSCode = new SendSMSCode
                                {
                                    phone = taskbodyItems[0],
                                    message = taskbodyItems[1]
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(sendSMSCode, options);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //SalarySheet_LinkRequest
                            case "Расчетный лист отрезок":
                                urlPath = "/Portal/SalarySheet_LinkRequest";
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
                                                    period = taskbodyItems[1]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetSalarysheetUrl, options);
                                    await SendWebRequestAsync(urlPath, jsonString);
                                break; 
                            //ActiveOrderNoteRequest
                            case "get_active_requests":
                                urlPath = "/Portal/ActiveOrderNoteRequest";
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
                                                    phone = phoneNumber
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetActivityRequest, options);
                                    await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //CancellationDocRequest
                            case "cancel_request":
                                urlPath = "/Portal/CancellationDocRequest";
                                var cancelRequestModel = new CancelRequestModel()
                                    {
                                        configuration = "<config_name>",
                                        queue = "<queue_name>",

                                        tasks = new List<CancelRequestModelTasks>
                                        {

                                            new CancelRequestModelTasks()
                                            {
                                                task_id = taskid,
                                                state = "new",
                                                task_type = "cancel_request",
                                                payload = new CancelRequestModelPayloads
                                                {
                                                    phone = phoneNumber,
                                                    key = taskbodyItems[1]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(cancelRequestModel, options);
                                    await SendWebRequestAsync(urlPath, jsonString);
                                    
                                    break;
                            //RestOfVacationRequest
                            case "RestOfVacationRequest":
                                urlPath = "/Portal/RestOfVacationRequest";
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
                                                    date = taskbodyItems[1]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(restOfVacationRequestModel, options);
                                    await SendWebRequestAsync(urlPath, jsonString);
                                    
                                    break;
                            //ConditionAndPlaceOfWorkRequest
                            case "ConditionAndPlaceOfWorkRequest":
                                urlPath = "/Portal/ConditionAndPlaceOfWorkRequest";
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
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //OrderNote_PeriodsListRequest
                            case "OrderNote_PeriodsListRequest":
                                urlPath = "/Portal/OrderNote_PeriodsListRequest";
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
                                            task_type = taskbodyItems[2],
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
                                await SendWebRequestAsync(urlPath, jsonString);
                                
                                break;
                            //OrderNote_2NDFLRequest 
                            case "get_2ndfl_sheet":
                                urlPath = "/Portal/OrderNote_2NDFLRequest";
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
                                                period = taskbodyItems[1],
                                                count = taskbodyItems[2],
                                                location = taskbodyItems[3]
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(orderNote_2NDFLRequest, options);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //OrderNote_PlaceListRequest
                            case "get_available_locations":
                                urlPath = "/Portal/OrderNote_PlaceListRequest";
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
                                                    doc_type = taskbodyItems[2]
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                    options = new JsonSerializerOptions { WriteIndented = true };
                                    jsonString = System.Text.Json.JsonSerializer.Serialize(orderNote_PlaceListRequest, options);
                                    await SendWebRequestAsync(urlPath, jsonString);
                                break; 
                            //OrderNote_FromJobRequest
                            case "get_working_place_sheet":
                                urlPath = "/Portal/OrderNote_FromJobRequest";
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
                                                    count=taskbodyItems[1],
                                                    location=taskbodyItems[2],
                                                    vacation=Convert.ToBoolean(taskbodyItems[3]),
                                                    vacation_period=taskbodyItems[4],
                                                    salary=Convert.ToBoolean(taskbodyItems[5]),
                                                }
                                            }
                                        }
                                    };
                                    //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_FromJobRequest, options);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break; 
                            //OrderNote_SeniorityRequest
                            case "get_szv_sheet":
                                urlPath = "/Portal/OrderNote_SeniorityRequest";
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
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //OrderNote_VacationRequest
                            case "get_vacations_sheet":
                                urlPath = "/Portal/OrderNote_VacationRequest";
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
                                                period = taskbodyItems[1],
                                                count = taskbodyItems[2],
                                                location = taskbodyItems[3]
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_VacationRequest, options);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            //OrderNote_WorkBookExtractionRequest
                            case "get_employment_history_part_copy":
                                urlPath = "/Portal/OrderNote_WorkBookExtractionRequest";
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
                                                period = taskbodyItems[1],
                                                count = taskbodyItems[2],
                                                location = taskbodyItems[3]
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_WorkBookExtractionRequest, options);
                                await SendWebRequestAsync(urlPath, jsonString);

                                
                                break;
                            //OrderNote_WorkBookCopyRequest
                            case "get_employment_history_copy":
                                urlPath = "/Portal/OrderNote_WorkBookCopyRequest";
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
                                                goal = taskbodyItems[1],
                                                count = taskbodyItems[2],
                                                location = taskbodyItems[3]

                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_WorkBookCopyRequest, options);
                                await SendWebRequestAsync(urlPath, jsonString);
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
                await Task.Delay(500); // timer each 1 sec 
            }
        }
        
        
        public bool deleteRowInDB()
        {
            var date = DateTime.Now.AddMinutes(-10);
            var sqlDataSource = _configuration.GetConnectionString("DBConnect");
            
            //string query = @"select * from saved_files where createtime<@date or status = 'downloaded'";
            string query = @"delete from saved_files where createtime<@date";
            

            DataTable table = new DataTable();
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@date", date);
                   
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }

                return true;
            }
        }
        

        //urlPath should be /Portal/some/path
        private async Task SendWebRequestAsync(string urlPath, string jsonString)
        {
            try
            {
                string loginPass = _configuration.GetConnectionString("LoginPass");
                string url =  _configuration.GetConnectionString("SapURL");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic", Convert.ToBase64String(
                                System.Text.ASCIIEncoding.ASCII.GetBytes(
                                    loginPass)));
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(url +  urlPath),
                        Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                    };
                    
                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    logger.LogInformation($"RESPONSE BODY: { responseBody }");
                    
                }
            }
            catch (HttpRequestException e)
            {
                logger.LogInformation($"Error: {e.Message }");
            }
        }
    }
}
