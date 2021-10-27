using System.Collections.Generic;


namespace RestService.Models
{
    public class RestOfVacationRequestModelPayloads
    {
        public string phone { get; set; }
        public string date { get; set; }
    }

    public class RestOfVacationRequestModelTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public RestOfVacationRequestModelPayloads payload { get; set; }
    }

    public class RestOfVacationRequestModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<RestOfVacationRequestModelTasks> tasks { get; set; }
    }
}
