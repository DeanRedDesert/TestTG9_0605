// -----------------------------------------------------------------------
// <copyright file = "AutomationCommander.Tests.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using Communications;
    using NUnit.Framework;

    [TestFixture]
    public class GivenAutomationCommander
    {
        [Test]
        public void WithConstructor()
        {
            Assert.DoesNotThrow(() => new AutomationCommander(new WebSocketLib(), new AutomationCommand()));
        }
    }
}
