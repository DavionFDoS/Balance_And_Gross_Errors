using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;
using Balance_and_Gross_errors.Solverdir;
namespace Balance_and_Gross_errors.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InputVariablesController : ControllerBase
    {
        private readonly InputVariablesContext _context;
        BalanceInput balanceInput = new BalanceInput();
        public InputVariablesController(InputVariablesContext context)
        {
            _context = context;
        }
        // POST: InputVariablesController/aa
        //[HttpPost]
        //public async Task<Responce> GetBalance(BalanceInput input)
        //{
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            // Сведение баланса
        //            Solver solver = new Solver(input);
        //            var output = solver.reconciledValuesArray;
        //            return new Responce
        //            {
        //                Type = "result",
        //                Data = output
        //            };
        //        }
        //        catch (Exception e)
        //        {
        //            return new Responce
        //            {
        //                Type = "error",
        //                Data = e.Message
        //            };
        //        }
        //    });
        //}
        // POST: InputVariablesController/Create
        [HttpPost]
        public async Task<Responce> GlobalTest (BalanceInput input)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Решение задачи
                    Solver solver = new Solver(input);
                    var output = solver.GTR;
                    //var output1 = solver.reconciledValuesArray;
                    return new Responce
                    {
                        Type = "result",
                        Data = output,
                        //Data1 = output1
                    };
                }
                catch (Exception e)
                {
                    return new Responce
                    {
                        Type = "error",
                        Data = e.Message,
                        //Data1 = e.Message
                    };
                }
            });
        }
        
       

    }
}
