// -----------------------------------------------------------------------
// <copyright file = "WebSocketConnection.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// WebSocketConnection()
    /// This does the socket communications once the handshake has been accomplished.
    /// It uses WebSocketProtocol to ensure data sent/received conforms to the WebSocket protocol.
    /// </summary>
    public sealed class WebSocketConnection : IUtpCommunication, IDisposable
    {
        #region Private Members

        /// <summary>
        /// Byte string data buffer.
        /// </summary>
        private readonly byte[] dataBuffer;

        /// <summary>
        /// Web socket protocol.
        /// </summary>
        private readonly WebSocketProtocol protocol;

        /// <summary>
        /// Web socket buffer.
        /// </summary>
        private readonly WebSocketBuffer buffer;

        /// <summary>
        /// Data send complete event.
        /// </summary>
        private event DataSendDoneDelegate SendDone;

        /// <summary>
        /// Residual length.
        /// </summary>
        private Int32 residualLength;

        /// <summary>
        /// Received bytes list.
        /// </summary>
        private List<byte> receivedBytes;

        /// <summary>
        /// Disconnect sent flag.
        /// </summary>
        private bool disconnectSent;

        /// <summary>
        /// Locket object.
        /// </summary>
        private readonly Object locket;

        /// <summary>
        /// Communication error delegate.
        /// </summary>
        private readonly CommunicationErrorDelegate errorDelegate;

        /// <summary>
        /// Amount of time in seconds to let local socket connection to sleep.
        /// </summary>
        private const int localSocketConnectedSleepValue = 4;

        #endregion Private Members

        #region public fields

        /// <summary>
        /// Buffer capacity value.
        /// </summary>
        public const Int32 BufferCapacity = 1400;

        /// <summary>
        /// Disconnected delegate.
        /// </summary>
        public event DisconnectedDelegate Disconnected;

        /// <summary>
        /// Communication error delegate
        /// </summary>
        /// <param name="hash">Int32 hash.</param>
        public delegate void CommunicationErrorDelegate(Int32 hash);

        /// <summary>
        /// Communication connection socket.
        /// </summary>
        public Socket ConnectionSocket { get; private set; }

        #endregion

        #region delegates

        /// <summary>
        /// Data send complete delegate.
        /// </summary>
        public delegate void DataSendDoneDelegate();

        /// <summary>
        /// Connection disconnected delegate.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        public delegate void DisconnectedDelegate(WebSocketConnection sender, EventArgs args);

        #endregion

        #region Constructors

        /// <summary>
        /// WebSocketConnection()
        /// Created once a WebSocket handshake has been successfully negotiated.
        /// </summary>
        /// <param name="lib">Web socket lib.</param>
        /// <param name="socket">The socket to use for this connection.</param>
        /// <param name="protocol">Web socket protocol.</param>
        public WebSocketConnection(WebSocketLib lib, Socket socket, WebSocketProtocol protocol)
            : this(lib, socket, protocol, BufferCapacity)
        {
        }

        /// <summary>
        /// WebSocketConnection() attempts to establish a connection to the client.
        /// </summary>
        /// <param name="lib">WebSocketLib.</param>
        /// <param name="socket">The socket to use for this connection.</param>
        /// <param name="protocol">Protocol in use.</param>
        /// <param name="bufferSize">The size of the receive buffer.</param>
        public WebSocketConnection(WebSocketLib lib, Socket socket, WebSocketProtocol protocol, Int32 bufferSize)
        {
            if(lib == null)
            {
                throw new ArgumentNullException("lib");
            }
            if(socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if(protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            errorDelegate = lib.CommunicationError;

            locket = new Object();

            disconnectSent = false;

            ConnectionSocket = socket;

            this.protocol = protocol;

            protocol.BufferSize = bufferSize;

            dataBuffer = new byte[bufferSize];

            receivedBytes = new List<byte>();

            residualLength = 0;

            buffer = new WebSocketBuffer(lib, this);

            SendDone += buffer.OnSend;

            protocol.Initialize(this, buffer);

            buffer.Initialize(protocol);

            Int32 hash = GetHashCode();

            Receive();
        }

        #endregion // Constructors

        #region Receive

        /// <summary>
        /// Receive() listens for client data received.
        /// </summary>
        private void Receive()
        {
            try
            {
                if(ConnectionSocket != null && ConnectionSocket.Connected)
                {
                    ConnectionSocket.BeginReceive(dataBuffer, 0, BufferCapacity, 0, ReceiveCallback, ConnectionSocket);
                }
            }
            catch
            {
                //  Nothing to do
            }
        }

        /// <summary>
        /// ReceiveCallback() The client sent some data to the server (us).
        /// </summary>
        /// <param name="asyncResult">Contains the socket </param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                var localSocket = (Socket)asyncResult.AsyncState;
                if(localSocket.Connected)
                {
                    Thread.Sleep(localSocketConnectedSleepValue);
                    // Filter the data through the protocol.
                    Receive(localSocket.EndReceive(asyncResult), dataBuffer);
                }
            }
            catch(SocketException)
            {
                if(errorDelegate != null)
                {
                    errorDelegate(GetHashCode());
                }
            }
            catch
            {
                //  Nothing to do
            }

            // Return to listening.
            Receive();
        }

        /// <summary>
        /// Receive data. Simple eh? We are trying to receive a WebSocket protocol worth of data at a time. 
        /// However the sender might send several WebSocket chunks, which are received together. 
        /// The behind the scenes socket buffering will properly order the data but will concatenate the 
        /// WebSocket chunks in the receive buffer. Our task is to identify concatenated data, and separate 
        /// them before sending them on to the protocol.
        /// </summary>
        /// <param name="receiveSize">Receive size in bytes.</param>
        /// <param name="receiveBuffer">Receive buffer.</param>
        public void Receive(Int32 receiveSize, byte[] receiveBuffer)
        {
            if(receiveSize > 0)
            {
                lock(locket)
                {
                    Int64 someLength;
                    // Add any new data to the data in _receivedBytes, if any.
                    receivedBytes.AddRange(receiveBuffer);

                    do
                    {
                        // Redimension the size of current.
                        var current = new byte[receivedBytes.Count];
                        // Copy the bytes to current.
                        receivedBytes.CopyTo(current);

                        // Get the protocol defined length of the data in current.
                        someLength = protocol.ParseDataBuffer(current);

                        // Adjust receiveSize to reflect any residual data that was waiting for more new data.
                        receiveSize += residualLength;

                        // if someLength is larger than (receiveSize + _residualLength), it means there is more data to come.
                        if((Int32)someLength > receiveSize)
                        {
                            // So the residual length should match what is in the carryover buffer.
                            residualLength = receivedBytes.Count;
                            // Return without further processing.
                            return;
                        }

                        // Redimension current again to the size of a transmission.
                        current = new byte[(Int32)someLength];
                        // Copy the transmission into current.
                        receivedBytes.CopyTo(0, current, 0, (Int32)someLength);

                        // Check someLength against receiveSize, or (_residualLength + receiveSize) against someLength.
                        // If they are equal then a complete data packet has been received.
                        if((Int32)someLength == receiveSize)
                        {
                            // If they are the same then refresh _receivedBytes.
                            receivedBytes = new List<byte>();
                            residualLength = 0;
                            receiveSize = 0;
                        }
                        else
                        {
                            // If they someLength is less than _residualLength, then grab the stuff after the first transmission,
                            // and push it to the beginning of _receivedBytes
                            // Dimension residual to receive the stuff after the first transmission.
                            var residual = new byte[receiveSize - (Int32)someLength];
                            // Copy the stuff after to residual.
                            receivedBytes.CopyTo((Int32)someLength, residual, 0, (receiveSize - (Int32)someLength));
                            // Clear out _receivedBytes.
                            receivedBytes = new List<byte>();
                            // Add back the end stuff to become the new front stuff.
                            receivedBytes.AddRange(residual);
                            residualLength = (receiveSize - (Int32)someLength);
                            receiveSize = 0;
                        }

                        protocol.UnmaskPayload(current, protocol.ProtocolParseData.Mask);
                        protocol.DataReceived(current, protocol.ProtocolParseData);

                    } while(residualLength >= (Int32)someLength);
                }
            }
        }

        #endregion Receive

        #region Send

        /// <summary>
        /// Send() bypasses the WebSocketLib Send().
        /// </summary>
        /// <param name="command">The Automation command to send.</param>
        /// <returns>True if the command has been queued for sending.</returns>
        public bool Send(AutomationCommand command)
        {
            if(command == null)
            {
                throw new ArgumentNullException("command");
            }

            // Serialize the automation command into a string.
            var sCommand = AutomationCommand.Serialize(command);
            // Make sure the string is valid. 
            if(!string.IsNullOrEmpty(sCommand))
            {
                // Queue up the string version of the AutomationCommand.
                OnSend(sCommand);
                // Indicate dispatch occured.
                return true;
            }
            // Indicate failed dispatch.
            return false;
        }

        /// <summary>
        /// Send()
        /// This takes the data to send and wraps it up in a WebSocket protocol envelope.
        /// </summary>
        /// <param name="payload">A string to send. Gets converted into an array of bytes.</param>
        public void Send(string payload)
        {
            if(string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException("payload");
            }

            // Wrap up the payload.
            byte[] byteData = protocol.Send(payload, true, 0x01);
            Send(byteData);
        }

        /// <summary>
        /// Send()
        /// Send an array of bytes over a socket.
        /// </summary>
        /// <param name="byteData">The array of bytes to send.</param>
        public void Send(byte[] byteData)
        {
            if(byteData == null)
            {
                throw new ArgumentNullException("byteData");
            }
            
            var state = new StateObjectEventArgs(ConnectionSocket, byteData.Length, "null");
            try
            {
                ConnectionSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, state);
            }
            catch
            {
                //  Nothing to do
            }
        }

        /// <summary>
        /// SendCallback() called when the data has been sent.
        /// If all of the data has been sent, then raise sendDone.
        /// </summary>
        /// <param name="asyncResult">IAsyncResult which is the StateObjectEventArgs.</param>
        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Retrieve the socket from the state object.
                var state = (StateObjectEventArgs)asyncResult.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = state.SocketX.EndSend(asyncResult);
                if(state.Size == bytesSent)
                {
                    // Tell the buffer to dequeue data to send.
                    if(SendDone != null)
                    {
                        SendDone();
                    }
                }
            }
            catch
            {
                //  Nothing to do
            }
        }

        /// <summary>
        /// OnSend() takes a serialized AutomationCommand (string), and buffers it for sending.
        /// </summary>
        /// <param name="serializedAutomationCommand">The serialized AutomationCommand to send.</param>
        public void OnSend(string serializedAutomationCommand)
        {
            if(string.IsNullOrEmpty(serializedAutomationCommand))
            {
                throw new ArgumentNullException("serializedAutomationCommand");
            }

            buffer.Send(serializedAutomationCommand);
        }

        #endregion Send

        #region Disconnect

        /// <summary>
        /// Disconnect() causes a WebSocket protocol disconnect to be sent to the connected machine.
        /// </summary>
        public void Disconnect()
        {
            if(disconnectSent != true)
            {
                Send(protocol.Disconnect());
                disconnectSent = true;
                if(ConnectionSocket.IsBound)
                {
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                }
            }
            if(Disconnected != null)
            {
                Disconnected(this, null);
            }
        }

        #endregion Disconnect

        /// <summary>
        /// Close() cleans up the socket connection.
        /// </summary>
        public void Close()
        {
            if(ConnectionSocket.IsBound)
            {
                ConnectionSocket.Shutdown(SocketShutdown.Both);
            }
            if(ConnectionSocket.Connected)
            {
                ConnectionSocket.Disconnect(false);
            }
            disconnectSent = false;
        }

        public void Dispose()
        {
            Close();
        }
    }
}