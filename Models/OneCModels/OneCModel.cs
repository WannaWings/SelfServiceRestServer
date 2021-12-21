using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    public class OneCPeriod
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class OneCLocation
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class OneCRequests
    {
        public string key { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string info { get; set; }
    }
    public class OneCPayload
    {
        public string result { get; set; }
        public bool valid { get; set; }
        public string url { get; set; }
        public string[] doc_types { get; set; }
        public List<OneCPeriod> periods { get; set; }
        public List<OneCLocation> locations { get; set; }
        public List<OneCRequests> requests { get; set; }
        public string response { get; set; }
        public string text { get; set; }
        public string phone { get; set; }
        public string error_code { get; set; }
        public string error_text { get; set; }
    }

    public class OneCTasks
    {
        public bool completed { get; set; }
        public string task_id { get; set; }
        public OneCPayload payload { get; set; }
    }

    public class OneCReceive
    {
        public Tasks tasks { get; set; }
    }
}
