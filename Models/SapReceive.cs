using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestService.Models
{
    public class Period
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class Location
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class Requests
    {
        public string key { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string info { get; set; }
    }
    public class Payload
    {
        public string result { get; set; }
        public bool valid { get; set; }
        public string url { get; set; }
        public string[] doc_types { get; set; }
        public List<Period> periods { get; set; }
        public List<Location> locations { get; set; }
        public List<Requests> requests { get; set; }
        public string response { get; set; }
        public string text { get; set; }
        public string phone { get; set; }
        public string error_code { get; set; }
        public string error_text { get; set; }
    }

    public class Tasks
    {
        public bool completed { get; set; }
        public string task_id { get; set; }
        public Payload payload { get; set; }
    }

    public class SapReceive
    {
        public Tasks tasks { get; set; }
    }
}
