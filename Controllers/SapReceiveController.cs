using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestService.Models;
using System;
using System.Data;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

// using NLog;
// using NLog.Web;


namespace RestService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SapReceiveController : ControllerBase
    {
        // private Logger _logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        

        private readonly IConfiguration _configuration;

        public SapReceiveController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // [HttpPost]
        // public JsonResult Post(SurveyModel sap)
        // {
        //     DataTable table = new DataTable();
        //     string sqlDataSource = _configuration.GetConnectionString("DBConnect");
        //     NpgsqlDataReader myReader;
        //
        //     using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
        //     {
        //         string[] queueData;
        //         string temp = "";
        //         string payload = "";
        //
        //         myCon.Open();
        //         string survey_id = "test";
        //             if (sap.task != null)
        //             {
        //                 if (sap.task.payload.phones != null)
        //             {
        //                 foreach (string phone in sap.task.payload.phones)
        //                 {
        //                     temp = temp + ";" + phone;
        //
        //                 }
        //                 payload = payload + "|" + temp;
        //             }
        //             
        //             if (sap.task.payload.TaskInternalId != null)
        //             {
        //                 payload = payload + "|" + sap.task.payload.TaskInternalId;
        //             }
        //             
        //             if (sap.task.payload.survey_id != null)
        //             {
        //                 payload = payload + "|" + sap.task.payload.survey_id;
        //                 survey_id = sap.task.payload.survey_id;
        //             }
        //             
        //                 string queryNotifications = @"insert into notifications(task_type, task_uuid, status,result,date, phones) 
        //                               VALUES (@task_type, @task_uuid, @status,@result,@date, @phones)";
        //                 using (NpgsqlCommand myCommandNotofications = new NpgsqlCommand(queryNotifications, myCon))
        //                 {
        //                     //_logger.Trace($"Notification came from SAP: {payload}  ");
        //                     //queueData = GetQueueData(sap.tasks.task_id);
        //                     string payloads = JsonConvert.SerializeObject(sap.task.payload, 
        //                         Newtonsoft.Json.Formatting.None, 
        //                         new JsonSerializerSettings { 
        //                             NullValueHandling = NullValueHandling.Ignore
        //                         });
        //                     myCommandNotofications.Parameters.AddWithValue("@task_type", sap.task.task_type);
        //                     myCommandNotofications.Parameters.AddWithValue("@task_uuid", sap.task.payload.TaskInternalId);
        //                     myCommandNotofications.Parameters.AddWithValue("@status", "true");
        //                     myCommandNotofications.Parameters.AddWithValue("@result", payload);
        //                     myCommandNotofications.Parameters.AddWithValue("@phones", sap.task.payload.phones);
        //                     myCommandNotofications.Parameters.AddWithValue("@date", DateTime.Now);
        //                     myReader = myCommandNotofications.ExecuteReader();
        //                     table.Load(myReader);
        //                     myReader.Close();
        //                     myCon.Close();
        //                     return new JsonResult("Added succesfully =)");
        //                 }
        //            
        //             
        //             }
        //         
        //         
        //     }
        //
        //     return new JsonResult("Added succesfully =)");
        //
        // }
        //

        [HttpPost]
        public JsonResult Post(SapReceive sap)
        {
            // MappedDiagnosticsLogicalContext.Set("User", "undefined");
            // MappedDiagnosticsLogicalContext.Set("Sessionid", "undefined");
            // _logger.Trace("sap result getting to rest server ");

            string temp = "";
            string payload = "";
            string task_type = "";
            string query = @"insert into sap_results(userid, taskid, completed,job_status, task_type, taskresult, dateofcreating, datasource, lastupdate) 
                                  VALUES (@userid, @taskid, @completed,@job_status, @task_type, @taskresult, @dateofcreating, @datasource, @lastupdate)";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("DBConnect");
            NpgsqlDataReader myReader;
            
            
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                string[] queueData;

                myCon.Open();
                
                string tesk_id = "test";
                    if (sap.task != null)
                    {
                        if (sap.task.payload.phones != null)
                        {
                            foreach (string phone in sap.task.payload.phones)
                            {
                                temp = temp + ";" + phone;

                            }
                            payload = payload + "|" + temp;
                        }
                    
                    if (sap.task.payload.text != null)
                    {
                        payload = payload + "|" + sap.task.payload.text;
                    }
                    if (sap.task.payload.task_internal_id != null)
                    {
                        payload = payload + "|" + sap.task.payload.task_internal_id;
                        tesk_id = sap.task.payload.task_internal_id;
                    }
                    
                        string queryNotifications = @"insert into notifications(task_type, task_uuid, status,result,date, phones) 
                                      VALUES (@task_type, @task_uuid, @status,@result,@date, @phones)";
                        using (NpgsqlCommand myCommandNotofications = new NpgsqlCommand(queryNotifications, myCon))
                        {
                            //_logger.Trace($"Notification came from SAP: {payload}  ");
                            //queueData = GetQueueData(sap.tasks.task_id);
                            string payloads = JsonConvert.SerializeObject(sap.task.payload, 
                                Newtonsoft.Json.Formatting.None, 
                                new JsonSerializerSettings { 
                                    NullValueHandling = NullValueHandling.Ignore
                                });
                            myCommandNotofications.Parameters.AddWithValue("@task_type", sap.task.task_type);
                            myCommandNotofications.Parameters.AddWithValue("@task_uuid", sap.task.payload.task_internal_id);
                            myCommandNotofications.Parameters.AddWithValue("@status", "true");
                            myCommandNotofications.Parameters.AddWithValue("@result", payload);
                            myCommandNotofications.Parameters.AddWithValue("@phones", sap.task.payload.phones);
                            myCommandNotofications.Parameters.AddWithValue("@date", DateTime.Now);
                            myReader = myCommandNotofications.ExecuteReader();
                            table.Load(myReader);
                            myReader.Close();
                            myCon.Close();
                            return new JsonResult("Added succesfully =)");
                        }
                   
                    
                    }
                
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
  
                    
                    if (sap.tasks.payload.periods != null)
                    {
                        foreach (Period period in sap.tasks.payload.periods)
                        {
                            temp = temp + ";" + period.key + ":" + period.value;
                        }
                        payload = payload + "|" + temp;
                    }
                    if (sap.tasks.payload.locations != null)
                    {
                        temp = "";
                        foreach (Location location in sap.tasks.payload.locations)
                        {
                            temp = temp + ";" + location.key + ":" + location.value;
                        }
                        payload = payload + "|" + temp;
                    }
                    if (sap.tasks.payload.requests != null)
                    {
                        temp = "";
                        foreach (Requests requests in sap.tasks.payload.requests)
                        {
                            temp = temp + "/#" + requests.key + "//@" + requests.name + "//@" + requests.status + "//@" + requests.info;
                        }
                        payload = payload + "|" + temp;
                    }
                    if (sap.tasks.payload.error_text != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.error_text;
                    }
                    if (sap.tasks.payload.error_code != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.error_code;
                    }
                    if (sap.tasks.payload.result != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.result;
                    }
                    if (sap.tasks.payload.response != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.response;
                    }
                    if (sap.tasks.payload.text != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.text;
                    }
                    
                    if (sap.tasks.payload.doc_types != null)
                    {
                        temp = "";
                        foreach (var i in sap.tasks.payload.doc_types)
                        {
                            temp = temp + ";" + i;
                        }
                        payload = payload + "|" + temp;
                    }
                    if (sap.tasks.payload.url != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.url;
                        //save file to postgresql


                        saveToDB(sap.tasks.payload.url, sap.tasks.task_id);
                    }

                    

                    queueData = GetQueueData(sap.tasks.task_id);
                    myCommand.Parameters.AddWithValue("@userid", queueData[0]);
                    myCommand.Parameters.AddWithValue("@taskid", sap.tasks.task_id);
                    myCommand.Parameters.AddWithValue("@completed", sap.tasks.completed);
                    myCommand.Parameters.AddWithValue("@task_type", queueData[1]);
                    myCommand.Parameters.AddWithValue("@datasource", queueData[2]);
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
            return new JsonResult("Added succesfully =)");


        }

        public string[] GetQueueData(string taskid)
        {
            string[] arr = new[] {"test", "test", "test"};
            if (taskid.Contains("TEST"))
            {
                return arr;
            }
            var cs = _configuration.GetConnectionString("DBConnect");
            using var con = new NpgsqlConnection(cs);
            NpgsqlDataReader myReader;
            DataTable table = new DataTable();
    

            con.Open();
            using (NpgsqlCommand cmdUpload = new NpgsqlCommand())
            {
               
                cmdUpload.CommandText = "select userid, task_type,datasource from queue where taskid =@taskid";
                
                cmdUpload.Parameters.AddWithValue("@taskid", taskid);
                cmdUpload.Connection = con;
                myReader = cmdUpload.ExecuteReader();
                table.Load(myReader);
                con.Close();
            }
            string userid = Convert.ToString( table.Rows[0]["userid"]);
            string task_type = Convert.ToString( table.Rows[0]["task_type"]);
            string datasource = Convert.ToString( table.Rows[0]["datasource"]);
            string[] arrAns = new[] {userid, task_type, datasource};
            return arrAns;
        }
        public bool saveToDB(string url, string taskid)
        {
            WebClient myWebClient = new WebClient();
            byte[] bytes = myWebClient.DownloadData(url);
            
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
                // con.Open();
                cmdUpload.ExecuteNonQuery();
                con.Close();
            }
            return true;

        }
    }
}


