// -----------------------------------------------------------------------
// <copyright file = "AutomationParameter.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class GivenAutomationParameter
    {
        #region Default Constructor

        [Test]
        public void WithDefaultConstructor()
        {
            Assert.DoesNotThrow(() => new AutomationParameter());
        }

        #endregion Default Constructor

        #region Param String Constructor

        [Test]
        public void ConstructWithParsableParamString()
        {
            const string apName = "Name";
            const string apType = "Type";
            const string apDesc = "Desc";
            var ap = new AutomationParameter(string.Format("{0}|{1}|{2}", apName, apType, apDesc));
            Assert.AreEqual(apName, ap.Name);
            Assert.AreEqual(apType, ap.Type);
            Assert.AreEqual(apDesc, ap.Description);
        }

        [Test]
        public void ConstructWithNonParsableParamString()
        {
            const string invalidParamString = "Value1|Value2|Value3|Value4";
            var ap = new AutomationParameter(invalidParamString);
            Assert.AreEqual(invalidParamString, ap.Description);
        }

        [Test]
        public void ConstructWithNullParamString()
        {
            //  No exceptions occur when paramString is null
            Assert.DoesNotThrow(() => new AutomationParameter(null));
        }

        #endregion Param String Constructor

        #region 4-arg Constructor

        [Test]
        public void ConstructWithValidArgs()
        {
            Assert.DoesNotThrow(() => new AutomationParameter("Name", "Value", "string", "Desc"));
        }

        [Test]
        public void ConstructWithFourArgsAndNullName()
        {
            Assert.DoesNotThrow(() => new AutomationParameter(null, "SomeValue"));
        }

        [Test]
        public void ConstructWithFourArgsAndNullValue()
        {
            Assert.DoesNotThrow(() => new AutomationParameter("Name", null));
        }

        #endregion 4-arg Constructor

        #region GetIncomingParameterValidationErrors

        [Test]
        public void GetParamErrorsWithValidArgs()
        {
            var param1 = new AutomationParameter("Name|Value|Desc");
            var errors = AutomationParameter.GetIncomingParameterValidationErrors(new List<AutomationParameter> {param1}, new List<string> {"Name"});
            Assert.IsTrue(string.IsNullOrEmpty(errors));
        }

        [Test]
        public void GetParamErrorsWithIncorrectParamName()
        {
            var param1 = new AutomationParameter("Name|Value|Desc");
            var errors = AutomationParameter.GetIncomingParameterValidationErrors(new List<AutomationParameter> {param1}, new List<string> {"WrongName"});
            Assert.IsFalse(string.IsNullOrEmpty(errors));
        }

        [Test]
        public void GetParamErrorsWithExtraParams()
        {
            var param1 = new AutomationParameter("Name1|Value1|Desc1");
            var param2 = new AutomationParameter("Name2|Value2|Desc2");
            var errors = AutomationParameter.GetIncomingParameterValidationErrors(new List<AutomationParameter> {param1, param2}, new List<string> {"Name1"});
            Assert.IsFalse(string.IsNullOrEmpty(errors));
        }

        [Test]
        public void GetParamErrorsWithExtraNames()
        {
            var param1 = new AutomationParameter("Name1|Value1|Desc1");
            var errors = AutomationParameter.GetIncomingParameterValidationErrors(new List<AutomationParameter> {param1}, new List<string> {"Name1", "Name2"});
            Assert.IsFalse(string.IsNullOrEmpty(errors));
        }

        [Test]
        public void GetParamErrorsWithDifferentOrders()
        {
            var param1 = new AutomationParameter("Name1|Value1|Desc1");
            var param2 = new AutomationParameter("Name2|Value2|Desc2");
            var errors = AutomationParameter.GetIncomingParameterValidationErrors(new List<AutomationParameter> {param1, param2}, new List<string> {"Name2", "Name1"});
            Assert.IsFalse(string.IsNullOrEmpty(errors));
        }

        [Test]
        public void GetParamErrorsWithNullArgs()
        {
            Assert.DoesNotThrow(() => AutomationParameter.GetIncomingParameterValidationErrors(null, null));
        }

        #endregion GetIncomingParameterValidationErrors

        #region SetParameter

        [Test]
        public void SetParameterWithValidArgs()
        {
            Assert.DoesNotThrow(() => AutomationParameter.SetParameter(new AutomationCommand(), "param", "value"));
        }

        [Test]
        public void SetParameterWithNullCommand()
        {
            Assert.Throws<ArgumentNullException>(() => AutomationParameter.SetParameter(null, "param", "value"));
        }

        [Test]
        public void SetParameterWithEmptyParamsList()
        {
            const string apName = "Name";
            const string apValue = "Value";
            const string apType = "String";
            const string apDesc = "Desc";
            var ac = new AutomationCommand();
            AutomationParameter.SetParameter(ac, apName, apValue, apType, apDesc);
            Assert.AreEqual(1, ac.Parameters.Count);
            Assert.AreEqual(apName, ac.Parameters[0].Name);
            Assert.AreEqual(apValue, ac.Parameters[0].Value);
            Assert.AreEqual(apType, ac.Parameters[0].Type);
            Assert.AreEqual(apDesc, ac.Parameters[0].Description);
        }

        [Test]
        public void SetParameterWithMatchingParamName()
        {
            const string apName = "Name";
            const string apValue = "Value";
            const string apType = "String";
            const string apDesc = "Desc";
            var ap = new AutomationParameter(apName, "BadValue", "BadType", "BadDesc");
            var ac = new AutomationCommand {Parameters = new List<AutomationParameter> {ap}};
            AutomationParameter.SetParameter(ac, apName, apValue, apType, apDesc);
            Assert.AreEqual(1, ac.Parameters.Count);
            Assert.AreEqual(apName, ac.Parameters[0].Name);
            Assert.AreEqual(apValue, ac.Parameters[0].Value);
            Assert.AreEqual(apType, ac.Parameters[0].Type);
            Assert.AreEqual(apDesc, ac.Parameters[0].Description);
        }

        [Test]
        public void SetParameterWithNonMatchingParamName()
        {
            const string apName1 = "Name1";
            const string apValue1 = "Value1";
            const string apType1 = "String";
            const string apDesc1 = "Desc1";
            const string apName2 = "Name2";
            const string apValue2 = "true";
            const string apType2 = "bool";
            const string apDesc2 = "Desc2";
            var ap = new AutomationParameter(apName2, apValue2, apType2, apDesc2);
            var ac = new AutomationCommand { Parameters = new List<AutomationParameter> { ap } };
            AutomationParameter.SetParameter(ac, apName1, apValue1, apType1, apDesc1);

            Assert.AreEqual(2, ac.Parameters.Count);
            var ap1 = ac.Parameters.FirstOrDefault(p => p.Name == apName1);
            var ap2 = ac.Parameters.FirstOrDefault(p => p.Name == apName2);

            Assert.IsNotNull(ap1);
            Assert.AreEqual(apName1, ap1.Name);
            Assert.AreEqual(apValue1, ap1.Value);
            Assert.AreEqual(apType1, ap1.Type);
            Assert.AreEqual(apDesc1, ap1.Description);

            Assert.IsNotNull(ap2);
            Assert.AreEqual(apName2, ap2.Name);
            Assert.AreEqual(apValue2, ap2.Value);
            Assert.AreEqual(apType2, ap2.Type);
            Assert.AreEqual(apDesc2, ap2.Description);
        }

        #endregion SetParameter

        #region GetParameterValues

        [Test]
        public void GetParameterValuesWithValidArgs()
        {
            Assert.DoesNotThrow(() => AutomationParameter.GetParameterValues(new AutomationCommand(), "Name"));
        }

        [Test]
        public void GetParameterValuesWithNullCommand()
        {
            Assert.Throws<ArgumentNullException>(() => AutomationParameter.GetParameterValues(null, "Name"));
        }

        [Test]
        public void GetParameterValuesWithIgnoredCaseAndParamsMatch()
        {
            const string apName = "Name";
            const string apValue = "Value";
            var ap = new AutomationParameter(apName, apValue);
            var ac = new AutomationCommand {Parameters = new List<AutomationParameter> {ap}};
            var values = AutomationParameter.GetParameterValues(ac, apName);

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(apValue, values[0]);
        }

        [Test]
        public void GetParameterValuesWithIgnoredCaseAndParamsMismatched()
        {
            const string apName = "Name";
            const string apValue = "Value";
            var ap = new AutomationParameter("name", apValue);
            var ac = new AutomationCommand { Parameters = new List<AutomationParameter> { ap } };
            var values = AutomationParameter.GetParameterValues(ac, apName);

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(apValue, values[0]);
        }

        [Test]
        public void GetParameterValuesWithoutIgnoreCaseAndParamsMatch()
        {
            const string apName = "Name";
            const string apValue = "Value";
            var ap = new AutomationParameter(apName, apValue);
            var ac = new AutomationCommand { Parameters = new List<AutomationParameter> { ap } };
            var values = AutomationParameter.GetParameterValues(ac, apName, false);

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(apValue, values[0]);
        }

        [Test]
        public void GetParameterValuesWithoutIgnoreCaseAndParamsMisatched()
        {
            const string apName = "Name";
            const string apValue = "Value";
            var ap = new AutomationParameter("name", apValue);
            var ac = new AutomationCommand { Parameters = new List<AutomationParameter> { ap } };
            var values = AutomationParameter.GetParameterValues(ac, apName, false);

            Assert.AreEqual(0, values.Count);
        }
        
        #endregion GetParameterValues

        #region GetParameterDictionary

        [Test]
        public void GetParameterDictionaryWithValidArgs()
        {
            Assert.DoesNotThrow(() => AutomationParameter.GetParameterDictionary(new AutomationCommand()));
        }

        [Test]
        public void GetParameterDictionaryWithNullCommand()
        {
            Assert.Throws<ArgumentNullException>(() => AutomationParameter.GetParameterDictionary(null));
        }

        [Test]
        public void GetParameterDictionaryWithSingleParam()
        {
            const string apName = "Name";
            const string apValue = "Value";
            var ap = new AutomationParameter {Name = apName, Value = apValue};
            var ac = new AutomationCommand {Parameters = new List<AutomationParameter> {ap}};
            var dictionary = AutomationParameter.GetParameterDictionary(ac);

            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(apName));
            Assert.IsTrue(dictionary[apName].Contains(apValue));
        }

        [Test]
        public void GetParameterDictionaryWithMultipleParamValues()
        {
            const string apName1 = "Name1";
            const string apValue1 = "Value1";
            const string apName2 = "Name2";
            const string apValue2 = "Value2";
            var ap1 = new AutomationParameter { Name = apName1, Value = apValue1 };
            var ap2 = new AutomationParameter { Name = apName2, Value = apValue2 };
            var ac = new AutomationCommand { Parameters = new List<AutomationParameter> { ap1, ap2 } };
            var dictionary = AutomationParameter.GetParameterDictionary(ac);

            Assert.AreEqual(2, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(apName1));
            Assert.IsTrue(dictionary.ContainsKey(apName2));
            Assert.IsTrue(dictionary[apName1].Contains(apValue1));
            Assert.IsTrue(dictionary[apName2].Contains(apValue2));
        }

        [Test]
        public void GetParameterDictionaryWithDuplicateParamValues()
        {
            const string apName = "Name";
            const string apValue1 = "Value1";
            const string apValue2 = "Value2";
            var ap1 = new AutomationParameter {Name = apName, Value = apValue1};
            var ap2 = new AutomationParameter {Name = apName, Value = apValue2};
            var ac = new AutomationCommand {Parameters = new List<AutomationParameter> {ap1, ap2}};
            var dictionary = AutomationParameter.GetParameterDictionary(ac);

            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(apName));
            Assert.IsTrue(dictionary[apName].Contains(apValue1));
            Assert.IsTrue(dictionary[apName].Contains(apValue2));
        }

        #endregion GetParameterDictionary
    }
}
