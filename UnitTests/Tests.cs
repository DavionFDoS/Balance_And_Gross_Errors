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
        [ExpectedException(typeof(ArgumentException))]
        public void TestApiPostIncorrect()
        {
            const string jsonString = @"{
  ""balanceInputVariables"": [
    {
                ""id"": ""00000000-0000-0000-0000-000000000001"",
      ""sourceId"": ""null"",
      ""destinationId"": ""00000000-0000-0000-0000-000000000001"",
      ""name"": ""X1"",
      ""measured"": 10.005,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.2,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    },
    {
                ""id"": ""00000000-0000-0000-0000-000000000002"",
      ""sourceId"": ""00000000-0000-0000-0000-000000000001"",
      ""destinationId"": ""null"",
      ""name"": ""X2"",
      ""measured"": 3.033,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.121,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    },
    {
                ""id"": ""00000000-0000-0000-0000-000000000003"",
      ""sourceId"": ""00000000-0000-0000-0000-000000000001"",
      ""destinationId"": ""00000000-0000-0000-0000-000000000002"",
      ""name"": ""X3"",
      ""measured"": 6.831,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.683,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    },
    {
                ""id"": ""00000000-0000-0000-0000-000000000004"",
      ""sourceId"": ""00000000-0000-0000-0000-000000000002"",
      ""destinationId"": ""null"",
      ""name"": ""X4"",
      ""measured"": 1.985,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.04,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    },
    {
                ""id"": ""00000000-0000-0000-0000-000000000005"",
      ""sourceId"": ""00000000-0000-0000-0000-000000000002"",
      ""destinationId"": ""00000000-0000-0000-0000-000000000003"",
      ""name"": ""X5"",
      ""measured"": 5.093,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.102,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    },
    {
                ""id"": ""00000000-0000-0000-0000-000000000006"",
      ""sourceId"": ""00000000-0000-0000-0000-000000000003"",
      ""destinationId"": ""null"",
      ""name"": ""X6"",
      ""measured"": 4.057,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.081,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    },
    {
                ""id"": ""00000000-0000-0000-0000-000000000007"",
      ""sourceId"": ""00000000-0000-0000-0000-000000000003"",
      ""destinationId"": ""null"",
      ""name"": ""X7"",
      ""measured"": 0.991,
      ""metrologicUpperBound"": 1000,
      ""metrologicLowerBound"": -10,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.02,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    }
  ]
}";

            var controller = new InputVariablesController();
            //var result = controller.GlobalTestString(jsonString).Result;
            // Assert.AreEqual("error", result.Type);
        }
        [TestMethod]
        public void TestGTPost()
        {
            var inputData = JsonConvert.DeserializeObject<BalanceInput>(File.ReadAllText(@"F:\Balance2\Balance_And_Gross_Errors\UnitTests\Input.json"));

            var expected = 0.1552143053428158;

            var controller = new InputVariablesController();
            var result = controller.GlobalTest(inputData).Result;
            Assert.AreEqual("result", result.Type);
            Assert.AreEqual(expected, result.Data);
        }

        [TestMethod]
        public void TestBalancePost()
        {
            var inputData = JsonConvert.DeserializeObject<BalanceInput>(File.ReadAllText(@"F:\Balance2\Balance_And_Gross_Errors\UnitTests\Input.json"));

            double[] expected = new[] { 10.055612418500504,
    3.0144745895183522,
    7.041137828982151,
    1.9822547563048074,
    5.058883072677343,
    4.067257698582969,
    0.9916253740943739};

            var controller = new InputVariablesController();
            var result = controller.GlobalTest(inputData).Result;
            Assert.AreEqual("result", result.Type);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], Array.ConvertAll<object, double>(result.Data1, a => (double)a)[i], 0.001);
            }
        }
    }
}
