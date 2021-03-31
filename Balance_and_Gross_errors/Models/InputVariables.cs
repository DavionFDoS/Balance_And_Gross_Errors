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
        public double metrologicUpperBound; 
        public double MetrologicUpperBound
        {
            set
            {
                if ((value < 0) || (metrologicLowerBound >metrologicUpperBound))
                {
                    throw new ArgumentException(nameof(metrologicUpperBound));
                }
                else
                {
                    metrologicUpperBound = value;
                }
            }
            get { return metrologicUpperBound; }
        }

        public double metrologicLowerBound;
        public double MetrologicLowerBound
        {
            set
            {
                if ((value < 0) || (metrologicLowerBound > metrologicUpperBound))
                {
                    throw new ArgumentException(nameof(metrologicLowerBound));
                }
                else
                {
                    metrologicLowerBound = value;
                }
            }
            get { return metrologicLowerBound; }
        }
        public double technologicUpperBound;
        public double TechnologicUpperBound
        {
            set
            {
                if ((value < 0) || (technologicLowerBound > technologicUpperBound))
                {
                    throw new ArgumentException(nameof(technologicUpperBound));
                }
                else
                {
                    technologicUpperBound = value;
                }
            }
            get { return technologicUpperBound; }
        }
        public double technologicLowerBound;
        public double TechnologicLowerBound
        {
            set
            {
                if ((value < 0) || (technologicLowerBound > technologicUpperBound))
                {
                    throw new ArgumentException(nameof(technologicLowerBound));
                }
                else
                {
                    technologicLowerBound = value;
                }
            }
            get { return technologicLowerBound; }
        }

        public double tolerance { get; set; }

        public bool isMeasured { get; set; }
        public bool isExcluded { get; set; }
        public bool useTechnologic { get; set; }        
    }
}
