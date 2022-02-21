using System.Collections.Generic;


namespace RestService.Models
{
    public class SurveyAnswerSendModelPayloads
    {
        public string phone { get; set; }
        public string  survey_id { get; set; }
        public List<QuestionsSurveyModel> questions { get; set; }
    }

    public class SurveyAnswerSendModelTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public SurveyAnswerSendModelPayloads payload { get; set; }
    }

    public class SurveyAnswerSendModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<SurveyAnswerSendModelTasks> tasks { get; set; }
    }

    public class QuestionsSurveyModel
    {
        public string key { get; set; }
        public string[] answers { get; set; }
        public string text { get; set; }

    }
}
