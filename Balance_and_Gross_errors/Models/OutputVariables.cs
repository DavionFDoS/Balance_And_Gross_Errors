using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class OutputVariables
    {
        private string id;
        private double value;
        private string name;
        private string source;
        private string target;

        public string getId()
        {
            return id;
        }

        public void setId(string id)
        {
            this.id = id;
        }

        public double getValue()
        {
            return value;
        }

        public void setValue(double value)
        {
            this.value = value;
        }

        public OutputVariables(string id, string name, double value, string source, string target)
        {
            setId(id);
            setValue(value);
            setName(name);
            setSource(source);
            setTarget(target);
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getSource()
        {
            return source;
        }

        public void setSource(string source)
        {
            this.source = source;
        }

        public string getTarget()
        {
            return target;
        }

        public void setTarget(string target)
        {
            this.target = target;
        }
    }


}
