// -----------------------------------------------------------------------
// <copyright file = "ModuleCommand.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using NUnit.Framework;
    using Framework;

    [TestFixture]
    public class GivenModuleCommand
    {
        #region Default Constructor

        [Test]
        public void WithDefaultConstructor()
        {
            Assert.DoesNotThrow(() => new ModuleCommand());
        }

        #endregion Default Constructor

        #region 4-arg Constructor

        [Test]
        public void ConstructWithFourValidArgs()
        {
            Assert.DoesNotThrow(() => new ModuleCommand("command", "returns", "desc", new[] {"param1"}));
        }

        [Test]
        public void ConstructWithFourArgsAndNullReturns()
        {
            Assert.Throws<ArgumentNullException>(() => new ModuleCommand("command", null, "desc", new[] {"param1"}));
        }

        [Test]
        public void ConstructWithFourArgsAndValidReturns()
        {
            Assert.DoesNotThrow(() => new ModuleCommand(null, "returns", null));
        }

        #endregion 4-arg Constructor
    }
}
