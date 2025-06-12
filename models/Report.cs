using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malshinon09._06.models
{
    internal class Report
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }// ID of the person who reported Foregin key to Person.Id
        public int TargetId { get; set; } // ID of the person being reported Foregin key to Person.Id
        public string ReportText { get; set; } // Text of the report
        public DateTime SubmittedAt { get; set; } // Timestamp of the report
        //public Person Reporter { get; set; }
        //public Person Target { get; set; }
    }
}
