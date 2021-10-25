using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    public class Queue
    {
        public int QueueID { get;set; }
        public string UserID { get;set; }
        public string TaskID { get;set; }
        public string TaskType { get;set; }
        public string State { get;set; }
        public string taskbody { get;set; }
        public string Status { get;set; }
        public DateTime DateOfCreating{ get;set; }
    }
}
