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
    public class QueueController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public QueueController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"select * from queue";


            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using(NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
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
        public JsonResult Post(Queue que)
        {
            string query = @"insert into queue(userid, taskid, taskbody, status, dateofcreating) 
                                  VALUES (@userid, @taskid, @taskbody, @status, @dateofcreating)";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@userid", que.UserID);
                    myCommand.Parameters.AddWithValue("@taskid", que.TaskID);
                    myCommand.Parameters.AddWithValue("@taskbody", que.taskbody);
                    myCommand.Parameters.AddWithValue("@status", que.Status);
                    myCommand.Parameters.AddWithValue("@dateofcreating", Convert.ToDateTime(que.DateOfCreating));
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Added succesfully =)");


        }

        [HttpPut]
        public JsonResult Put(Queue que)
        {
            string query = @"update queue
                            set status = @status
                            where taskid = @taskid";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("RestServerAppCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    
                    myCommand.Parameters.AddWithValue("@taskid", que.TaskID);
                    myCommand.Parameters.AddWithValue("@status", que.Status);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Updated");
        }

        [HttpDelete("{taskid}")]
        public JsonResult Delete(string taskid)
        {
            string query = @"delete from queue
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
