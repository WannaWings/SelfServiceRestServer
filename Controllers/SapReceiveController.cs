using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
                            temp = temp + ";" + requests.key + ":" + requests.name + ":" + requests.status + ":" + requests.info;
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
                    if (sap.tasks.payload.url != null)
                    {
                        payload = payload + "|" + sap.tasks.payload.url;
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
    }
}
