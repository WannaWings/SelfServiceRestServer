using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    public class SapResults
    {
        public int ResultID { get; set; }
        public string UserID { get; set; }
        public string TaskID { get; set; }
        public bool Completed { get; set; }
        public string taskbody { get; set; }
        public string job_status { get; set; }
        public string TaskResult { get; set; }
        public string DateOfCreating { get; set; }
    }
}

