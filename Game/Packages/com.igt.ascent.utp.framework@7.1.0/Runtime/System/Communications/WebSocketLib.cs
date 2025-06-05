// -----------------------------------------------------------------------
// <copyright file = "WebSocketLib.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// WebSocketLib for UTP inter-communication.
    /// </summary>
    public class WebSocketLib : IUtpConnection, ICommPortal
    {
        #region fields

        /// <summary>
        /// Event for UTP communication.
        /// </summary>
        public event EventHandler<AutomationCommandArgs> AutomationCommandReceived;

        /// <summary>
        /// Host socket for UTP communication.
        /// </summary>
        public Socket HostSocket { get; set; }

        /// <summary>
        /// Client socket for UTP communication.
        /// </summary>
        public Socket ClientSocket { get; set; }

        /// <summary>
        /// Port number for UTP communication.
        /// </summary>
        public Int32 Port { get; private set; }

        /// <summary>
        /// IP Address for UTP communication.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string IpAdd { get; private set; }

        /// <summary>
        /// Server successfully created flag.
        /// </summary>
        private bool serverCreated;

        /// <summary>
        /// Client successfully created flag.
        /// </summary>
        private bool clientCreated;

        /// <summary>
        /// Server is listening flag.
        /// </summary>
        private bool isListening;

        /// <summary>
        /// Stores the connections. Clients shall have a single connection to the server. Servers can have multiple connections to it.
        /// </summary>
        private Dictionary<Int32, WebSocketConnection> connections { get; set; }

        /// <summary>
        /// UTP communication hash list.
        /// </summary>
        private List<Int32> hashList { get; set; }
        private object hashLock = new object();

        #endregion

        #region constructor

        /// <summary>
        /// WebSocketLib() Constructor.
        /// </summary>
        public WebSocketLib()
        {
            serverCreated = clientCreated = false;
            connections = new Dictionary<Int32, WebSocketConnection>();
            hashList = new List<int>();
        }

        #endregion

        #region IUtpConnectionRealization

        /// <summary>
        /// Create a server with connection parameters
        /// </summary>
        /// <param name="address">IP address to connect to.</param>
        /// <param name="port">Port address to connect to.</param>
        /// <returns>Returns true no matter what.</returns>
        public bool Connect(string address, int port = 5780)
        {
            CreateServer(address, port, false);
            return (serverCreated && isListening);
        }

        /// <summary>
        /// Close connection to client.
        /// </summary>
        /// <returns>Client or Server status flag.</returns>
        public bool Close()
        {
            Disconnect();

            if(serverCreated && HostSocket.IsBound)
            {
                isListening = false;
                HostSocket.Close();
                serverCreated = false;
            }
            if(clientCreated && ClientSocket.IsBound)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
                clientCreated = false;
            }

            return IsOpen == false;
        }

        /// <summary>
        /// Check if able to establish connection flag.
        /// </summary>
        public bool IsOpen
        {
            get { return (HostSocket != null && isListening) || (ClientSocket != null && ClientSocket.IsBound); }
        }

        #endregion IUtpConnection_Realization

        #region IUtpCommunication Realization

        /// <summary>
        /// Send() broadcasts the AutomationCommand to all connected clients.
        /// </summary>
        /// <param name="data">The AutomationCommand to broadcast.</param>
        /// <returns>Connection fail status flag.</returns>
        public bool Send(AutomationCommand data)
        {
            if(connections != null)
            {
                return connections.Aggregate(true, (current, connection) => current & connection.Value.Send(data));
            }
            return false;
        }

        #endregion IUtpCommunication Realization

        #region CreateServer

        /// <summary>
        /// CreateServer() creates a server by opening a socket and listening on it.
        /// Connecting clients are subjected to a WebSocket handshake to produce a proper connection.
        /// </summary>
        /// <param name="address">The address of the local port to listen to</param>
        /// <param name="port">The port that will be used to listen to.</param>
        /// <param name="loopback">Choice of provided address, or the looback address.</param>
        public void CreateServer(string address, Int32 port, bool loopback)
        {
            // Sanity checking.
            if(serverCreated || clientCreated)
            {
                // A server or client has already been created, so don't create another one.
                return;
            }
            serverCreated = true;

            IpAdd = address;

            Port = port;

            try
            {
                HostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                // If the address string length is zero, then select loopback or any, or if not zero, the passed in IP address.
                IPEndPoint ipEndPointp = address.Length == 0 ? new IPEndPoint((loopback ? IPAddress.Loopback : IPAddress.Any), Port) : new IPEndPoint(IPAddress.Parse(address), port);
                HostSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
                HostSocket.Bind(ipEndPointp);
                HostSocket.Listen(100);
                isListening = true;
            }
            catch
            {
                //  Nothing to do
            }

            ListenForClients();
        }

        /// <summary>
        /// ListenForClients() begins accepting a connection on the ClientSocket.
        /// </summary>
        private void ListenForClients()
        {
            try
            {
                // Send the socket as the state object. It will show up in the IAsyncResult when OnClientConnect is called.
                HostSocket.BeginAccept(OnClientConnect, HostSocket);
            }
            catch(Exception ex) when(ex is ObjectDisposedException || ex is NotSupportedException || ex is InvalidOperationException ||
                                     ex is ArgumentOutOfRangeException || ex is SocketException)
            {
                //  Nothing to do
            }
        }

        /// <summary>
        /// OnClientConnect() is called when a connection occurs.
        /// This performs a WebSocket handshake with the client.
        /// </summary>
        /// <param name="asyncResult">IAsyncResult information.</param>
        private void OnClientConnect(IAsyncResult asyncResult)
        {
            // End the accept, and get the proper socket.
            try
            {
                if(!isListening)
                {
                    return;
                }

                // Get the passed in socket. Prescribed method of acquiring the socket to the connected client. See MSDN.
                var listener = (Socket)asyncResult.AsyncState;
                Socket clientSocket = listener.EndAccept(asyncResult);

                // Now we go examine the passed HTTP, to determine if it is upgradeable.
                if(WebSocketProtocol.PerformServerHandshake(new TcpSocket(clientSocket)))
                {
                    // An upgraded connection has been established.
                    var connection = new WebSocketConnection(this, clientSocket, new WebSocketProtocol(false));
                    connection.Disconnected += Disconnected;
                    connections.Add(connection.GetHashCode(), connection);

                    lock(hashLock)
                    {
                        hashList.Add(connection.GetHashCode());
                    }
                }
            }
            catch(Exception ex) when(ex is ArgumentNullException || ex is ArgumentException || ex is SocketException ||
                                     ex is ObjectDisposedException || ex is InvalidOperationException || ex is NotSupportedException)
            {
                //  Nothing to do
            }

            // Now look for new connections.
            ListenForClients();
        }

        #endregion // CreateServer

        #region CreateClient

        /// <summary>
        /// CreateClient() creates a client by opening a socket and trying to connect to a server.
        /// When the socket connects, perform a WebSocket handshake.
        /// </summary>
        /// <param name="address">The address of the server to connect to.</param>
        /// <param name="port">The port on the server to connect to.</param>
        public bool CreateClient(string address, Int32 port)
        {
            // Sanity checking.
            if(clientCreated || serverCreated)
            {
                // A client or a server has already been created, so don't create another one.
                return false;
            }
            clientCreated = true;

            try
            {
                // Create a TCP/IP socket.
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(address);
                var remoteEp = new IPEndPoint(ipAddress, port);
                ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));
                // Synchronous blocking way to connect to server.
                ClientSocket.Connect(remoteEp);
            }
            catch
            {
                //  Nothing to do
            }
            return (SetupAfterConnect(ClientSocket));
        }

        /// <summary>
        /// SetupAfterConnect()
        /// Once this client has connected to a server, set up a WebSocket connection.
        /// If the WebSocket handshake succeeds, 
        /// </summary>
        /// <param name="client">The client with which the server is connected.</param>
        private bool SetupAfterConnect(Socket client)
        {
            bool connected = false;
            if(WebSocketProtocol.PerformClientHandshake(client))
            {
                // Create a WebSocketConnection with the server.
                // An upgraded connection has been established.
                var connection = new WebSocketConnection(this, client, new WebSocketProtocol(true));
                connection.Disconnected += Disconnected;
                connections.Add(connection.GetHashCode(), connection);
                lock(hashLock)
                {
                    hashList.Add(connection.GetHashCode());
                }
                connected = true;
            }
            return connected;
        }

        #endregion CreateClient

        /// <summary>
        /// Send() - Only visible access point to send data over the WebSocket protocol.
        /// </summary>
        /// <param name="data">An AutomationCommand.</param>
        /// <param name="hash">The communication channel identifier.</param>
        public bool Send(AutomationCommand data, Int32 hash)
        {
            // Send an AutomationCommand, serialized into a string.
            if(clientCreated)
            {
                // Clients have only one WebSocket communication channel.
                lock(hashLock)
                {
                    connections[hashList.FirstOrDefault()].OnSend(AutomationCommand.Serialize(data));
                }
            }
            else
            {
                // Servers have to know how to pick the WebSocket communication channel.
                connections[hash].OnSend(AutomationCommand.Serialize(data));
            }
            return true;
        }

        /// <summary>
        /// Dispatch() - When data is available from the WebSocket, this raises the data delivery event.
        /// Provides an identifier to the WebSocket communications channel to be used for replies.
        /// </summary>
        /// <param name="data">Data byte string.</param>
        /// <param name="hash">Integrity hash.</param>
        public void Dispatch(byte[] data, Int32 hash)
        {
            if(AutomationCommandReceived != null)
            {
                AutomationCommandReceived.Invoke(connections[hash], new AutomationCommandArgs(new string(Encoding.UTF8.GetChars(data))));
            }
        }

        /// <summary>
        /// Disconnected() - Stuff to do when after the WebSocket disconnects.
        /// The passed WebSocketConnection shall be removed from the dictionary of connections.
        /// Add the identifier to the dead list.
        /// </summary>
        /// <param name="sender">The WebSocketConnection to remove from the dictionary.</param>
        /// <param name="e">Unused parameter.</param>
        private void Disconnected(WebSocketConnection sender, EventArgs e)
        {
            Int32 hash = sender.GetHashCode();
            if(connections.ContainsKey(hash))
            {
                connections[hash].Close();
                connections.Remove(hash);
            }
        }

        /// <summary>
        /// Disconnect() is set up to disconnect WebSocket connections from internal requests.
        /// </summary>
        public void Disconnect()
        {
            // Go through each hash and remove the connection by hash.
            lock(hashLock)
            {
                foreach(var hash in hashList)
                {
                    if(connections.ContainsKey(hash))
                    {
                        connections[hash].Disconnect();
                    }
                }
                hashList.Clear();
            }
        }

        /// <summary>
        /// CommunicationError() - If a connection has a problem, then disconnect it, close the socket, and remove the connection from the dictionary.
        /// </summary>
        /// <param name="hash">Integrity hash.</param>
        public void CommunicationError(Int32 hash)
        {
            if(connections.ContainsKey(hash))
            {
                connections[hash].Disconnect();
            }
        }
    }
}