using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malshinon09._06.models
{
    
        internal class Alert
        {
            public int Id { get; set; }
            public int TargetId { get; set; }
            public string AlertType { get; set; } // HIGH_RISK, BURST_REPORTS
            public string Reason { get; set; }
            public string TimeWindow { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    
}
