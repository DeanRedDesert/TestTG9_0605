//-----------------------------------------------------------------------
// <copyright file = "SocketTransport.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Threading;

    /// <summary>
    /// Base class used for socket transport for the foundation.
    /// </summary>
    public class SocketTransport : ITransport, IDisposable
    {
        #region Fields

        /// <summary>
        /// Address used to connect to the foundation.
        /// </summary>
        protected readonly string Address;

        /// <summary>
        /// Port used to connect to the foundation.
        /// </summary>
        protected readonly ushort Port;

        /// <summary>
        /// Manual reset event used to determine when a connection has been completed.
        /// </summary>
        private readonly AutoResetEvent connectResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Field used to store the reason that a connection failed.
        /// </summary>
        protected string ConnectFailureReason;

        /// <summary>
        /// Inner exception which caused a connect failure.
        /// </summary>
        protected Exception ConnectFailureInnerException;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        protected bool Disposed;

        /// <summary>
        /// TcpClient used for managing the connection to the foundation.
        /// </summary>
        protected TcpClient Client;

        /// <summary>
        /// Delegate to invoke when an application message is received.
        /// </summary>
        protected HandleMessageDelegate HandleMessage;

        /// <summary>
        /// Timeout to use when connecting to the foundation. If the foundation does not respond within this
        /// period of time, then the connection will fail.
        /// </summary>
        protected const int ConnectTimeout = 5000;

        /// <summary>
        /// Retry time for socket buffer exceptions.
        /// </summary>
        protected const int SendBufferRetry = 30;

        /// <summary>
        /// Buffer used to receiving foundation transport headers.
        /// </summary>
        protected readonly byte[] HeaderBuffer = new byte[TransportHeaderSegment.SegmentSize];

        /// <summary>
        /// Used to store any exceptions which are caught in communication threads.
        /// </summary>
        private List<Exception> pendingExceptions;

        /// <summary>
        /// Used to synchronize the operations to the pending exceptions.
        /// </summary>
        private readonly object pendingExceptionsLock = new object();

        /// <summary>
        /// Auto reset event to determine whether an exception is thrown on the transport callback thread.
        /// </summary>
        private readonly AutoResetEvent transportExceptionResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Used to synchronize the operations of closing TCP client.
        /// </summary>
        private readonly object clientSocketLocker = new object();

        /// <summary>
        /// Set to true when connected to the TCP transport; false when disconnected.
        /// </summary>
        private volatile bool connected;

        /// <summary>
        /// Set to true when the client is in the process of disconnecting.
        /// </summary>
        protected volatile bool IsDisconnecting;

        #endregion

        #region Properties

        /// <summary>
        /// Major version of the specific foundation protocol supported.
        /// </summary>
        /// <devdoc>
        /// This property stores the major version in the form expected by the binary format. It
        /// is an implementation detail of <see cref="SocketTransport"/> and its derived types.
        /// </devdoc>
        protected byte VersionMajor { get; private set; }

        /// <summary>
        /// Minor version of the specific foundation protocol supported.
        /// </summary>
        /// <devdoc>
        /// This property stores the minor version in the form expected by the binary format. It
        /// is an implementation detail of <see cref="SocketTransport"/> and its derived types.
        /// </devdoc>
        protected byte VersionMinor { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create an instance of the transport.
        /// </summary>
        /// <param name="address">The IP address to use to connect to the foundation.</param>
        /// <param name="port">The port to connect on.</param>
        /// <exception cref="ArgumentNullException">Thrown when the address is null.</exception>
        public SocketTransport(string address, ushort port) : this(address, port, 1, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the transport with the specified version.
        /// </summary>
        /// <param name="address">The IP address to use to connect to the foundation.</param>
        /// <param name="port">The port to connect on.</param>
        /// <param name="versionMajor">The desired major version.</param>
        /// <param name="versionMinor">The desired minor version.</param>
        /// <exception cref="ArgumentNullException">Thrown when the address is null.</exception>
        public SocketTransport(string address, ushort port, byte versionMajor, byte versionMinor)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Port = port;
            VersionMajor = versionMajor;
            VersionMinor = versionMinor;
        }

        #endregion

        #region ITransport Implementation

        /// <inheritdoc />
        /// <devdoc>
        /// This property is implemented explicitly to avoid confounding <see cref="VersionMajor"/> with
        /// <see cref="ITransport.MajorVersion"/>. Clients of <see cref="SocketTransport"/> can use
        /// the former, while clients of <see cref="ITransport"/> should use the latter, which adapts
        /// the type to a more widely applicable <see cref="int"/>.
        /// </devdoc>
        int ITransport.MajorVersion => VersionMajor;

        /// <inheritdoc />
        /// <devdoc>
        /// This property is implemented explicitly to avoid confounding <see cref="VersionMinor"/> with
        /// <see cref="ITransport.MinorVersion"/>. Clients of <see cref="SocketTransport"/> can use
        /// the former, while clients of <see cref="ITransport"/> should use the latter, which adapts
        /// the type to a more widely applicable <see cref="int"/>.
        /// </devdoc>
        int ITransport.MinorVersion => VersionMinor;

        /// <inheritdoc />
        public virtual void Connect()
        {
            var throwAsyncException = false;
            Exception syncException = null;
            IsDisconnecting = false;

            lock(clientSocketLocker)
            {
                if(Client != null && Client.Client != null && Client.Connected)
                {
                    throw new ConnectionException("Client already connected.", Address, Port);
                }

                // The current Socket implementation in Mono throws an exception if both a IPv4 and IPv6 address are
                // available for a host and a connection attempt is made utilizing the DNS name. To avoid this, we
                // specify we want to us IPv4, then resolve the addresses for the host name and choose the IPv4
                // version of the address. This prevents the underlying Socket from erroneously using the
                // IPv6 address, which causes an exception.
                Client = new TcpClient(AddressFamily.InterNetwork);

                var addrs = Dns.GetHostAddresses(Address);
                IPAddress ipv4Addr = null;

                // Don't use foreach/linq here, as it causes allocations. Avoid dependency on System.Linq in low-level components.
                for(var i = 0; i < addrs.Length; i++)
                {
                    if(addrs[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4Addr = addrs[i];
                        break;
                    }
                }
                if(ipv4Addr == null)
                {
                    throw new ConnectionException("Could not obtain IPv4 address.", Address, Port);
                }
                Client.BeginConnect(ipv4Addr, Port, OnConnect, Client);
            }

            // Set the default connect failure reason to use in the case of the timeout elapsing.
            ConnectFailureReason = "Timeout connecting to host.";

            // Reset the inner exception to null in case a previous attempt was made to connect.
            ConnectFailureInnerException = null;

            // Allow a 5 second timeout for the connection to succeed.
            connectResetEvent.WaitOne(ConnectTimeout);

            // Client.Client could be null on some connection failures
            // and accessing Client.Connected would cause a NullReferenceException in this case.
            lock(clientSocketLocker)
            {
                if(Client.Client == null || !Client.Connected)
                {
                    throwAsyncException = true;
                }
            }

            if(throwAsyncException)
            {
                CloseClient();
                ThrowConnectFailureException();
            }
            else
            {
                // Start accepting data from the foundation.
                StartReceive();

                lock(clientSocketLocker)
                {
                    //Send the connection request to the foundation.
                    try
                    {
                        Client.Client.Send(ConnectMessage.CreateRequest(VersionMajor, VersionMinor).GetBytes());
                    }
                    catch(SocketException ex)
                    {
                        syncException = ex;
                    }
                    catch(ObjectDisposedException ex)
                    {
                        syncException = ex;
                    }
                }
            }

            // Check if the connect process failed.
            if(syncException != null)
            {
                CloseClient();
                throw new ConnectionException("Could not send connect message.", Address, Port, syncException);
            }
        }

        /// <inheritdoc />
        public virtual void PrepareToDisconnect()
        {
            IsDisconnecting = true;
        }

        /// <inheritdoc />
        public virtual void Disconnect()
        {
            IsDisconnecting = true;
            CloseClient();
        }

        /// <inheritdoc />
        public void SendMessage(IBinaryMessage message)
        {
            SocketTransportTracing.Log.SendMessageStart(message.Size);
            Exception syncException = null;

            lock(clientSocketLocker)
            {
                if(Client?.Client == null || !Client.Connected)
                {
                    throw new ConnectionException("Transport is not connected.", Address, Port);
                }

                var sent = false;

                while(!sent && syncException == null)
                {
                    try
                    {
                        // create a transport header and insert it into the message
                        var header = new TransportHeaderSegment(
                                (uint)(TransportHeaderSegment.SegmentSize + message.Size),
                                MessageType.Application);

                        message.Prepend(header);

                        // allocate a buffer and move the data into it
                        var buffer = new byte[message.Size];

                        message.Write(buffer, 0);

                        Client.Client.Send(buffer);

                        sent = true;
                    }
                    catch(SocketException ex)
                    {
                        if(ex.SocketErrorCode == SocketError.WouldBlock ||
                           ex.SocketErrorCode == SocketError.IOPending ||
                           ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                        {
                            // socket buffer is probably full, wait and try again
                            Thread.Sleep(30);
                        }
                        else
                        {
                            syncException = ex;
                        }
                    }
                    catch(ObjectDisposedException ex)
                    {
                        syncException = ex;
                    }
                    catch(NullReferenceException ex)
                    {
                        syncException = ex;
                    }
                }
            }

            // If any unexpected exceptions occurred, close the client socket and throw a ConnectionException.
            if(syncException != null)
            {
                SocketTransportTracing.Log.SendMessageStop(false);
                CloseClient();
                throw new ConnectionException("Could not send message.", Address, Port, syncException);
            }
            SocketTransportTracing.Log.SendMessageStop(true);
        }

        /// <inheritdoc />
        public void SetMessageHandler(HandleMessageDelegate handler)
        {
            HandleMessage = handler;
        }

        #endregion

        #region IExceptionMonitor Implementation

        /// <inheritdoc />
        WaitHandle IExceptionMonitor.ExceptionSignalHandle => transportExceptionResetEvent;

        /// <inheritdoc />
        Exception IExceptionMonitor.CheckException()
        {
            lock(pendingExceptionsLock)
            {
                var transportException = pendingExceptions != null ? new SocketTransportException(Address, Port, pendingExceptions) : null;

                pendingExceptions = null;
                return transportException;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Receive bytes from the socket.
        /// </summary>
        /// <param name="receivedBytes">The number of bytes received so far.</param>
        /// <param name="messageBuffer">Message buffer to add received bytes to.</param>
        /// <returns>The current number of bytes received.</returns>
        /// <exception cref="ConnectionException">
        /// Thrown when there is a problem receiving data from the foundation.
        /// </exception>
        private int GetReceivedBytes(int receivedBytes, byte[] messageBuffer)
        {
            try
            {
                lock(clientSocketLocker)
                {
                    receivedBytes += Client.Client.Receive(messageBuffer, receivedBytes,
                        messageBuffer.Length - receivedBytes,
                        SocketFlags.None);
                }
            }
            catch(SocketException ex)
            {
                // If IOPending then there is an overlapping operation that cannot be completed immediately.
                // If WouldBlock then the buffer was empty and we should wait for it to contain something.
                // Buffer space could be full because of an in progress send.
                if(ex.SocketErrorCode == SocketError.WouldBlock ||
                   ex.SocketErrorCode == SocketError.IOPending ||
                   ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    // socket buffer is probably empty, wait and try again
                    Thread.Sleep(SendBufferRetry);
                }
                else
                {
                    CloseClient();
                    throw new ConnectionException("Failed to receive data.", Address, Port, ex);
                }
            }
            catch(ObjectDisposedException ex)
            {
                CloseClient();
                throw new ConnectionException("Could not start receive.", Address, Port, ex);
            }
            catch(NullReferenceException ex)
            {
                CloseClient();
                throw new ConnectionException("Could not start receive.", Address, Port, ex);
            }
            return receivedBytes;
        }

        /// <summary>
        /// End an asynchronous receive.
        /// </summary>
        /// <param name="asyncResult">Asynchronous result.</param>
        /// <exception cref="ConnectionException">
        /// Thrown when there is a problem receiving data from the foundation.
        /// </exception>
        protected void EndReceive(IAsyncResult asyncResult)
        {
            try
            {
                // Use the socket embedded in the asyncResult to avoid
                // random socket exceptions caused by disconnection and
                // re-connection.
                var byteReadCount = ((Socket)asyncResult.AsyncState).EndReceive(asyncResult);

                if(byteReadCount < HeaderBuffer.Length)
                {
                    // Check if either the client or host closed, or is in the process of closing, the socket.
                    if(byteReadCount == 0 || !connected || IsDisconnecting)
                    {
                        // This read was completed in the process of closing the socket.
                        // If the socket has not yet been disposed, then this will receive 0 bytes,
                        // which indicates a graceful close.
                        // OnFoundationDataReceived() is listening for an SocketClosingException
                        // to indicate that the socket was closed during normal operation.
                        // Here, the 'connected' flag being set to false indicates that we are in
                        // the process of closing the socket and a read of 0 bytes will be expected.
                        throw new SocketClosingException(
                            $"Asynchronous read completed with {byteReadCount} bytes; the socket is most likely closed or is closing. " +
                            $"Connected == {connected}, IsDisconnecting == {IsDisconnecting}.", null);
                    }

                    SocketTransportTracing.Log.ReceiveStop(false);
                    throw new ConnectionException(
                        $"Asynchronous read completed with {byteReadCount} bytes. Expected at least {HeaderBuffer.Length} bytes. " +
                        $"Connected == {connected}, IsDisconnecting == {IsDisconnecting}.", Address, Port);
                }
            }
            catch(ObjectDisposedException ex)
            {
                // When disconnect, the asynchronous BeginReceive will be terminated
                // by the Socket.Close call, which leads to EndReceive throwing an
                // ObjectDisposedException.
                // Propagate the exception to the caller of EndReceive, so that the
                // caller can bypass any step that follows EndReceive.
                SocketTransportTracing.Log.ReceiveStop(false);
                throw new SocketClosingException("Client object was disposed in EndReceive call.", ex);
            }
            catch(ArgumentNullException ex)
            {
                SocketTransportTracing.Log.ReceiveStop(false);
                throw new ConnectionException("Client disconnected.", Address, Port, ex);
            }
            catch(ArgumentException ex)
            {
                SocketTransportTracing.Log.ReceiveStop(false);
                throw new ConnectionException("Asynchronous result not from BeginReceive call.", Address, Port, ex);
            }
            catch(InvalidOperationException ex)
            {
                SocketTransportTracing.Log.ReceiveStop(false);
                throw new ConnectionException("EndReceive was already called for the asynchronous read.", Address, Port,
                                              ex);
            }
            catch(SocketException ex)
            {
                lock(clientSocketLocker)
                {
                    // If client has been set to null, it means that this socket exception was due to a normal disconnect.
                    // If we are in the process of disconnecting, ignore any error code.
                    if((ex.SocketErrorCode == SocketError.Shutdown && Client == null) || IsDisconnecting)
                    {
                        SocketTransportTracing.Log.ReceiveStop(false);
                        throw new SocketClosingException("EndReceive terminated due to disconnection.", ex);
                    }

                    // For now assume that a socket exception at this point represents an actual problem.
                    // Considering it was asynchronous there should not be a buffer problem.
                    SocketTransportTracing.Log.ReceiveStop(false);
                    throw new ConnectionException("Receive failure.", Address, Port, ex);
                }
            }
            SocketTransportTracing.Log.ReceiveStop(true);
        }

        /// <summary>
        /// Start receiving data from the foundation.
        /// </summary>
        /// <exception cref="ConnectionException">
        /// Thrown when there is an error starting the asynchronous receive.
        /// </exception>
        protected void StartReceive()
        {
            SocketTransportTracing.Log.ReceiveStart();
            Exception syncException = null;

            lock(clientSocketLocker)
            {
                // If 'Client' is null then the connection is either in the process of being closed or has closed.
                // If so, do nothing.
                if(Client != null)
                {
                    // Client.Client could be null on some connection failures and accessing Client.Connected
                    // would cause a NullReferenceException in this case.
                    if(Client.Client != null && Client.Connected)
                    {
                        try
                        {
                            Client.Client.BeginReceive(HeaderBuffer, 0, HeaderBuffer.Length, SocketFlags.None,
                                OnAsyncReceiveComplete, Client.Client);
                        }
                        catch(SocketException ex)
                        {
                            syncException = ex;
                        }
                        catch(ObjectDisposedException ex)
                        {
                            syncException = ex;
                        }
                        catch(NullReferenceException ex)
                        {
                            syncException = ex;
                        }
                    }
                    else
                    {
                        syncException = new Exception("Client.Client is null or Client is not connected.");
                    }
                }
            }

            if(syncException != null)
            {
                SocketTransportTracing.Log.ReceiveStop(false);
                CloseClient();
                throw new ConnectionException("Could not start receive.", Address, Port, syncException);
            }
        }

        /// <summary>
        /// Immediate callback for Socket.BeginReceive() that ensures <see cref="OnFoundationDataReceived"/> is
        /// always executed on a separate thread from the one that called Socket.BeginReceive().
        /// </summary>
        /// <param name="ar">Asynchronous result of the connection.</param>
        private void OnAsyncReceiveComplete(IAsyncResult ar)
        {
            if(ar.CompletedSynchronously)
            {
                // MS's implementation of Socket.BeginReceive executes the callback immediately on the same thread
                // if data is already available in the buffer, which results consecutive messages executing on the
                // same thread, which can cause deadlocks in the client. If this happens, push it off into another
                // thread from the system's thread pool instead.
                ThreadPool.QueueUserWorkItem((state) => OnFoundationDataReceived(ar));
            }
            else
            {
                OnFoundationDataReceived(ar);
            }
        }

        /// <summary>
        /// Function called when a connection is established using BeginConnect.
        /// </summary>
        /// <param name="ar">Asynchronous result of the connection.</param>
        /// <exception cref="ConnectionException">Thrown when there is a problem connecting to the host.</exception>
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                lock(clientSocketLocker)
                {
                    ((TcpClient)ar.AsyncState).EndConnect(ar);
                }
                connected = true;
            }
            catch(SocketException socketException)
            {
                ConnectFailureReason = "Could not connect to host.";
                ConnectFailureInnerException = socketException;
            }
            catch(ObjectDisposedException objectDisposedException)
            {
                ConnectFailureReason = "Client disconnected.";
                ConnectFailureInnerException = objectDisposedException;
            }
            catch(ArgumentNullException argumentNullException)
            {
                ConnectFailureReason = "Client disconnected.";
                ConnectFailureInnerException = argumentNullException;
            }
            catch(NullReferenceException nullReferenceException)
            {
                ConnectFailureReason = "Host refusing connection.";
                ConnectFailureInnerException = nullReferenceException;
            }

            connectResetEvent.Set();
        }

        /// <summary>
        /// Close the socket connection to the foundation,
        /// and dispose the TCP client and its socket.
        /// </summary>
        private void CloseClient()
        {
            // Denote that the socket is now in the process of closing.
            connected = false;

            // Close and dispose the socket.
            lock(clientSocketLocker)
            {
                if(Client != null)
                {
                    if(Client.Client != null && Client.Connected)
                    {
                        // This call to shutdown will ensure all data is sent and received on the connected
                        // socket before it is closed. This may result in an asynchronous read of 0 bytes if
                        // the call to Close() is not completed before the EndReceive is handled.
                        Client.Client.Shutdown(SocketShutdown.Both);
                        Client.Client.Close();
                    }

                    // Dispose TcpClient.
                    Client.Close();
                    Client = null;
                }
            }
        }

        /// <summary>
        /// Throw a connection exception for a timeout, or for an exception caught during OnConnect.
        /// </summary>
        private void ThrowConnectFailureException()
        {
            if(ConnectFailureInnerException != null)
            {
                throw new ConnectionException(ConnectFailureReason, Address, Port, ConnectFailureInnerException);
            }
            throw new ConnectionException(ConnectFailureReason, Address, Port);
        }

        /// <summary>
        /// Handle data received from an asynchronous receive.
        /// </summary>
        /// <param name="asyncResult">Asynchronous result.</param>
        private void OnFoundationDataReceived(IAsyncResult asyncResult)
        {
            try
            {
                ProcessReceive(asyncResult);
            }
            catch(SocketClosingException)
            {
                // Do nothing. This is thrown when trying to read from a socket that is disposed, or 0 bytes are received while
                // in the process of closing the connection. This is normal during the process of disconnecting.
            }
            catch(Exception exception)
            {
                // If we're disconnecting, ignore any exceptions on this read.
                if(!IsDisconnecting)
                {
                    lock(pendingExceptionsLock)
                    {
                        if(pendingExceptions == null)
                        {
                            pendingExceptions = new List<Exception>();
                        }
                        pendingExceptions.Add(exception);
                    }

                    // Once an error has occurred disconnect to prevent further issues.
                    SocketTransportTracing.Log.ReceiveStop(false);
                    CloseClient();
                    transportExceptionResetEvent.Set();
                }
            }
        }

        /// <summary>
        /// Process the asynchronous data received from the foundation.
        /// </summary>
        /// <param name="asyncResult">Asynchronous result.</param>
        /// <exception cref="ConnectionException">
        /// Thrown when the message received is empty or the message header is null.
        /// </exception>
        protected virtual void ProcessReceive(IAsyncResult asyncResult)
        {
            // End the current asynchronous read.
            EndReceive(asyncResult);

            if(IsDisconnecting)
            {
                return;
            }

            var messageReader = ReceiveCompleteMessage(out var messageType);

            if(messageReader == null)
            {
                throw new ConnectionException(
                    "An empty message was received. There might be a problem in the connection", Address, Port);
            }

            if(IsDisconnecting)
            {
                return;
            }

            // Allow receive while processing message.
            StartReceive();

            switch(messageType)
            {
                case MessageType.Transport:
                    var body = messageReader.Read<TransportBodyHeaderSegment>();

                    if(body.BodyType == BodyType.ConnectionAccepted)
                    {
                        var connect = messageReader.Read<ConnectBinaryMessageSegment>();
                        if(connect.VersionMajor != VersionMajor)
                        {
                            var message = $"Invalid major version: {connect.VersionMajor}. This transport is configured to support major version {VersionMajor}.";
                            throw new InvalidMessageException(message);
                        }
                    }
                    break;
                case MessageType.Application:
                    var tempHandler = HandleMessage;
                    if(tempHandler != null)
                    {
                        SocketTransportTracing.Log.InvokeHandlerStart();
                        tempHandler(messageReader);
                        SocketTransportTracing.Log.InvokeHandlerStop();
                    }
                    break;

                default:
                    throw new InvalidMessageException("Invalid message type: " + messageType);
            }
        }

        /// <summary>
        /// Reads the data from AsynchronousResult and converts into a byte array.
        /// </summary>
        protected IBinaryMessageReader ReceiveCompleteMessage(out MessageType messageType)
        {
            var reader = new BinaryMessageReader(HeaderBuffer);
            var header = reader.Read<TransportHeaderSegment>();
            var calculatedCrc = header.CalculateCrc();

            // Either the header is invalid or data was received which was not a header.
            if(header.Crc != calculatedCrc)
            {
                throw new MessageCrcException(header.MessageType, header.Crc, calculatedCrc);
            }

            var receivedBytes = 0;
            var messageBuffer = new byte[header.PacketLength - header.Size];

            // Synchronously receive the remainder of the message.
            while(receivedBytes < messageBuffer.Length)
            {
                receivedBytes = GetReceivedBytes(receivedBytes, messageBuffer);

                if(receivedBytes == 0)
                {
                    throw new ConnectionException(
                        "Synchronous read completed with 0 bytes after having received an application header. Connection probably lost.",
                        Address, Port);
                }
            }

            SocketTransportTracing.Log.CompleteMessageReceived(messageBuffer.Length);
            messageType = header.MessageType;
            return new BinaryMessageReader(messageBuffer);
        }

        #endregion

        #region Finalizer Implementation

        /// <summary>
        /// Object finalizer.
        /// </summary>
        ~SocketTransport()
        {
            Dispose(false);
        }

        #endregion

        #region Disposable Implementation

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // The finalizer does not need to execute if the object has been disposed.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// </summary>
        /// <param name="disposing">
        /// Flag indicating if the object is being disposed. If true Dispose was called, if false the finalizer called
        /// this function. If the finalizer called the function, then only unmanaged resources should be released.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if(!Disposed && disposing)
            {
                // The auto reset event implements IDisposable, so there is no need to check if it converted.
                (connectResetEvent as IDisposable).Dispose();
                (transportExceptionResetEvent as IDisposable).Dispose();

                // The TcpClient class implements IDisposable, so there is no need to check if it was converted.
                (Client as IDisposable)?.Dispose();

                Disposed = true;
            }
        }

        #endregion
    }
}