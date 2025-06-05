// -----------------------------------------------------------------------
// <copyright file = "PadTouchCoordinates.Tests.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using NUnit.Framework;
    using Framework;

    [TestFixture]
    public class GivenPadTouchCoordinates
    {
        [Test]
        public void ConstructWithValidArgs()
        {
            Assert.DoesNotThrow(() => new PadTouchCoordinates(0, 0));
        }
    }
}
