using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malshinon09._06.models
{
    internal class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Secret_code { get; set; }
        public int num_reports { get; set; }
        public int num_mentions { get; set; }

        public DateTime CreatedAt { get; set; }

        //enum type values: reporter, target, both, potential_agent
    }
}
