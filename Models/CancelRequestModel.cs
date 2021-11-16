using System.Collections.Generic;

namespace RestService.Models
{
    public class CancelRequestModelPayloads
    {
        public string phone { get; set; }
        public string key { get; set; }
       
    }

    public class CancelRequestModelTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public CancelRequestModelPayloads payload { get; set; }
    }

    public class CancelRequestModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<CancelRequestModelTasks> tasks { get; set; }
    }
}