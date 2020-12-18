using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class InputVariables
    {
        private string id;
        private string sourceId;
        private string destinationId;
        private string name;

        private double measured;
        private Constraints metrologicRange;
        private Constraints technologicRange;
        private double tolerance;

        private bool isMeasured;
        private bool isExcluded;

        public void setValues(string id, string sourceId, string destinationId, double measured, Constraints metrologicRange, Constraints technologicRange, double tolerance, bool isMeasured, bool isExcluded)
        {
            setId(id);
            setSourceId(sourceId);
            setDestinationId(destinationId);
            setMeasured(measured);
            setMetrologicRange(new Constraints(metrologicRange.getLowerBound(), metrologicRange.getUpperBound()));
            setTechnologicRange(new Constraints(technologicRange.getLowerBound(), technologicRange.getUpperBound()));
            setTolerance(tolerance);
            setIsMeasured(isMeasured);
            setIsExcluded(isExcluded);
        }

        public double getMeasured()
        {
            return measured;
        }

        public void setMeasured(double measured)
        {
            this.measured = measured;
        }

        public double getTolerance()
        {
            return tolerance;
        }

        public void setTolerance(double tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool getIsExcluded()
        {
            return isExcluded;
        }

        public void setIsExcluded(bool isExcluded)
        {
            this.isExcluded = isExcluded;
        }

        public bool getIsMeasured()
        {
            return isMeasured;
        }

        public void setIsMeasured(bool isMeasured)
        {
            this.isMeasured = isMeasured;
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public Constraints getMetrologicRange()
        {
            return metrologicRange;
        }

        public void setMetrologicRange(Constraints metrologicRange)
        {
            this.metrologicRange = metrologicRange;
        }

        public Constraints getTechnologicRange()
        {
            return technologicRange;
        }

        public void setTechnologicRange(Constraints technologicRange)
        {
            this.technologicRange = technologicRange;
        }

        public string getId()
        {
            return id;
        }

        public void setId(string id)
        {
            this.id = id;
        }

        public string getSourceId()
        {
            return sourceId;
        }

        public void setSourceId(string sourceId)
        {
            this.sourceId = sourceId;
        }

        public string getDestinationId()
        {
            return destinationId;
        }

        public void setDestinationId(string destinationId)
        {
            this.destinationId = destinationId;
        }
    }

}
