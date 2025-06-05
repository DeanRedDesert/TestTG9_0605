// -----------------------------------------------------------------------
// <copyright file = "AutomationCommand.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using NUnit.Framework;
    using System.Collections.Generic;
    using Framework;

    [TestFixture]
    public class GivenAutomationCommand
    {
        #region Constructors

        [Test]
        public void WithDefaultConstructor()
        {
            Assert.DoesNotThrow(() => new AutomationCommand());
        }

        [Test]
        public void WithThreeArgConstructor()
        {
            Assert.DoesNotThrow(() => new AutomationCommand("module", "command", new List<AutomationParameter>()));
        }

        [Test]
        public void WithSixArgConstructor()
        {
            Assert.DoesNotThrow(() => new AutomationCommand("module", "command", "desc", "type", new List<AutomationParameter>()));
        }

        #endregion Constructors

        #region Serialize / Deserialize

        [Test]
        public void WhenSerializeAndDeserializeValidObject()
        {
            var ac = new AutomationCommand("module", "command", "desc", "type", new List<AutomationParameter> { new AutomationParameter() }, true);

            var serialized = AutomationCommand.Serialize(ac);
            var deserialized = AutomationCommand.Deserialize(serialized);

            Assert.AreEqual("module", deserialized.Module);
            Assert.AreEqual("command", deserialized.Command);
            Assert.AreEqual("desc", deserialized.Description);
            Assert.AreEqual("type", deserialized.Type);
            Assert.AreEqual(1, deserialized.Parameters.Count);
            Assert.IsTrue(deserialized.IsEvent);
        }

        [Test]
        public void WhenSerializeNullCommand()
        {
            Assert.Throws<ArgumentNullException>(() => AutomationCommand.Serialize(null));
        }

        [Test]
        public void WhenDeserializeNullString()
        {
            Assert.Throws<ArgumentNullException>(() => AutomationCommand.Deserialize(null));
        }

        #endregion Serialize / Deserialize
    }
}
