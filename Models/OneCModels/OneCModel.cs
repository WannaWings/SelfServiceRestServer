using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    
    
    public class Payloads1C
    {
        public string snils { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string date { get; set; }
       
    }

    public class GetTasks1C
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public Payloads1C payload { get; set; }
    }

    public class SapGetModel1C
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<GetTasks1C> tasks { get; set; }
    }
}