// -----------------------------------------------------------------------
// <copyright file = "WebSocketBuffer.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using NUnit.Framework;
    using Communications;
    using System.Net.Sockets;

    [TestFixture]
    public class GivenWebSocketBuffer
    {
        private WebSocketLib webSocketLib;
        private Socket hostSocket;
        private WebSocketProtocol webSocketProtocol;
        private WebSocketConnection connection;

        private const string data = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                            "<AutomationCommand xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                            "<Module>Game State</Module><Command>GetCurrentState</Command><Parameters><AutomationParameter><Name>CurrentState</Name>" +
                            "<Value>BaseGameIdleState</Value><Description>The current state of the game.</Description><Type>string</Type>" +
                            "</AutomationParameter></Parameters></AutomationCommand>";

        [SetUp]
        public void Arrange()
        {
            webSocketLib = new WebSocketLib();
            hostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            webSocketProtocol = new WebSocketProtocol(false);
            connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
        }

        [Test]
        public void ConstructWithNullLib()
        {
            Assert.Throws<ArgumentNullException>(() => new WebSocketBuffer(null, connection));
        }

        [Test]
        public void ConstructWithNullConnection()
        {
            Assert.Throws<ArgumentNullException>(() => new WebSocketBuffer(webSocketLib, null));
        }

        [Test]
        public void ConstructWithValidArgs()
        {
            Assert.DoesNotThrow(() => new WebSocketBuffer(webSocketLib, connection));
        }

        [Test]
        public void InitializeWithNullProtocol()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            Assert.Throws<ArgumentNullException>(() => buffer.Initialize(null));
        }

        [Test]
        public void InitializeWithValidProtocol()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            Assert.DoesNotThrow(() => buffer.Initialize(webSocketProtocol));
        }

        [Test]
        public void SendWithNullData()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            Assert.Throws<ArgumentNullException>(() => buffer.Send(null));
        }

        [Test]
        public void SendWithValidData()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            buffer.Initialize(webSocketProtocol);
            buffer.Send(data);
        }

        [Test]
        public void AfterOnSend()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);

            if(buffer.Busy)
                Assert.Inconclusive("Expected buffer not to be busy immediately after construction.");

            buffer.Initialize(webSocketProtocol);

            buffer.OnSend();
            Assert.IsFalse(buffer.Busy, "Buffer should not be busy after OnSend when the queue is empty.");

            buffer.Busy = true;
            buffer.Send(data);
            buffer.Busy = false;
            buffer.OnSend();
            Assert.IsTrue(buffer.Busy, "Buffer should be busy after OnSend when the queue has items.");
            buffer.OnSend();
            Assert.IsFalse(buffer.Busy, "Buffer should not be busy after the second call to OnSend.");
        }

        [Test]
        public void ReceiveRawWithNullData()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            Assert.Throws<ArgumentNullException>(() => buffer.ReceiveRaw(null, new ParseData()));
        }

        [Test]
        public void ReceiveRawWithNullParseData()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            Assert.Throws<ArgumentNullException>(() => buffer.ReceiveRaw(new byte[] { 0x20 }, null));
        }

        [Test]
        public void ReceiveRawWithValidArgs()
        {
            var buffer = new WebSocketBuffer(webSocketLib, connection);
            Assert.DoesNotThrow(() => buffer.ReceiveRaw(new byte[] { 0x20 }, new ParseData()));
        }
    }
}
