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
                    solver.Balance();
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
       
            [HttpPost("Gt")]
            public async Task<GlrRes> Gtest(BalanceInput input)
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        // Решение задачи
                        Solver solver = new Solver(input);
                        var ab = solver.GTR;
                        return new GlrRes
                        {
                            Data = ab
                        };

                    }
                    catch (Exception e)
                    {
                        return new GlrRes
                        {
                            Data = e.Message,
                        };
                    }
                });
            }

            [HttpPost("GLR")]
        public async Task<GlrRes> GlrTest(BalanceInput input)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Решение задачи
                    Solver solver = new Solver(input);
                    var (root, flows) = solver.GlrPrep();
                    var nodes = root.Where(x => x.IsLeaf);
                    var results = new List<Glr>();

                    foreach (var node in nodes)
                    {
                        var flowerrors = new List<Flow>();
                        var flowCorrections = new List<InputVariables>();
                        foreach (var flow in node.Item.Flows)
                        {
                            var (i, j,k) = flow;

                            var newFlow = new Flow($"Соединяет узлы: {input.BalanceInputVariables[i].name} -> {input.BalanceInputVariables[j].name}");

                            var existingFlowIdx = flows.FindIndex(x => x.Input == i && x.Output == j);
                            if (existingFlowIdx != -1)
                            {
                                var (_, _, existingFlow) = flows[existingFlowIdx];

                                newFlow.Id =  input.BalanceInputVariables[existingFlow].id;
                                newFlow.Name = "Поток " + input.BalanceInputVariables[existingFlow].name;

                                var variable = new InputVariables
                                {
                                    id = Guid.NewGuid().ToString(),
                                    sourceId = input.BalanceInputVariables[i].id,
                                    destinationId = input.BalanceInputVariables[j].id,
                                    name = input.BalanceInputVariables[existingFlow].name + " (Доп. поток)",
                                    measured = input.BalanceInputVariables[existingFlow].measured,
                                    correction = (input.BalanceInputVariables[existingFlow].measured + solver.corr[existingFlow])/2,
                                    metrologicLowerBound = input.BalanceInputVariables[existingFlow].metrologicLowerBound,
                                    metrologicUpperBound = input.BalanceInputVariables[existingFlow].metrologicUpperBound,
                                    technologicLowerBound= input.BalanceInputVariables[existingFlow].technologicLowerBound,
                                    technologicUpperBound = input.BalanceInputVariables[existingFlow].technologicUpperBound,
                                    tolerance = input.BalanceInputVariables[existingFlow].tolerance,
                                    isMeasured = true,
                                };

                                flowCorrections.Add(variable);
                            }

                            flowerrors.Add(newFlow);
                        }
                        results.Add(new Glr
                        {
                            FlowErrors = flowerrors,
                            FlowCorrections = flowCorrections,
                            GlobalTestValue = node.Item.GlobalTestValue
                        });
                    }

                    return new GlrRes
                    {
                        Data = results
                    };

                }
                catch (Exception e)
                {
                    return new GlrRes
                    {
                        Data = e.Message,
                    };
                }
            });
        }
    }
}
