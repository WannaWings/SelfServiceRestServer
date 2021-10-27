using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    public class OrderNote_PlaceListRequestPayloads
    {
        public string phone { get; set; }
        public string doc_type { get; set; }
       
    }

    public class OrderNote_PlaceListRequestTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public OrderNote_PlaceListRequestPayloads payload { get; set; }
    }

    public class OrderNote_PlaceListRequestModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<OrderNote_PlaceListRequestTasks> tasks { get; set; }
    }
}
