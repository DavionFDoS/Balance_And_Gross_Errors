using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class BalanceSettings
    {
        public enum BalanceSettingsConstraints
        {
            TECHNOLOGIC,
            METROLOGIC
        }
        public BalanceSettingsConstraints balanceSettingsConstraints { get; set; }
    }
}
