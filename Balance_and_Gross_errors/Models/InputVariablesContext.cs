using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Models
{
    public class InputVariablesContext : DbContext
    {
        public InputVariablesContext(DbContextOptions<InputVariablesContext> options)
            : base(options)
        {
           // Database.EnsureCreated();
        }
        public DbSet<InputVariables> InputVariablesList { get; set; }
    }
}
