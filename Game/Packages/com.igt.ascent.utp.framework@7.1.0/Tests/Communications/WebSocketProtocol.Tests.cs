// -----------------------------------------------------------------------
// <copyright file = "WebSocketProtocol.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using Communications;
    using NUnit.Framework;

    [TestFixture]
    public class GivenWebSocketProtocol
    {
        private Socket clientSocket;
        private WebSocketProtocol webSocketProtocol;
        private WebSocketConnection webSocketConnection;
        private WebSocketBuffer webSocketBuffer;

        private const string socketHeader = "GET /ws HTTP/1.1" + "\r\n" +
                                            "Host: localhost:5780" + "\r\n" +
                                            "Pragma: no-cache" + "\r\n" +
                                            "Cache-Control: no-cache" + "\r\n" +
                                            "Origin: http://acloud.insideigt.com" + "\r\n" +
                                            "Sec-WebSocket-Version: 13" + "\r\n" +
                                            "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36" + "\r\n" +
                                            "Accept-Encoding: gzip, deflate, sdch" + "\r\n" +
                                            "Accept-Language: en-US,en;q=0.8" + "\r\n" +
                                            "Sec-WebSocket-Key: hPnNvpRrMh3tAtb69BrRtA==" + "\r\n" +
                                            "Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits" + "\r\n";

        [SetUp]
        public void Arrange()
        {
            var webSocketLib = new WebSocketLib();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            webSocketProtocol = new WebSocketProtocol(false);
            webSocketConnection = new WebSocketConnection(webSocketLib, clientSocket, webSocketProtocol);
            webSocketBuffer = new WebSocketBuffer(webSocketLib, webSocketConnection);
        }

        [Test]
        public void ConstructWithValidArgs([Values(true, false)] bool mask)
        {
            Assert.DoesNotThrow(() => new WebSocketProtocol(mask));
        }

        [Test]
        public void InitializeWithValidArgs()
        {
            Assert.DoesNotThrow(() => webSocketProtocol.Initialize(webSocketConnection, webSocketBuffer));
        }

        [Test]
        public void InitializeWithNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => webSocketProtocol.Initialize(webSocketConnection, null));
        }

        [Test]
        public void ServerHandshakeWithNullConnection()
        {
            Assert.Throws<ArgumentNullException>(() => WebSocketProtocol.PerformServerHandshake(null));
        }

        [Test]
        public void ServerHandshakeWithValidHeader()
        {
            const string header = socketHeader +
                                  "Upgrade: websocket" + "\r\n" +
                                  "Connection: Upgrade" + "\r\n";
            Assert.IsTrue(WebSocketProtocol.PerformServerHandshake(new SocketMock(header)));
        }

        [Test]
        public void ServerHandshakeWithoutUpgrade()
        {
            const string header = socketHeader +
                                  "Connection: Upgrade" + "\r\n";
            Assert.IsFalse(WebSocketProtocol.PerformServerHandshake(new SocketMock(header)));
        }

        [Test]
        public void ServerHandshakeWithoutConnection()
        {
            const string header = socketHeader +
                                  "Upgrade: websocket" + "\r\n";
            Assert.IsFalse(WebSocketProtocol.PerformServerHandshake(new SocketMock(header)));
        }

        [Test]
        public void ServerHandshakeWithConnectionNotUpgrade()
        {
            const string header = socketHeader +
                                  "Upgrade: websocket" + "\r\n" +
                                  "Connection: SomethingElse" + "\r\n";
            Assert.IsFalse(WebSocketProtocol.PerformServerHandshake(new SocketMock(header)));
        }

        [Test]
        public void ServerHandshakeWithUpgradeNotWebsocket()
        {
            const string header = socketHeader +
                                  "Upgrade: somethingelse" + "\r\n" +
                                  "Connection: Upgrade" + "\r\n";
            Assert.IsFalse(WebSocketProtocol.PerformServerHandshake(new SocketMock(header)));
        }

        [Test]
        public void WithReceivingValidData()
        {
            var bytes = GetBytes("<AutomationCommand><Module>UtpController</Module><Command>GetHelp</Command></AutomationCommand>");
            var pd = new ParseData
            {
                Fin = true,
                Mask = true,
                Opcode = 1,
                PayloadOffset = 6,
                MaskOffset = 2,
                PayloadLength = 95,
                BufferSize = 0
            };
            Assert.DoesNotThrow(() => webSocketProtocol.DataReceived(bytes, pd));
        }

        [Test]
        public void WithReceivingNullData()
        {
            Assert.Throws<ArgumentNullException>(() => webSocketProtocol.DataReceived(null, new ParseData()));
        }

        [Test]
        public void WithReceivingNullParseData()
        {
            Assert.Throws<ArgumentNullException>(() => webSocketProtocol.DataReceived(new byte[0], null));
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }

    public class SocketMock : ISocket
    {
        private readonly string headerContent;

        public SocketMock(string headerContent)
        {
            this.headerContent = headerContent + " \r\n\r\n";
        }

        public Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(headerContent);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void Close()
        {
            
        }
    }
}
