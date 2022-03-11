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
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Task = System.Threading.Tasks.Task;

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
                await TimeoutFuncsAsync(1);
                
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
                        string body;
                       // body = null;
                        List<ResultFrom1c> myDeserializedClass;
                        //myDeserializedClass = new List<ResultFrom1c>();
                        switch (task_type)
                        {
                        //survey
                        //https://sappo1ci.sap.metinvest.com:50001/RestAdapter/Portal/SurveyResponce
                        //1C requests 
                            case "salarySheet":
                                var GetModel1C = new SapGetModel1C()
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",
                                    
                                    tasks = new List<GetTasks1C>
                                    {
                                    
                                        new GetTasks1C()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_available_salary_periods",
                                            payload = new Payloads1C
                                            {
                                                snils = "",
                                                phone = phoneNumber,
                                                email = taskbodyItems[1],
                                                date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                                            }
                                        }
                                    }
                                };
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(GetModel1C, options);
                                await UpdateQueueCompleateAsync(taskid, sqlDataSource);
                                body =  await Send1CRequestAsync(jsonString);
                                 myDeserializedClass = JsonConvert.DeserializeObject<List<ResultFrom1c>>(body);
                                await Add1CResultToDB(myDeserializedClass[0],userid, task_type);
                                //SalarySheet1c nas  = JsonConvert.DeserializeObject<SalarySheet1c>(body);
                                break;
                            case "check_employee_phone_1c":
                            {
                                var GetModelCheck1C = new SapGetModel1C()
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",

                                    tasks = new List<GetTasks1C>
                                    {

                                        new GetTasks1C()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "check_employee",
                                            payload = new Payloads1C
                                            {
                                                snils = "",
                                                phone = taskbodyItems[0],
                                                email = "",
                                                date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                                            }
                                        }
                                    }
                                };
                                options = new JsonSerializerOptions {WriteIndented = true};
                                jsonString = System.Text.Json.JsonSerializer.Serialize(GetModelCheck1C, options);
                                await UpdateQueueCompleateAsync(taskid, sqlDataSource);
                                body = await Send1CRequestAsync(jsonString);
                                myDeserializedClass = JsonConvert.DeserializeObject<List<ResultFrom1c>>(body);
                                await Add1CResultToDB(myDeserializedClass[0], userid, task_type);
                                //SalarySheet1c nas  = JsonConvert.DeserializeObject<SalarySheet1c>(body);
                                break;
                            };
                            case "get_employee_data1С":
                        {
                            var getEmployeeData1C = new SapGetModel1C()
                            {
                                configuration = "<config_name>",
                                queue = "<queue_name>",

                                tasks = new List<GetTasks1C>
                                {

                                    new GetTasks1C()
                                    {
                                        task_id = taskid,
                                        state = "new",
                                        task_type = "get_employee_data",
                                        payload = new Payloads1C
                                        {
                                            snils = "",
                                            phone = phoneNumber,
                                            email = taskbodyItems[1],
                                            date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                                        }
                                    }
                                }
                            };
                            options = new JsonSerializerOptions {WriteIndented = true};
                            jsonString = System.Text.Json.JsonSerializer.Serialize(getEmployeeData1C, options);
                            await UpdateQueueCompleateAsync(taskid, sqlDataSource);

                            body = await Send1CRequestAsync(jsonString);
                            myDeserializedClass = JsonConvert.DeserializeObject<List<ResultFrom1c>>(body);
                            await Add1CResultToDB(myDeserializedClass[0], userid, task_type);
                            //SalarySheet1c nas  = JsonConvert.DeserializeObject<SalarySheet1c>(body);
                            break;
                        }
                            case "available_vacations1C":
                                var availableVacations1C = new SapGetModel1C()
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",
                                    
                                    tasks = new List<GetTasks1C>
                                    {
                                    
                                        new GetTasks1C()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "available_vacations",
                                            payload = new Payloads1C
                                            {
                                                snils = "",
                                                phone = phoneNumber,
                                                email = taskbodyItems[2],
                                                date = taskbodyItems[1]
                                            }
                                        }
                                    }
                                };
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(availableVacations1C, options);
                                await UpdateQueueCompleateAsync(taskid, sqlDataSource);
                                body =  await Send1CRequestAsync(jsonString);
                                myDeserializedClass = JsonConvert.DeserializeObject<List<ResultFrom1c>>(body);
                                await Add1CResultToDB(myDeserializedClass[0],userid, task_type);
                                //SalarySheet1c nas  = JsonConvert.DeserializeObject<SalarySheet1c>(body);
                                break;
                            case "salarySeetFile":
                                var salartSheetFile = new SalartSheetFile()
                                {
                                    configuration = "<config_name>",
                                    queue = "<queue_name>",
                                    
                                    tasks = new List<Tasks1CSalaryFile>
                                    {
                                    
                                        new Tasks1CSalaryFile()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "get_salary_sheet",
                                            payload = new Payloads1CSalaryFile
                                            {
                                                period = taskbodyItems[1],
                                                snils = "",
                                                phone = phoneNumber,
                                                email = taskbodyItems[2],
                                                date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                                            }
                                        }
                                    }
                                };
                                options = new JsonSerializerOptions { WriteIndented = true };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(salartSheetFile, options);
                                await UpdateQueueCompleateAsync(taskid, sqlDataSource);
                               
                                body = await Send1CRequestAsync(jsonString);
                                myDeserializedClass = JsonConvert.DeserializeObject<List<ResultFrom1c>>(body);

                                await Add1CResultToDB(myDeserializedClass[0],userid, task_type);
                                
                                
                                //SalarySheet1c nas  = JsonConvert.DeserializeObject<SalarySheet1c>(body);
                                break;
                                
                                
                            //SAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAPSAP
                        
                         //surveySAP
                            case "send_survey_results":
                                taskbodyItems = taskbodyString.Split('|');
                                string payloadSurvey = taskbodyItems[2].Replace("\"text\":\"\"", "");
                                List<QuestionsSurveyModel> questionsSurveyModels= JsonConvert.DeserializeObject<List<QuestionsSurveyModel>>(payloadSurvey);
                                urlPath = "/Portal/SurveyResponce";
                                // urlPath = "http://sappo2ci.sap.metinvest.com:50000/RESTAdapter/Portal/SurveyResponce";
                                var surveyAnswerSend = new SurveyAnswerSendModel
                                {
                                    configuration = "MIDev",
                                    queue = "survey_result",
                                    tasks = new List<SurveyAnswerSendModelTasks>
                                    {

                                        new SurveyAnswerSendModelTasks()
                                        {
                                            task_id = taskid,
                                            state = "new",
                                            task_type = "send_survey_results",
                                            payload = new SurveyAnswerSendModelPayloads
                                            {
                                                phone = taskbodyItems[0],
                                                survey_id = taskbodyItems[1],
                                                questions = questionsSurveyModels
                                            }
                                        }
                                    }
                                };
                                //serialize json response for sap 
                                //options = new JsonSerializerOptions { WriteIndented = true };
                                options = new JsonSerializerOptions
                                {
                                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                                    WriteIndented = true
                                };
                                jsonString = System.Text.Json.JsonSerializer.Serialize(surveyAnswerSend, options);
                                await SendWebRequestAsync(urlPath, jsonString);
                                await UpdateQueueCompleateAsync(taskid, sqlDataSource);
                                break;
                            
                            
                        //SalarySheet_PeroidsListRequest SAP
                            case "Расчетный лист":{
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            }
                            //AvailableDocTypesRequest
                            case "sendSMS":
                            {urlPath = "/Portal/AvailableDocTypesRequest";
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
                                options = new JsonSerializerOptions {WriteIndented = true};
                                jsonString = System.Text.Json.JsonSerializer.Serialize(verifySMSModel, options);
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);

                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            }
                            
                        //OTP_Out
                            case "sendSMSCode":
                        {urlPath = "/Portal/OTP_Out";
                            var sendSMSCode = new SendSMSCode
                            {
                                phone = taskbodyItems[0],
                                message = taskbodyItems[1]
                            };
                            //serialize json response for sap 
                            options = new JsonSerializerOptions {WriteIndented = true};
                            jsonString = System.Text.Json.JsonSerializer.Serialize(sendSMSCode, options);
                            await UpdateQueueCompleateAsync(taskid, sqlDataSource);
                            await SendWebRequestAsync(urlPath, jsonString);
                            break;
                        }
                            
                        //SalarySheet_LinkRequest
                            case "Расчетный лист отрезок":
                        {
                        
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
                            options = new JsonSerializerOptions {WriteIndented = true};
                            jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetSalarysheetUrl, options);
                            await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
                            await SendWebRequestAsync(urlPath, jsonString);
                            break;
                        }
                        //ActiveOrderNoteRequest
                            case "get_active_requests":
                            {
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
                                options = new JsonSerializerOptions {WriteIndented = true};
                                jsonString = System.Text.Json.JsonSerializer.Serialize(SapGetActivityRequest, options);
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            }
                            //CancellationDocRequest
                            case "cancel_request":
                            {
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
                                options = new JsonSerializerOptions {WriteIndented = true};
                                jsonString = System.Text.Json.JsonSerializer.Serialize(cancelRequestModel, options);
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
                                await SendWebRequestAsync(urlPath, jsonString);

                                break;
                            }
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
                                    await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);

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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                    await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
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
                                await UpdateQueueStatusToSendedSap(taskid, sqlDataSource);
                                jsonString = System.Text.Json.JsonSerializer.Serialize(OrderNote_WorkBookCopyRequest, options);
                                await SendWebRequestAsync(urlPath, jsonString);
                                break;
                            default:
                                break;
                        }
                        
                        
                        //send sap message
                        
                        
                        //change queue status
                        
                        
                    }
                    
                }
                
                logger.LogInformation($"am working");
                await Task.Delay(500); // timer each 1 sec 
            }
        }

        public async Task UpdateQueueStatusToSendedSap(string taskid, string sqlDataSource)
        {
            string updateQeueueQuery = @"update queue set status ='sendedSap', lastupdate = @lastupdate where taskid = @taskid";
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(updateQeueueQuery, myCon))
                {
                    myCommand.Parameters.AddWithValue("@taskid", taskid);
                    myCommand.Parameters.AddWithValue("@lastupdate", DateTime.Now);
                    myCommand.ExecuteReader();
                    myCon.Close();
                }
            }
        }

        public async Task TimeoutFuncsAsync(int minutesDelay)
        {
            string query = @"select  * from queue where status='sendedSap' and lastupdate < @dateofcreating  ";
            var date = DateTime.Now.AddMinutes(minutesDelay*(-1));

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("DBConnect");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@dateofcreating", date);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            if (table.Rows.Count != 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    DateTime creatingDate = Convert.ToDateTime(table.Rows[i]["dateofcreating"]);
                    
                    string taskid = table.Rows[i]["taskid"].ToString();
                    string userid = table.Rows[i]["userid"].ToString();
                    string task_type = table.Rows[i]["task_type"].ToString();
                    await AddTimeoutResultsAsync(taskid, userid, task_type, sqlDataSource);
                
                    
                    
                }
            }
        }

        public async Task AddTimeoutResultsAsync(string taskid, string userid, string task_type, string dbConn)
        {
            string query =
                @"insert into sap_results(userid, taskid, completed,job_status, task_type, taskresult, dateofcreating, datasource, lastupdate) 
                                  VALUES (@userid, @taskid, @completed,@job_status, @task_type, @taskresult, @dateofcreating, @datasource, @lastupdate)";
            using (NpgsqlConnection myCon = new NpgsqlConnection(dbConn))
            {
                myCon.Open();
                string[] queueData;
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@userid", userid);
                    myCommand.Parameters.AddWithValue("@taskid", taskid);
                    myCommand.Parameters.AddWithValue("@completed", false);
                    myCommand.Parameters.AddWithValue("@task_type", task_type);
                    myCommand.Parameters.AddWithValue("@datasource", "SAP");
                    myCommand.Parameters.AddWithValue("@job_status", "fromSap");
                    myCommand.Parameters.AddWithValue("@taskresult", "TIMEOUTERROR|Сервис временно не работает");
                    myCommand.Parameters.AddWithValue("@dateofcreating", DateTime.Now);
                    myCommand.Parameters.AddWithValue("@lastupdate", DateTime.Now);
                    myCommand.ExecuteReader();
                    myCon.Close();
                    await UpdateQueueCompleateAsync(taskid, dbConn);
                }

                myCon.Open();


            }
        }
        public static async Task UpdateQueueCompleateAsync(string task_id, string _dbConn)
        { 
            string updateQeueueQuery = @"update queue set status ='completed', lastupdate = @lastupdate where taskid = @taskid";

            using (NpgsqlConnection myCon = new NpgsqlConnection(_dbConn))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(updateQeueueQuery, myCon))
                {
                    myCommand.Parameters.AddWithValue("@taskid", task_id);
                    myCommand.Parameters.AddWithValue("@lastupdate", DateTime.Now);
                    myCommand.ExecuteReader();
                    myCon.Close();
                }
            }
            
        }
        public bool UpdateQueueToCompleate(string task_id, string _dbConn)
        { 
            string updateQeueueQuery = @"update queue set status ='completed', lastupdate = @lastupdate where taskid = @taskid";

            using (NpgsqlConnection myCon = new NpgsqlConnection(_dbConn))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(updateQeueueQuery, myCon))
                {
                    myCommand.Parameters.AddWithValue("@taskid", task_id);
                    myCommand.Parameters.AddWithValue("@lastupdate", DateTime.Now);
                    myCommand.ExecuteReader();
                    myCon.Close();
                }
            }

            return true;
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
                        RequestUri = new Uri(url + urlPath),
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
        
        private async Task SendWebRequestSurveyAsync(string urlPath, string jsonString)
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
                        RequestUri = new Uri(urlPath),
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
        
        private async Task<string> Send1CRequestAsync(string jsonString)
        {
            try
            {
                string loginPass = _configuration.GetConnectionString("1CLoginPass");
                string url =  _configuration.GetConnectionString("1CURL");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic", Convert.ToBase64String(
                                System.Text.ASCIIEncoding.ASCII.GetBytes(
                                    loginPass)));
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(url),
                        Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
                    };
                    
                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    logger.LogInformation($"RESPONSE BODY: { responseBody }");
                    return responseBody;
                    
                }
            }
            catch (HttpRequestException e)
            {
                logger.LogInformation($"Error: {e.Message }");
            }

            return "Error";
        }

        private async Task<bool> Add1CResultToDB(ResultFrom1c result, string userId, string task_type)
        {
            string temp = "";
            string payload = "";
            string query = @"insert into sap_results(userid, taskid, completed,job_status, task_type, taskresult, dateofcreating, datasource, lastupdate) 
                                  VALUES (@userid, @taskid, @completed,@job_status, @task_type, @taskresult, @dateofcreating, @datasource, @lastupdate)";
            
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("DBConnect");

            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    
                    if (result.payload.result != null)
                    {
                        payload = payload + "|" + result.payload.result;
                    }
                    if (result.payload.text != null)
                    {
                        payload = payload + "|" + result.payload.text;
                    }
                    if (result.payload.periods != null)
                    {
                        foreach (Period1CRecieve period in result.payload.periods)
                        {
                            temp = temp + "/@" + period.key + "/#" + period.value;
                        }
                        payload = payload + "|" + temp;
                    }
                    
                    if (result.payload.error_text != null)
                    {
                        payload = payload + "|" + result.payload.error_text;
                    }
                    if (result.payload.error_code != null)
                    {
                        payload = payload + "|" + result.payload.error_code;
                    }
                    
                    if (result.payload.file != null)
                    {
                        payload = payload + "|" + "File Saved";
                        //save file to postgresql


                        saveToDB(result.payload.file, result.task_id);
                    }

                    myCommand.Parameters.AddWithValue("@userid", userId);
                    myCommand.Parameters.AddWithValue("@taskid", result.task_id);
                    myCommand.Parameters.AddWithValue("@completed", result.completed);
                    myCommand.Parameters.AddWithValue("@task_type", task_type);
                    myCommand.Parameters.AddWithValue("@datasource", "1C");
                    myCommand.Parameters.AddWithValue("@job_status", "fromSap");
                    myCommand.Parameters.AddWithValue("@taskresult", payload);
                    myCommand.Parameters.AddWithValue("@dateofcreating", DateTime.Now);
                    myCommand.Parameters.AddWithValue("@lastupdate", DateTime.Now);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
                
            }

            

            return true;
        }
        public bool saveToDB(string file, string taskid)
        {
            byte[] bytes = Convert.FromBase64String(file);
            
            var cs = _configuration.GetConnectionString("DBConnect");
            using var con = new NpgsqlConnection(cs);
            con.Open();
            
            using (NpgsqlCommand cmdUpload = new NpgsqlCommand())
            {
                cmdUpload.CommandText = "insert into saved_files(taskid, file, status,createtime) values(@taskid, @file,@status,@createtime) ";
                cmdUpload.Parameters.AddWithValue("@taskid", taskid);
                cmdUpload.Parameters.AddWithValue("@file", bytes);
                cmdUpload.Parameters.AddWithValue("@status", "created");
                cmdUpload.Parameters.AddWithValue("@createtime", DateTime.Now);
                cmdUpload.Connection = con;
                cmdUpload.ExecuteNonQuery();
                con.Close();
            }
            return true;

        }
    }
      
}
