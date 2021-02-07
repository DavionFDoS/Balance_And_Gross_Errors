using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;

namespace Balance_and_Gross_errors.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BalanceController : ControllerBase
    {

        private readonly ILogger<BalanceController> _logger;

        public BalanceController(ILogger<BalanceController> logger)
        {
            _logger = logger;
        }

        //[HttpPost]
        //public InputVariables Post()
        //{
        //    InputVariables inputVariables = new InputVariables();
        //    inputVariables.setValues();
        //    return inputVariables;
        //}


    }
}
