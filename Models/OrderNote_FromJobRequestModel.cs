using System.Collections.Generic;


namespace RestService.Models
{
    public class OrderNote_FromJobRequestModelPayloads
    {
        public string phone { get; set; }
        public string count { get; set; }
        public string location { get; set; }
        public bool vacation { get; set; }
        public string vacation_period { get; set; }
        public bool salary { get; set; }
    }

    public class OrderNote_FromJobRequestModelTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public OrderNote_FromJobRequestModelPayloads payload { get; set; }
    }

    public class OrderNote_FromJobRequestModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<OrderNote_FromJobRequestModelTasks> tasks { get; set; }
    }
}
