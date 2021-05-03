using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class TreeElement
    {
        public TreeElement()
        {
        }

        public TreeElement(List<(int, int, int)> flows, double globalTestValue)
        {
            Flows = flows;
            GlobalTestValue = globalTestValue;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public List<(int, int, int)> Flows { get; } = new List<(int, int, int)>();

        public double GlobalTestValue { get; set; }
        public double metrologicUpperBound { get; set; }
        public double metrologicLowerBound { get; set; }
        public double technologicUpperBound { get; set; }
        public double technologicLowerBound { get; set; }
    }
}
