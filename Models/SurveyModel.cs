using System.Collections.Generic;
using Newtonsoft.Json;

namespace RestService.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Answer
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Question
    {
        public string question { get; set; }
        public string key { get; set; }
        public bool free_text { get; set; }
        public bool multiple { get; set; }
        public List<Answer> answers { get; set; }
    }

    public class PayloadSurver
    {
        public List<string> phones { get; set; }

        [JsonProperty("task_internal_id")]
        public string TaskInternalId { get; set; }
        public string survey_id { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string title { get; set; }
        public List<Question> questions { get; set; }
    }

    public class Task
    {
        public string task_type { get; set; }
        public PayloadSurver payload { get; set; }
    }

    public class SurveyModel
    {
        public Task task { get; set; }
    }


}