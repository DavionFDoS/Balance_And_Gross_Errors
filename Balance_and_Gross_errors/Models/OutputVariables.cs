using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class OutputVariables
    {
        public string id { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public string name { get; set; }
        public double value { get; set; }

        public double upperBound { get; set; }

        public double lowerBound { get; set; }

    }
}
