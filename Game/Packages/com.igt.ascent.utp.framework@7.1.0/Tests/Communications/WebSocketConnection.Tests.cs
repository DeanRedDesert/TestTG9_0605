// -----------------------------------------------------------------------
// <copyright file = "WebSocketConnection.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.Net.Sockets;
    using NUnit.Framework;
    using IGT.Game.Utp.Framework.Communications;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.IO;
    using System.Net;
    using System.Threading;

    [TestFixture]
    public class GivenWebSocketConnection
    {
        private WebSocketLib webSocketLib;
        private Socket hostSocket;
        private WebSocketProtocol webSocketProtocol;

        [SetUp]
        public void Arrange()
        {
            webSocketLib = new WebSocketLib();
            hostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            webSocketProtocol = new WebSocketProtocol(false);
        }

        [Test]
        public void ConstructWithNullLib()
        {
            Assert.Throws<ArgumentNullException>(() => new WebSocketConnection(null, hostSocket, webSocketProtocol));
            Assert.Throws<ArgumentNullException>(() => new WebSocketConnection(null, hostSocket, webSocketProtocol, 5));
        }

        [Test]
        public void ConstructWithNullSocket()
        {
            Assert.Throws<ArgumentNullException>(() => new WebSocketConnection(webSocketLib, null, webSocketProtocol));
            Assert.Throws<ArgumentNullException>(() => new WebSocketConnection(webSocketLib, null, webSocketProtocol, 5));
        }

        [Test]
        public void ConstructWithNullProtocol()
        {
            Assert.Throws<ArgumentNullException>(() => new WebSocketConnection(webSocketLib, hostSocket, null));
            Assert.Throws<ArgumentNullException>(() => new WebSocketConnection(webSocketLib, hostSocket, null, 5));
        }

        [Test]
        public void ConstructWithValidArgs()
        {
            var connection1 = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.AreEqual(hostSocket, connection1.ConnectionSocket);

            var connection2 = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol, 5);
            Assert.AreEqual(hostSocket, connection2.ConnectionSocket);
        }

        [Test]
        public void ReceiveWithValidArgs()
        {
            var ac = new AutomationCommand("MyModule", "MyCommand", null);
            var bytes = ObjectToByteArray(ac);

            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.DoesNotThrow(() => connection.Receive(3, bytes));
        }

        [Test]
        public void SendWithNullCommand()
        {
            AutomationCommand ac = null;
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.Throws<ArgumentNullException>(() => connection.Send(ac));
        }

        [Test]
        public void SendWithValidCommand()
        {
            var ac = new AutomationCommand("", "", null);
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.IsTrue(connection.Send(ac));
        }

        [Test]
        public void SendWithNullPayload()
        {
            string payload = null;
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.Throws<ArgumentNullException>(() => connection.Send(payload));
        }

        [Test]
        public void SendWithValidPayload()
        {
            string payload = "some payload";
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.DoesNotThrow(() => connection.Send(payload));
        }

        [Test]
        public void SendWithNullBytes()
        {
            byte[] bytes = null;
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.Throws<ArgumentNullException>(() => connection.Send(bytes));
        }

        [Test]
        public void SendWithValidBytes()
        {
            byte[] bytes = new byte[] { 0x20 };
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.DoesNotThrow(() => connection.Send(bytes));
        }

        [Test]
        public void OnSendWithNullSerializedCommand()
        {
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            Assert.Throws<ArgumentNullException>(() => connection.OnSend(null));
        }

        [Test]
        public void OnSendWithValidSerializedCommand()
        {
            var connection = new WebSocketConnection(webSocketLib, hostSocket, webSocketProtocol);
            connection.OnSend("command");
        }

        private byte[] ObjectToByteArray(object obj)
        {
            if(obj == null)
                return null;

            var bf = new BinaryFormatter();
            using(var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }

    [TestFixture]
    public class GivenConnectedWebSocketConnection
    {
        const string address = "127.0.0.1";
        private int port = 5780;

        private WebSocketLib webSocketLib;
        private WebSocketProtocol webSocketProtocol;

        private Socket hostSocket;
        private Socket clientSocket;

        [SetUp]
        public void Arrange()
        {
            webSocketLib = new WebSocketLib();
            webSocketProtocol = new WebSocketProtocol(false);

            var ipEndPointp = new IPEndPoint(IPAddress.Parse(address), port);
            hostSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            hostSocket.Bind(ipEndPointp);
            hostSocket.Listen(100);
            hostSocket.BeginAccept(OnClientConnect, hostSocket);

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(address, port);

            if(!clientSocket.Connected)
                Assert.Inconclusive("Expected the client socket to be connected.");
        }

        [TearDown]
        public void TearDown()
        {
            port++;
        }

        [Test]
        public void AfterDisconnect()
        {
            var eventHandled = false;
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            WebSocketConnection.DisconnectedDelegate eventHandler = delegate(WebSocketConnection sender, EventArgs args)
            {
                eventHandled = true;
                waitHandle.Set();
            };

            var connection = new WebSocketConnection(webSocketLib, clientSocket, webSocketProtocol);
            connection.Disconnected += eventHandler;
            connection.Disconnect();
            waitHandle.WaitOne(new TimeSpan(0, 0, 0, 2));
            Assert.IsTrue(eventHandled);
            Assert.IsFalse(clientSocket.Connected);
        }

        [Test]
        public void AfterClose()
        {
            var connection = new WebSocketConnection(webSocketLib, clientSocket, webSocketProtocol);
            connection.Close();
            Assert.IsFalse(clientSocket.Connected);
        }

        private void OnClientConnect(IAsyncResult asyncResult)
        {

        }
    }
}
