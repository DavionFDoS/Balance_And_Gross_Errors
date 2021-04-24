using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class Glr
    {
        public List<Flow> FlowErrors { get; set; }

        public List<InputVariables> FlowCorrections { get; set; }

        public double GlobalTestValue { get; set; }
    }
}
