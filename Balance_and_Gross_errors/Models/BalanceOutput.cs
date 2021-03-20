using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class BalanceOutput
    {
        public double DisbalanceOriginal { get; set; }
        public double Disbalance { get; set; }

        private List<OutputVariables> balanceOutputVariables;
        public BalanceOutput()
        {
            this.balanceOutputVariables = new List<OutputVariables>();
        }
        public List<OutputVariables> BalanceOutputVariables
        {
            get
            {
                return balanceOutputVariables;
            }

            set
            {
                foreach (OutputVariables output in balanceOutputVariables)
                {
                    balanceOutputVariables.Add(output);
                }
            }
        }
    }
}
