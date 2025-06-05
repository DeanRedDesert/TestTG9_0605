// -----------------------------------------------------------------------
// <copyright file = "UtpTypeSerializer.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using NUnit.Framework;
    using Framework;

    [TestFixture]
    public class GivenUtpTypeSerializer
    {
        [Test]
        public void GetTypeDefinitionWithInvalidReturnDef()
        {
            Assert.Throws<ArgumentNullException>(() => UtpTypeSerializer.GetTypeDefinition(null));
        }

        [Test]
        public void GetTypeDefinitionWithSingleReturnDef()
        {
            const string initialValue = "string NewState";
            var result = UtpTypeSerializer.GetTypeDefinition(initialValue);
            Assert.AreEqual(initialValue, result);
        }

        [Test]
        public void GetTypeDefinitionWithMultipleReturnDefs()
        {
            const string initialValue = "string NewState,bool SomethingElse";
            const string expectedResult = "string NewState, bool SomethingElse";
            var result = UtpTypeSerializer.GetTypeDefinition(initialValue);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetTypeDefinitionWithCustomReturnDef()
        {
            const string initialValue = "UtpModules SomeMockObj";
            const string expectedResult = "Object(UtpModules){String PhysicalButtons, String DppButtons} SomeMockObj";
            var result = UtpTypeSerializer.GetTypeDefinition(initialValue);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void IsSimpleObjectWithSimpleObjects([Values(typeof(int), typeof(string), typeof(Enum))] Type type)
        {
            Assert.IsTrue(UtpTypeSerializer.IsSimpleObject(type));
        }

        [Test]
        public void IsSimpleObjectWithComplexObjects([Values(typeof(PadTouchCoordinates), typeof(ModuleCommand))] Type type)
        {
            Assert.IsFalse(UtpTypeSerializer.IsSimpleObject(type));
        }

        [Test]
        public void SerializeAndDeserialize()
        {
            var mockObj = new MockClass {SomeString = "StringValue"};
            var returnObj = UtpTypeSerializer.DeserializeObject<MockClass>(UtpTypeSerializer.SerializeObject(mockObj)) as MockClass;
            Assert.IsNotNull(returnObj);
            Assert.AreEqual(mockObj.SomeString, returnObj.SomeString);
        }
    }

    public class MockClass
    {
        public string SomeString { get; set; }
    }
}
