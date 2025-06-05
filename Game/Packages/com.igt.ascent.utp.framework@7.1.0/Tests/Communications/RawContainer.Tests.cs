// -----------------------------------------------------------------------
// <copyright file = "RawContainer.Tests.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System.IO;
    using NUnit.Framework;
    using Communications;

    [TestFixture]
    public class GivenRawContainer
    {
        [Test]
        public void WithConstructor()
        {
            var bytes = new byte[] { 0x20 };
            Assert.DoesNotThrow(() => new RawContainer(bytes));

            var rc1 = new RawContainer(bytes);
            Assert.AreEqual(bytes, rc1.Data);

            var rc2 = new RawContainer(null);
            Assert.IsNull(rc2.Data);
        }
    }
}
