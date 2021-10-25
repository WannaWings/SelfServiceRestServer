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

namespace RestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SapResultsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SapResultsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"select  * from sap_results";



            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);


        }


        [HttpPost]
        public JsonResult Post(SapResults sap)
        {
            string query = @"insert into sap_results(userid, taskid, completed, taskbody, taskresult, dateofcreating) 
                                  VALUES (@userid, @taskid, @completed, @taskbody, @taskresult, @dateofcreating)";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@userid", sap.UserID);
                    myCommand.Parameters.AddWithValue("@taskid", sap.TaskID);
                    myCommand.Parameters.AddWithValue("@completed", sap.Completed);
                    myCommand.Parameters.AddWithValue("@taskbody", sap.taskbody);
                    myCommand.Parameters.AddWithValue("@job_status", sap.job_status);
                    myCommand.Parameters.AddWithValue("@taskresult", sap.TaskResult);
                    myCommand.Parameters.AddWithValue("@dateofcreating", Convert.ToDateTime(sap.DateOfCreating));
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Added succesfully =)");


        }


        [HttpDelete("{taskid}")]
        public JsonResult Delete(string taskid)
        {
            string query = @"delete from sap_results
                            where taskid = @taskid";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {

                    myCommand.Parameters.AddWithValue("@taskid", taskid);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("deleted");
        }


    }
}
