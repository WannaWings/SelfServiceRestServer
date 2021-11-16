using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestService.Models;
using System;
using System.Data;
using System.Net;
using Microsoft.AspNetCore.Authorization;


namespace RestService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SapReceiveController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SapReceiveController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        public JsonResult Post(SapReceive sap)
        {
            string temp = "";
            string payload = "";
            string query = @"insert into sap_results(userid, taskid, completed,job_status, taskbody, taskresult, dateofcreating) 
                                  VALUES (@userid, @taskid, @completed,@job_status, @taskbody, @taskresult, @dateofcreating)";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
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
                    // if (sap.tasks.payload.valid != null)
                    // {
                    //     payload = payload + "|" + sap.tasks.payload.valid;
                    // }
                    if (sap.tasks.payload.url != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.url;
                        //save file to postgresql


                        saveToDB(sap.tasks.payload.url, sap.tasks.task_id);
                    }
                    
                    myCommand.Parameters.AddWithValue("@userid", "defoult");
                    myCommand.Parameters.AddWithValue("@taskid", sap.tasks.task_id);
                    myCommand.Parameters.AddWithValue("@completed", sap.tasks.completed);
                    myCommand.Parameters.AddWithValue("@taskbody", "defoult");
                    myCommand.Parameters.AddWithValue("@job_status", "fromSap");
                    myCommand.Parameters.AddWithValue("@taskresult", payload);
                    myCommand.Parameters.AddWithValue("@dateofcreating", DateTime.Now);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Added succesfully =)");


        }
        public static bool saveToDB(string url, string taskid)
        {
            WebClient myWebClient = new WebClient();
            byte[] bytes = myWebClient.DownloadData(url);

            var cs = "Host=10.55.60.160;Username=m.valiev;Password=pa55w0rd!;Database=Portal.Bot";
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
