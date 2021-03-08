using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class InputVariables
    {
        public string id { get; set; }
        public string sourceId { get; set; }
        public string destinationId { get; set; }
        public string name { get; set; }

        public double measured { get; set; }
        //public Constraints metrologicRange { get;  set; }
        //public Constraints technologicRange { get;  set; }
        public double metrologicUpperBound { get; set; }
        public double metrologicLowerBound { get; set; }
        public double technologicUpperBound { get; set; }
        public double technologicLowerBound { get; set; }
        public double tolerance { get; set; }

        public bool isMeasured { get; set; }
        public bool isExcluded { get; set; }
        //public InputVariables()
        //{
        //    metrologicRange = new Constraints();
        //    technologicRange = new Constraints();
        //}           
    }
}
