using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;

namespace Balance_and_Gross_errors.Models
{
    public class BalanceInput
    {
        private List<InputVariables> balanceInputVariables;

        public BalanceInput()
        {
            this.balanceInputVariables = new List<InputVariables>();
        }
        public List<InputVariables> BalanceInputVariables
        {
            get
            {
                return balanceInputVariables;
            }

            set 
            {
                foreach (InputVariables input in balanceInputVariables)
                {
                    balanceInputVariables.Add(input);
                }
            }
        }
    }
}
