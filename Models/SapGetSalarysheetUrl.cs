using System.Collections.Generic;


namespace RestService.Models
{
    public class SalarySheetUrlPayloads
    {
        public string phone { get; set; }
        public string period { get; set; }
    }

    public class SalarySheetUrlGetTasks
    {
        public string task_id { get; set; }
        public string task_type { get; set; }
        public string state { get; set; }
        public SalarySheetUrlPayloads payload { get; set; }
    }

    public class SapGetSalarysheetUrl
    {
        public string configuration { get; set; }
        public string queue { get; set; }
        public List<SalarySheetUrlGetTasks> tasks { get; set; }
    }
}
