using System.Collections.Generic;


namespace RestService.Models
{
    public class OrderNote_2NDFLRequestModelPayloads
    {
        public string phone { get; set; }
        public string period { get; set; }
        public string count { get; set; }
        public string location { get; set; }
    }

    public class OrderNote_2NDFLRequestModelTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public OrderNote_2NDFLRequestModelPayloads payload { get; set; }
    }

    public class OrderNote_2NDFLRequestModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<OrderNote_2NDFLRequestModelTasks> tasks { get; set; }
    }
}
