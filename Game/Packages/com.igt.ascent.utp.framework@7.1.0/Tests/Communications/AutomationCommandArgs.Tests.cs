// -----------------------------------------------------------------------
// <copyright file = "AutomationCommandArgs.Tests.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System.IO;
    using NUnit.Framework;
    using Communications;

    [TestFixture]
    public class GivenAutomationCommandArgs
    {
        [OneTimeSetUp]
        public void ArrangeOnce()
        {
        #if PACKAGE
            Directory.SetCurrentDirectory(@"..\..\..\Packages\com.igt.ascent.unity-test-portal\Tests\");
        #endif
        }
        
        [Test]
        public void ConstructorWithValidAutomationCommand()
        {
            const string module = "Game State";
            const string command = "GetCurrentState";
            const string paramName = "CurrentState";
            const string paramValue = "IdleState";
            var testAcFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"Communications\TestAutomationCommand.xml");
            var xmlContents = File.ReadAllText(testAcFilePath);
            var acArgs = new AutomationCommandArgs(xmlContents);
            Assert.AreEqual(module, acArgs.Command.Module);
            Assert.AreEqual(command, acArgs.Command.Command);
            Assert.AreEqual(1, acArgs.Command.Parameters.Count);
            Assert.AreEqual(paramName, acArgs.Command.Parameters[0].Name);
            Assert.AreEqual(paramValue, acArgs.Command.Parameters[0].Value);
        }

        [Test]
        public void ConstructorWithInvalidAutomationCommand()
        {
            var acArgs = new AutomationCommandArgs("gibberish");
            Assert.IsNull(acArgs.Command);
        }
    }
}
