// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using Npgsql;
// using RestService.Models;
// using System;
// using System.Data;
// using System.Net;
// using Microsoft.AspNetCore.Authorization;
//
//
// namespace RestService.Controllers
// {
//     [Authorize]
//     [Route("api/[controller]")]
//     [ApiController]
//     public class OneCReceiveController : ControllerBase
//     {
//         private readonly IConfiguration _configuration;
//
//         public OneCReceiveController(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }
//
//
//         [HttpPost]
//         public JsonResult Post(OneCReceive OneCModel)
//         {
//             string temp = "";
//             string payload = "";
//             string query = @"insert into OneCModel_results(userid, taskid, completed,job_status, taskbody, taskresult, dateofcreating, datecamefrom, lastupdate) 
//                                   VALUES (@userid, @taskid, @completed,@job_status, @taskbody, @taskresult, @dateofcreating, @datecamefrom, @lastupdate)";
//
//             DataTable table = new DataTable();
//             string sqlDataSource = _configuration.GetConnectionString("DBConnect");
//             NpgsqlDataReader myReader;
//             using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
//             {
//                 myCon.Open();
//                 using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
//                 {
//                     
//                     if (OneCModel.tasks.payload.periods != null)
//                     {
//                         foreach (Period period in OneCModel.tasks.payload.periods)
//                         {
//                             temp = temp + ";" + period.key + ":" + period.value;
//                         }
//                         payload = payload + "|" + temp;
//                     }
//                     if (OneCModel.tasks.payload.locations != null)
//                     {
//                         temp = "";
//                         foreach (Location location in OneCModel.tasks.payload.locations)
//                         {
//                             temp = temp + ";" + location.key + ":" + location.value;
//                         }
//                         payload = payload + "|" + temp;
//                     }
//                     if (OneCModel.tasks.payload.requests != null)
//                     {
//                         temp = "";
//                         foreach (Requests requests in OneCModel.tasks.payload.requests)
//                         {
//                             temp = temp + "/#" + requests.key + "//@" + requests.name + "//@" + requests.status + "//@" + requests.info;
//                         }
//                         payload = payload + "|" + temp;
//                     }
//                     if (OneCModel.tasks.payload.error_text != null)
//                     {
//                         payload = payload + "|" + OneCModel.tasks.payload.error_text;
//                     }
//                     if (OneCModel.tasks.payload.error_code != null)
//                     {
//                         payload = payload + "|" + OneCModel.tasks.payload.error_code;
//                     }
//                     if (OneCModel.tasks.payload.result != null)
//                     {
//                         payload = payload + "|" + OneCModel.tasks.payload.result;
//                     }
//                     if (OneCModel.tasks.payload.response != null)
//                     {
//                         payload = payload + "|" + OneCModel.tasks.payload.response;
//                     }
//                     if (OneCModel.tasks.payload.text != null)
//                     {
//                         payload = payload + "|" + OneCModel.tasks.payload.text;
//                     }
//                     if (OneCModel.tasks.payload.doc_types != null)
//                     {
//                         temp = "";
//                         foreach (var i in OneCModel.tasks.payload.doc_types)
//                         {
//                             temp = temp + ";" + i;
//                         }
//                         payload = payload + "|" + temp;
//                     }
//                     // if (OneCModel.tasks.payload.valid != null)
//                     // {
//                     //     payload = payload + "|" + OneCModel.tasks.payload.valid;
//                     // }
//                     if (OneCModel.tasks.payload.url != null)
//                     {
//                         payload = payload + "|" + OneCModel.tasks.payload.url;
//                         //save file to postgresql
//
//
//                         saveToDB(OneCModel.tasks.payload.url, OneCModel.tasks.task_id);
//                     }
//                     
//                     myCommand.Parameters.AddWithValue("@userid", "defoult");
//                     myCommand.Parameters.AddWithValue("@taskid", OneCModel.tasks.task_id);
//                     myCommand.Parameters.AddWithValue("@completed", OneCModel.tasks.completed);
//                     myCommand.Parameters.AddWithValue("@taskbody", "defoult");
//                     myCommand.Parameters.AddWithValue("@job_status", "fromOneCModel");
//                     myCommand.Parameters.AddWithValue("@taskresult", payload);
//                     myCommand.Parameters.AddWithValue("@dateofcreating", DateTime.Now);
//                     myCommand.Parameters.AddWithValue("@lastupdate", DateTime.Now);
//                     myCommand.Parameters.AddWithValue("@datecamefrom", "1C");
//                     myReader = myCommand.ExecuteReader();
//                     table.Load(myReader);
//                     myReader.Close();
//                     myCon.Close();
//                 }
//             }
//             return new JsonResult("Added succesfully =)");
//
//
//         }
//         public bool saveToDB(string url, string taskid)
//         {
//             WebClient myWebClient = new WebClient();
//             byte[] bytes = myWebClient.DownloadData(url);
//
//             var cs = _configuration.GetConnectionString("DBConnect");
//             using var con = new NpgsqlConnection(cs);
//             con.Open();
//             
//             using (NpgsqlCommand cmdUpload = new NpgsqlCommand())
//             {
//                
//                 cmdUpload.CommandText = "insert into saved_files(taskid, file, status,createtime) values(@taskid, @file,@status,@createtime) ";
//                 
//                 cmdUpload.Parameters.AddWithValue("@taskid", taskid);
//                 cmdUpload.Parameters.AddWithValue("@file", bytes);
//                 cmdUpload.Parameters.AddWithValue("@status", "created");
//                 cmdUpload.Parameters.AddWithValue("@createtime", DateTime.Now);
//                 cmdUpload.Connection = con;
//                 // con.Open();
//                 cmdUpload.ExecuteNonQuery();
//                 con.Close();
//             }
//             return true;
//
//         }
//     }
// }
