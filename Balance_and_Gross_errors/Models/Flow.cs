using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class Flow
    {
        
        public Flow(string info)
        {
            Id = Guid.NewGuid().ToString();
            Name = string.Empty;
            Info = info;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }

    }
}
