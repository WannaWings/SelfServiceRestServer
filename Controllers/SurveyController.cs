using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

// using NLog;
// using NLog.Web;


namespace RestService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        // private Logger _logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        

        private readonly IConfiguration _configuration;

        public SurveyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        public JsonResult Post(SurveyModel surveyBody)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("DBConnect");
            NpgsqlDataReader myReader;
        
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                //myDeserializedClass = JsonConvert.DeserializeObject<List<SurveyModel>>(surveyBody);
                /*
                { “task”: {
                        "task_type": "change_survey_status",
                        "payload": {
            		        "phones":[“<phone>”],
	                        "task_internal_id ": "<task_uuid>",
	                        “survey_key”:”<survey_key>”,
	                        “status”:”<status>”
                                }}}
                */

                if (surveyBody.task.task_type == "change_survey_status")
                { 
                    string updateQeueueQuery = @"update survey_members set status = @status where survey_id = @survey_id and phone = @phone";
                    using (NpgsqlCommand myCommand = new NpgsqlCommand(updateQeueueQuery, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@survey_id", surveyBody.task.payload.survey_key);
                        myCommand.Parameters.AddWithValue("@phone", surveyBody.task.payload.phones[0]);
                        myCommand.Parameters.AddWithValue("@status", surveyBody.task.payload.status);
                        myCommand.ExecuteReader();
                        myCon.Close();
                    }
                        
                    return new JsonResult("Added succesfully =)");
                }
                
                string querySurveyAdd = @"insert into surveys(survey_id, task_internal_id, title, start_date, end_date, created)
                                                VALUES (@survey_id, @task_internal_id, @title, @start_date, @end_date, @created);";
                using (NpgsqlCommand myCommandNotofications = new NpgsqlCommand(querySurveyAdd, myCon))
                {
                    if (surveyBody.task.payload.task_internal_id == null)
                        surveyBody.task.payload.task_internal_id = "null";
                    //_logger.Trace($"Notification came from SAP: {payload}  ");
                    //queueData = GetQueueData(sap.tasks.task_id);
                    // string payloads = JsonConvert.SerializeObject(surveyBody.task.payload, 
                    //     Newtonsoft.Json.Formatting.None, 
                    //     new JsonSerializerSettings { 
                    //         NullValueHandling = NullValueHandling.Ignore
                    //     });
                    myCommandNotofications.Parameters.AddWithValue("@survey_id", surveyBody.task.payload.survey_id);
                    myCommandNotofications.Parameters.AddWithValue("@task_internal_id", surveyBody.task.payload.task_internal_id);
                    myCommandNotofications.Parameters.AddWithValue("@title", surveyBody.task.payload.title);
                    myCommandNotofications.Parameters.AddWithValue("@start_date", Convert.ToDateTime(surveyBody.task.payload.start_date));
                    myCommandNotofications.Parameters.AddWithValue("@end_date",Convert.ToDateTime(surveyBody.task.payload.end_date));
                    myCommandNotofications.Parameters.AddWithValue("@created", DateTime.Now);
                    myReader = myCommandNotofications.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }

               
                
                string querySurveyQuestionsAdd = @"insert into survey_questions(survey_id, question_key, question_text, multiple, free_text, answers)
                                                    VALUES(@survey_id, @question_key, @question_text, @multiple, @free_text, @answers);";

                foreach (var question in surveyBody.task.payload.questions)
                {
                    string answers = JsonConvert.SerializeObject(question.answers,
                        Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    using (NpgsqlCommand myCommandNotofications = new NpgsqlCommand(querySurveyQuestionsAdd, myCon))
                    {
                        myCommandNotofications.Parameters.AddWithValue("@survey_id", surveyBody.task.payload.survey_id);
                        myCommandNotofications.Parameters.AddWithValue("@question_key", question.key);
                        myCommandNotofications.Parameters.AddWithValue("@question_text", question.question);
                        myCommandNotofications.Parameters.AddWithValue("@multiple", question.multiple);
                        myCommandNotofications.Parameters.AddWithValue("@free_text", question.free_text);
                        myCommandNotofications.Parameters.AddWithValue("@answers", answers);
                        myReader = myCommandNotofications.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                    }
                }
                string querySurveyMemmberAdd = @"insert into survey_members(survey_id, status, phone, title)
                                                    VALUES(@survey_id, @status, @phone, @title);";
                foreach (var phone in surveyBody.task.payload.phones)
                {
                    using (NpgsqlCommand myCommandNotofications = new NpgsqlCommand(querySurveyMemmberAdd, myCon))
                    {
                        myCommandNotofications.Parameters.AddWithValue("@survey_id", surveyBody.task.payload.survey_id);
                        myCommandNotofications.Parameters.AddWithValue("@status", "created");
                        myCommandNotofications.Parameters.AddWithValue("@phone", phone);
                        myCommandNotofications.Parameters.AddWithValue("@title", surveyBody.task.payload.title);
                        
                        myReader = myCommandNotofications.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                    }
                }

                
                
                
                myCon.Close();
            }
            return new JsonResult("Added succesfully =)");
        
        }

    }
}


