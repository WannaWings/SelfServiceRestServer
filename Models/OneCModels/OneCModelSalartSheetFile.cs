using System.Collections.Generic;

namespace RestService.Models
{
    public class OneCModelSalartSheetFile
    {
        
    }
    public class Payloads1CSalaryFile
    {
        public string snils { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string date { get; set; }
        public string period { get; set; }
       
    }

    public class Tasks1CSalaryFile
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public Payloads1CSalaryFile payload { get; set; }
    }

    public class SalartSheetFile
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<Tasks1CSalaryFile> tasks { get; set; }
    }
}