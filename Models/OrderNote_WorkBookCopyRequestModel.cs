using System.Collections.Generic;


namespace RestService.Models
{
    public class OrderNote_WorkBookCopyRequestTasksPayloads
    {
        public string phone { get; set; }
        public string count { get; set; }
        public string location { get; set; }
        public string goal { get; set; }
    }

    public class OrderNote_WorkBookCopyRequestTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public OrderNote_WorkBookCopyRequestTasksPayloads payload { get; set; }
    }

    public class OrderNote_WorkBookCopyRequestModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<OrderNote_WorkBookCopyRequestTasks> tasks { get; set; }
    }
}
