using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class Constraints
    {
        private double upperBound;
        private double lowerBound;
        public Constraints()
        {
            setLowerBound(0.0);
            setUpperBound(0.0);
        }
        public Constraints(double lowerBound, double upperBound)
        {
            setLowerBound(lowerBound);
            setUpperBound(upperBound);
        }
        public double getUpperBound()
        {
            return upperBound;
        }

        public void setUpperBound(double upperBound)
        {
            this.upperBound = upperBound;
        }

        public double getLowerBound()
        {
            return lowerBound;
        }

        public void setLowerBound(double lowerBound)
        {
            this.lowerBound = lowerBound;
        }
    }

}
