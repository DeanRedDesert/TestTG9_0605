//-----------------------------------------------------------------------
// <copyright file = "WebSocketLib.Tests.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;
    using Communications;
    using NUnit.Framework;
    using System.Text;

    [TestFixture]
    public class GivenWebSocketLib
    {
        private WebSocketLib wsLib;
        private List<AutomationCommandArgs> receivedCommands;
        private List<AutomationCommandArgs> receivedCommands2;

        private void OnAutomationCommandReceived(object sender, AutomationCommandArgs automationCommandArgs)
        {
            receivedCommands.Add(automationCommandArgs);
        }

        private void OnAutomationCommandReceived2(object sender, AutomationCommandArgs automationCommandArgs)
        {
            receivedCommands2.Add(automationCommandArgs);
        }

        [SetUp]
        public void Arrange()
        {
            wsLib = new WebSocketLib();
            receivedCommands = new List<AutomationCommandArgs>();
            receivedCommands2 = new List<AutomationCommandArgs>();
        }

        [TearDown]
        public void TearDown()
        {
            if(wsLib.IsOpen && !wsLib.Close())
            {
                wsLib.Close();
            }
        }

        #region Constructors

        [Test]
        public void WithDefaultConstructor()
        {
            Assert.DoesNotThrow(() => new WebSocketLib());
            Assert.IsNull(wsLib.HostSocket);
            Assert.IsNull(wsLib.ClientSocket);
            Assert.AreEqual(wsLib.Port, 0);
            Assert.IsNull(wsLib.IpAdd);
            Assert.IsFalse(wsLib.IsOpen);
        }

        #endregion Constructors

        [TestCase("127.0.0.1", 5785)]
        [TestCase("127.0.0.1", null)]
        [TestCase("127.0.0.1", 1024)]
        [TestCase("", 5785)]
        public void WhenConnectValidArguments(string address, int port)
        {
            Assert.IsTrue(wsLib.Connect(address, port));
            Assert.AreEqual(wsLib.IpAdd, address);
            Assert.AreEqual(wsLib.Port, port);
            Assert.IsTrue(wsLib.IsOpen);
        }

        [TestCase("localhost", 5785)]
        [TestCase("127.0.0.1", -1)]
        public void WhenConnectInValidArguments(string address, int port)
        {
            Assert.IsFalse(wsLib.Connect(address, port));
        }

        [Test]
        public void WhenClose()
        {
            wsLib.Connect("127.0.0.1", 5785);
            if(!wsLib.IsOpen)
            {
                Assert.Inconclusive("Failed to create server");
            }

            Assert.IsTrue(wsLib.Close());
            Assert.IsFalse(wsLib.IsOpen);
        }

        [Test]
        public void WhenCloseNotOpened()
        {
            Assert.IsFalse(wsLib.IsOpen);
            Assert.IsTrue(wsLib.Close());
            Assert.IsFalse(wsLib.IsOpen);
        }

        [Test]
        public void WhenIsOpen()
        {
            Assert.IsFalse(wsLib.IsOpen);

            wsLib.Connect("127.0.0.1", 5785);
            if(!wsLib.IsOpen)
            {
                Assert.Inconclusive("Failed to create server");
            }

            Assert.IsTrue(wsLib.IsOpen);
            Assert.IsTrue(wsLib.Close());
            Assert.IsFalse(wsLib.IsOpen);
        }

        [TestCase("127.0.0.1", 5785, false)]
        [TestCase("127.0.0.1", 9988, false)]
        [TestCase("", 5785, false)]
        [TestCase("127.0.0.1", 5785, true)]
        public void WhenCreateServerValidArguments(string address, int port, bool loopback)
        {
            Assert.DoesNotThrow(() => wsLib.CreateServer(address, port, loopback));
            Assert.AreEqual(wsLib.IpAdd, address);
            Assert.AreEqual(wsLib.Port, port);
            if(address.Length == 0)
            {
                Assert.AreEqual(wsLib.HostSocket.LocalEndPoint, new IPEndPoint(loopback ? IPAddress.Loopback : IPAddress.Any, port));
            }
            else
            {
                Assert.AreEqual(wsLib.HostSocket.LocalEndPoint, new IPEndPoint(IPAddress.Parse(address), port));
            }
            Assert.IsTrue(wsLib.IsOpen);
        }

        [TestCase("127.0.0.1", 5785, false)]
        [TestCase("127.0.0.1", 9988, false)]
        [TestCase("", 5785, false)]
        [TestCase("127.0.0.1", 5785, true)]
        public void WhenCreateServerServerAlreadyCreated(string address, int port, bool loopback)
        {
            wsLib.Connect(address, port);

            // Attempt to open another server with a different port
            var newPort = port + 1;
            Assert.DoesNotThrow(() => wsLib.CreateServer(address, newPort, loopback));

            Assert.AreEqual(wsLib.IpAdd, address);
            Assert.AreEqual(wsLib.Port, port);
            if(address.Length == 0)
            {
                Assert.AreEqual(wsLib.HostSocket.LocalEndPoint, new IPEndPoint(loopback ? IPAddress.Loopback : IPAddress.Any, port));
            }
            else
            {
                Assert.AreEqual(wsLib.HostSocket.LocalEndPoint, new IPEndPoint(IPAddress.Parse(address), port));
            }
            Assert.IsTrue(wsLib.IsOpen);

            Assert.AreNotEqual(wsLib.Port, newPort);
        }

        [TestCase("127.0.0.1", 5785, false)]
        [TestCase("127.0.0.1", 9988, false)]
        [TestCase("", 5785, false)]
        [TestCase("127.0.0.1", 5785, true)]
        public void WhenCreateServerClientAlreadyCreated(string address, int port, bool loopback)
        {
            wsLib.CreateClient(address, port);

            var newPort = port + 1;
            Assert.DoesNotThrow(() => wsLib.CreateServer(address, newPort, loopback));

            Assert.AreEqual(wsLib.IpAdd, null);
            Assert.AreEqual(wsLib.Port, 0);
            Assert.AreEqual(wsLib.ClientSocket.LocalEndPoint, null);
            Assert.AreNotEqual(wsLib.Port, newPort);
        }

        [TestCase("127.0.0.1", 5785)]
        [TestCase("127.0.0.1", 1024)]
        public void WhenCreateClientValidArguments(string address, int port)
        {
            var server = new WebSocketLib();
            if(!server.Connect(address, port))
            {
                Assert.Inconclusive("Failed to create server");
            }

            var clientCreated = wsLib.CreateClient(address, port);
            Assert.IsTrue(clientCreated);

            Assert.AreEqual(((IPEndPoint)wsLib.ClientSocket.RemoteEndPoint).Address, IPAddress.Parse(address));
            Assert.AreEqual(((IPEndPoint)wsLib.ClientSocket.RemoteEndPoint).Port, port);

            server.Close();
        }

        [TestCase("localhost", 5785)]
        [TestCase("127.0.0.1", -1)]
        public void WhenCreateClientInValidArguments(string address, int port)
        {
            var server = new WebSocketLib();
            server.Connect(address, port);

            var clientCreated = wsLib.CreateClient(address, port);
            Assert.IsFalse(clientCreated);

            server.Close();
        }

        [Test]
        public void WhenCreateClientNoServer()
        {
            var address = "127.0.0.1";
            int port = 5785;
            var client = wsLib.CreateClient(address, port);
            Assert.IsFalse(client);
        }

        [Test]
        public void WhenSendBroadcast()
        {
            var address = "127.0.0.1";
            int port = 5785;

            wsLib.Connect(address, port);

            var client1 = new WebSocketLib();
            client1.CreateClient(address, port);
            client1.AutomationCommandReceived += OnAutomationCommandReceived;

            var client2 = new WebSocketLib();
            client2.CreateClient(address, port);
            client2.AutomationCommandReceived += OnAutomationCommandReceived2;

            var command = new AutomationCommand("module", "command", "desc", "type", new List<AutomationParameter> { new AutomationParameter() }, true);
            wsLib.Send(command);

            // Need to wait for the commands to be processed by the clients
            Thread.Sleep(100);

            Assert.AreEqual(1, receivedCommands.Count);
            Assert.AreEqual(1, receivedCommands2.Count);

            var receivedCommand = receivedCommands.LastOrDefault();
            var receivedCommand2 = receivedCommands2.LastOrDefault();

            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(command.Module, receivedCommand.Command.Module);
            Assert.AreEqual(command.Command, receivedCommand.Command.Command);
            Assert.AreEqual(command.Description, receivedCommand.Command.Description);
            Assert.AreEqual(command.Type, receivedCommand.Command.Type);
            Assert.AreEqual(command.Parameters.Count, receivedCommand.Command.Parameters.Count);
            Assert.AreEqual(command.IsEvent, receivedCommand.Command.IsEvent);

            Assert.AreEqual(receivedCommand.Data, receivedCommand2.Data);
        }

        [Test]
        public void WhenSendToClient()
        {
            var address = "127.0.0.1";
            int port = 5785;

            wsLib.Connect(address, port);

            var client = new WebSocketLib();
            client.CreateClient(address, port);
            client.AutomationCommandReceived += OnAutomationCommandReceived;

            var prop = wsLib.GetType().GetProperty("hashList", BindingFlags.NonPublic | BindingFlags.Instance);
            var hashList = (prop.GetValue(wsLib, null) as List<Int32>);
            var clientHash = hashList.First();

            var command = new AutomationCommand("module", "command", "desc", "type", new List<AutomationParameter> { new AutomationParameter() }, true);
            wsLib.Send(command, clientHash);

            // Need to wait for client to process command
            Thread.Sleep(100);

            Assert.AreEqual(1, receivedCommands.Count);
            var receivedCommand = receivedCommands.LastOrDefault();

            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(command.Module, receivedCommand.Command.Module);
            Assert.AreEqual(command.Command, receivedCommand.Command.Command);
            Assert.AreEqual(command.Description, receivedCommand.Command.Description);
            Assert.AreEqual(command.Type, receivedCommand.Command.Type);
            Assert.AreEqual(command.Parameters.Count, receivedCommand.Command.Parameters.Count);
            Assert.AreEqual(command.IsEvent, receivedCommand.Command.IsEvent);
        }

        [Test]
        public void WhenSendToServer()
        {
            var address = "127.0.0.1";
            int port = 5785;

            var server = new WebSocketLib();
            server.Connect(address, port);
            server.AutomationCommandReceived += OnAutomationCommandReceived;

            wsLib.CreateClient(address, port);
            var command = new AutomationCommand("module", "command", "desc", "type", new List<AutomationParameter> { new AutomationParameter() }, true);
            wsLib.Send(command, 0);

            // Need to wait for server to process command
            Thread.Sleep(100);

            Assert.AreEqual(receivedCommands.Count, 1);
            var receivedCommand = receivedCommands.LastOrDefault();

            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(command.Module, receivedCommand.Command.Module);
            Assert.AreEqual(command.Command, receivedCommand.Command.Command);
            Assert.AreEqual(command.Description, receivedCommand.Command.Description);
            Assert.AreEqual(command.Type, receivedCommand.Command.Type);
            Assert.AreEqual(command.Parameters.Count, receivedCommand.Command.Parameters.Count);
            Assert.AreEqual(command.IsEvent, receivedCommand.Command.IsEvent);

            server.Close();
        }

        [Test]
        public void WhenDispatch()
        {
            var command = new AutomationCommand("module", "command", "desc", "type", new List<AutomationParameter> { new AutomationParameter() }, true);
            var data = Encoding.ASCII.GetBytes(AutomationCommand.Serialize(command));
            var hash = 112233;

            // Inject a client hash
            var newConnection = new Dictionary<Int32, WebSocketConnection>();
            newConnection.Add(hash, null);
            var prop = wsLib.GetType().GetProperty("connections", BindingFlags.NonPublic | BindingFlags.Instance);
            prop.SetValue(wsLib, newConnection, null);

            Assert.DoesNotThrow(() => wsLib.Dispatch(data, hash));

            wsLib.AutomationCommandReceived += OnAutomationCommandReceived;
            Assert.DoesNotThrow(() => wsLib.Dispatch(data, hash));

            // Wait for event to occur
            Thread.Sleep(100);

            Assert.AreEqual(receivedCommands.Count, 1);
            var receivedCommand = receivedCommands.LastOrDefault();

            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(command.Module, receivedCommand.Command.Module);
            Assert.AreEqual(command.Command, receivedCommand.Command.Command);
            Assert.AreEqual(command.Description, receivedCommand.Command.Description);
            Assert.AreEqual(command.Type, receivedCommand.Command.Type);
            Assert.AreEqual(command.Parameters.Count, receivedCommand.Command.Parameters.Count);
            Assert.AreEqual(command.IsEvent, receivedCommand.Command.IsEvent);
        }

        [Test]
        public void WhenDisconnectClient()
        {
            var address = "127.0.0.1";
            int port = 5785;

            var server = new WebSocketLib();
            server.Connect(address, port);

            wsLib.CreateClient(address, port);
            Assert.DoesNotThrow(() => wsLib.Disconnect());
            Thread.Sleep(100);
            Assert.IsFalse(wsLib.ClientSocket.Connected);

            server.Close();
        }

        [Test]
        public void WhenDisconnectServer()
        {
            var address = "127.0.0.1";
            var port = 5785;
            wsLib.Connect(address, port);

            var client = new WebSocketLib();
            client.CreateClient(address, port);

            Assert.DoesNotThrow(() => wsLib.Disconnect());

            client.Close();
        }

        [Test]
        public void WhenDisconnectServerWithMultipleClients()
        {
            var address = "127.0.0.1";
            var port = 5785;
            wsLib.Connect(address, port);

            var client1 = new WebSocketLib();
            client1.CreateClient(address, port);
            var client2 = new WebSocketLib();
            client2.CreateClient(address, port);

            Assert.DoesNotThrow(() => wsLib.Disconnect());

            client1.Close();
            client2.Close();
        }

        [Test]
        public void WhenCommunicationError()
        {
            var address = "127.0.0.1";
            int port = 5785;

            wsLib.Connect(address, port);

            var client = new WebSocketLib();
            client.CreateClient(address, port);
            client.AutomationCommandReceived += OnAutomationCommandReceived;

            var prop = wsLib.GetType().GetProperty("hashList", BindingFlags.NonPublic | BindingFlags.Instance);
            var hashList = (prop.GetValue(wsLib, null) as List<Int32>);
            var clientHash = hashList.First();

            Assert.DoesNotThrow(() => wsLib.CommunicationError(clientHash));

            client.Close();
        }

        [Test]
        public void WhenMultiThreadConnectionsCloses()
        {
            var address = "127.0.0.1";
            int port = 5785;

            wsLib.Connect(address, port);

            var startTime = DateTime.Now;

            // Spin up a bunch of threads to randomly open and close the wsLib
            for (var i = 0; i < 100; i++)
            {
                Thread t = new Thread(() => RandomWebSocket(wsLib));
                t.Start();
            }

            var endTime = startTime.AddSeconds(30);
            while(string.IsNullOrEmpty(ThreadError) && DateTime.Now < endTime)
            {
                Thread.Sleep(100);
            }

            if(!string.IsNullOrEmpty(ThreadError))
            {
                Assert.Fail(ThreadError);
            }
        }

        private string ThreadError;

        private void RandomWebSocket(WebSocketLib wsLib)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(new Random().Next(0, 1000));
                    wsLib.Close();
                    Thread.Sleep(new Random().Next(0, 1000));
                    wsLib.Connect(wsLib.IpAdd, wsLib.Port);
                }
            }
            catch(Exception ex)
            {
                ThreadError = ex.ToString();
            }
        }
    }
}
