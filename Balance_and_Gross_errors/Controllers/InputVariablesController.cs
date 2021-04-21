using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;
using Balance_and_Gross_errors.Solverdir;
using Newtonsoft.Json;

namespace Balance_and_Gross_errors.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InputVariablesController : ControllerBase
    {
        [HttpPost]
        public async Task<BalanceOutput> GetBalance(BalanceInput input)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Решение задачи
                    Solver solver = new Solver(input);
                    return solver.balanceOutput;

                }
                catch (Exception e)
                {
                    return new BalanceOutput
                    {
                        Status = e.Message,
                    };
                }
            });
        }
    }
}
