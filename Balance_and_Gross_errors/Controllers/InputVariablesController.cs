using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;

namespace Balance_and_Gross_errors.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InputVariablesController : ControllerBase
    {
        private readonly InputVariablesContext _context;

        public InputVariablesController(InputVariablesContext context)
        {
            _context = context;
        }
        // GET:  InputVariablesController/Create
        [HttpGet("{id}")]
        public async Task<ActionResult<InputVariables>> GetId(string id)
        {
            var Item = await _context.InputVariablesList.FindAsync(id);

            if (Item == null)
            {
                return NotFound();
            }
            //check

            return Item;
        }
        // POST: InputVariablesController/Create
        [HttpPost]
        public async Task<ActionResult<InputVariables>> Post(InputVariables input)
        {
            if (input == null)
            {
                return BadRequest();
            }
            _context.InputVariablesList.Add(input);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetId), new { id = input.id }, input);
        }

       
    }
}
