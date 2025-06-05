// -----------------------------------------------------------------------
// <copyright file = "ModuleEvent.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using NUnit.Framework;
    using Framework;

    [TestFixture]
    public class GivenModuleEvent
    {
        #region Default Constructor

        [Test]
        public void WithDefaultConstructor()
        {
            Assert.DoesNotThrow(() => new ModuleEvent());
        }

        #endregion Default Constructor

        #region 3-arg Constructor

        [Test]
        public void ConstructWithThreeValidArgs()
        {
            Assert.DoesNotThrow(() => new ModuleEvent("Name", "EventArgs", "Desc"));
        }

        [Test]
        public void ConstructWithThreeArgsAndNullEventArgs()
        {
            Assert.Throws<ArgumentNullException>(() => new ModuleEvent("Name", null, "Desc"));
        }

        [Test]
        public void ConstructWithThreeArgsAndValidEventArgs()
        {
            var me = new ModuleEvent(null, "EventArgs", null);
        }

        #endregion 3-arg Constructor
    }
}
