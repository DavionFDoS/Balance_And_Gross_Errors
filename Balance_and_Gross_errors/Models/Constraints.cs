using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class Constraints
    {
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
        public Constraints()
        {
            LowerBound = 0.0;
            UpperBound = 0.0;
        }

    }

}
