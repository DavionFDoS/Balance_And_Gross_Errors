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

        // POST: InputVariablesController/aa
        //[HttpPost]
        //public BalanceOutput GetBalance(BalanceInput input)
        //{
        //    BalanceOutput balanceOutput = new BalanceOutput();
        //    balanceOutput.BalanceOutputVariables.Add(new OutputVariables());
        //    return balanceOutput;
        //}


        // POST: InputVariablesController/Create
        [HttpPost]
        public async Task<Responce> GlobalTest(BalanceInput input)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Решение задачи
                    Solver solver = new Solver(input);
                    var output = solver.GTR;
                    var output1 = solver.sol;
                    return new Responce
                    {
                        Type = "result",
                        Data = output,
                        Data1 = output1
                    };
                }
                catch (Exception e)
                {
                    return new Responce
                    {
                        Type = "error",
                        Data = e.Message,
                        Data1 = e.Message
                    };
                }
            });
        }

        [HttpPost]
        public async Task<Responce> GlobalTestString([FromForm] string input)
        {
            try
            {
                // Проверка аргумента на null
                _ = input ?? throw new ArgumentNullException(nameof(input));

                // Решение задачи
                var inputData = JsonConvert.DeserializeObject<BalanceInput>(input);
                return await GlobalTest(inputData);
            }
            catch (Exception e)
            {
                return new Responce
                {
                    Type = "error",
                    Data = e.Message
                };
            }
        }



    }
}
