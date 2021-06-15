using Microsoft.VisualStudio.TestTools.UnitTesting;
using Balance_and_Gross_errors.Controllers;
using Balance_and_Gross_errors.Models;
using System;
using Newtonsoft.Json;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class Tests
    {        
        [TestMethod]
        public void TestGTPost()
        {
            var inputData = JsonConvert.DeserializeObject<BalanceInput>(File.ReadAllText(@"C:\Users\Matvey\source\repos\Gradwork\Balance_And_Gross_Errors\UnitTests\Input.json"));

            var expected = 0.1552143053428158;

            var controller = new InputVariablesController();
            var result = controller.GetBalanceGurobi(inputData).Result;
            Assert.AreEqual("Success", result.Status);
            Assert.AreEqual(expected, result.GlobaltestValue);
        }

        [TestMethod]
        public void TestBalancePost()
        {
            var inputData = JsonConvert.DeserializeObject<BalanceInput>(File.ReadAllText(@"C:\Users\Matvey\source\repos\Gradwork\Balance_And_Gross_Errors\UnitTests\Input.json"));

            double[] expected = new[] { 
                10.055612418500504,
                3.0144745895183522,
                7.041137828982151,
                1.9822547563048074,
                5.058883072677343,
                4.067257698582969,
                0.9916253740943739
            };

            var controller = new InputVariablesController();
            var result = controller.GetBalanceGurobi(inputData).Result;
            Assert.AreEqual("Success", result.Status);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result.BalanceOutputVariables[i].value, 0.001);
            }
        }
        
        [TestMethod]
        public void TestModelCorrectnessMetro()
        {
            var inputData = JsonConvert.DeserializeObject<BalanceInput>(File.ReadAllText(@"C:\Users\Matvey\source\repos\Gradwork\Balance_And_Gross_Errors\UnitTests\MetrologicBoundsError.json"));

            var controller = new InputVariablesController();
            var result = controller.GetBalanceGurobi(inputData).Result;
            Assert.AreEqual("MetrologicBounds are incorrect", result.Status);
        }
        
        [TestMethod]
        public void TestModelCorrectnessTechno()
        {
            var inputData = JsonConvert.DeserializeObject<BalanceInput>(File.ReadAllText(@"C:\Users\Matvey\source\repos\Gradwork\Balance_And_Gross_Errors\UnitTests\TechnologicBoundsError.json"));

            var controller = new InputVariablesController();
            var result = controller.GetBalanceGurobi(inputData).Result;
            Assert.AreEqual("TechnologicBounds are incorrect", result.Status);
        }
    }
}
