// -----------------------------------------------------------------------
// <copyright file = "StateObjectEventArgs.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.Net.Sockets;
    using NUnit.Framework;
    using Communications;

    [TestFixture]
    public class GivenStateObjectEventArgs
    {
        [Test]
        public void ConstructorWithNullSocket()
        {
            Assert.Throws<ArgumentNullException>(() => new StateObjectEventArgs(null, 1, "data"));
        }

        [Test]
        public void ConstructorWithValidData()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            var obj = new StateObjectEventArgs(socket, 1, "data");

            Assert.AreEqual(socket, obj.SocketX);
            Assert.AreEqual(1, obj.Size);
            Assert.AreEqual("data", obj.Data);
        }
    }
}
