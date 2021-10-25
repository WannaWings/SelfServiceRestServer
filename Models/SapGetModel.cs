using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    public class Payloads
    {
        public string phone { get; set; }
       
    }

    public class GetTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public Payloads payload { get; set; }
    }

    public class SapGetModel
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<GetTasks> tasks { get; set; }
    }
}
