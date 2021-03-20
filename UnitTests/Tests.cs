using Microsoft.VisualStudio.TestTools.UnitTesting;
using Balance_and_Gross_errors.Controllers;
using Balance_and_Gross_errors.Models;
namespace UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestApiPostIncorrectComplicated()
        {
            InputVariables input = new InputVariables();
            input.id = "00000000 - 0000 - 0000 - 0000 - 000000000001";
            input.sourceId = "null";
            input.destinationId = "00000000-0000-0000-0000-000000000001";
            input.name = "X1";
            input.measured = 10.005;
            input.metrologicUpperBound = 1000;
            input.metrologicLowerBound = 0;
            input.technologicUpperBound = 1000;
            input.technologicLowerBound = 0;
            input.tolerance = 0.2;
            input.isMeasured = true;
            input.isExcluded = false;
            input.useTechnologic = true;

            InputVariables input1 = new InputVariables();
            input.id = "00000000 - 0000 - 0000 - 0000 - 000000000002";
            input.sourceId = "00000000-0000-0000-0000-000000000001";
            input.destinationId = "null";
            input.name = "X2";
            input.measured = 3.033;
            input.metrologicUpperBound = 1000;
            input.metrologicLowerBound = 0;
            input.technologicUpperBound = 1000;
            input.technologicLowerBound = 0;
            input.tolerance = 0.121;
            input.isMeasured = true;
            input.isExcluded = false;
            input.useTechnologic = true;
            var data = new BalanceInput();
            data.BalanceInputVariables.Add(input);
            data.BalanceInputVariables.Add(input1);
            var controller = new InputVariablesController();
            var result = controller.GlobalTest(data).Result;

            Assert.AreEqual("error", result.Type);
        }
        [TestMethod]
        public void TestGTPostString()
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
      ""metrologicLowerBound"": 0,
      ""technologicUpperBound"": 1000,
      ""technologicLowerBound"": 0,
      ""tolerance"": 0.02,
      ""isMeasured"": true,
      ""isExcluded"": false,
      ""useTechnologic"": true
    }
  ]
}";

            var expected = 0.1552143053428158;

            var controller = new InputVariablesController();
            var result = controller.GlobalTestString(jsonString).Result;
            var resultData = result.Data as BalanceOutput;
            Assert.AreEqual("result", result.Type);
            Assert.AreEqual(expected, result.Data);
        }
    }
}
