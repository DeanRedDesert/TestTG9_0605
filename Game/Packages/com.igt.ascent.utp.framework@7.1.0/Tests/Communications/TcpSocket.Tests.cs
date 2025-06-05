// -----------------------------------------------------------------------
// <copyright file = "TcpSocket.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.Net.Sockets;
    using NUnit.Framework;
    using Communications;
    using System.Net;

    [TestFixture]
    public class GivenTcpSocket
    {
        const string address = "127.0.0.1";
        private int port = 5770;

        private Socket hostSocket;
        private Socket clientSocket;

        [SetUp]
        public void Arrange()
        {
            hostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            var ipEndPointp = new IPEndPoint(IPAddress.Parse(address), port);
            hostSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            hostSocket.Bind(ipEndPointp);
            hostSocket.Listen(100);
            hostSocket.BeginAccept(OnClientConnect, hostSocket);

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(address, port);
        }

        [TearDown]
        public void TearDown()
        {
            port++;
        }

        [Test]
        public void ConstructorWithNullSocket()
        {
            Assert.Throws<ArgumentNullException>(() => new TcpSocket(null));
        }

        [Test]
        public void ConstructorWithValidData()
        {
            Assert.DoesNotThrow(() => new TcpSocket(hostSocket));
        }

        [Test]
        public void GetNetworkStream()
        {
            var tcpSocket = new TcpSocket(clientSocket);
            Assert.IsNotNull(tcpSocket.GetNetworkStream());
        }

        [Test]
        public void GetStream()
        {
            var tcpSocket = new TcpSocket(clientSocket);
            Assert.IsNotNull(tcpSocket.GetStream());
        }

        [Test]
        public void AfterClose()
        {
            //var ipEndPointp = new IPEndPoint(IPAddress.Parse(address), port);
            //hostSocket.Bind(ipEndPointp);
            //hostSocket.Listen(100);
            //hostSocket.BeginAccept(OnClientConnect, hostSocket);

            //var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var tcpSocket = new TcpSocket(clientSocket);
            //clientSocket.Connect(address, port);

            if(!clientSocket.Connected)
                Assert.Inconclusive("Expected the socket to be connected.");

            tcpSocket.Close();
            Assert.IsFalse(clientSocket.Connected);
        }

        private void OnClientConnect(IAsyncResult asyncResult)
        {

        }
    }
}
