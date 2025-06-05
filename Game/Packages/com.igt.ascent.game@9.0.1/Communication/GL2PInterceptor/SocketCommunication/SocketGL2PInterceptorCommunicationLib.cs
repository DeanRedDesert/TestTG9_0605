//-----------------------------------------------------------------------
// <copyright file = "SocketGL2PInterceptorCommunicationLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib.SocketCommunication
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Runtime.Serialization;

    /// <summary>
    /// Static library that contains functionality for sending and receiving
    /// data through a socket.
    /// </summary>
    internal static class SocketGL2PInterceptorCommunicationLib
    {
        /// <summary>
        /// Communication error conditions.
        /// </summary>
        public enum ErrorCondition
        {
            /// <summary>
            /// Okay status.
            /// </summary>
            Ok,

            /// <summary>
            /// Socket is not connected.
            /// </summary>
            SocketDisconnected,

            /// <summary>
            /// An operation has timed out.
            /// </summary>
            OperationTimedOut,

            /// <summary>
            /// Socket is null or has been disposed.
            /// </summary>
            SocketDisposed,

            /// <summary>
            /// A serialization issue has occurred.
            /// </summary>
            SerializationError,

            /// <summary>
            /// An unknown socket exception has occurred.
            /// </summary>
            UnknownException
        }

        /// <summary>
        /// Delegate used for specifying a callback when an error condition occurs on a socket.
        /// </summary>
        /// <param name="socket">Socket that encountered an error.</param>
        /// <param name="errorCondition">Error condition of the socket.</param>
        /// <param name="exception">Exception that triggered the error condition, may be null.</param>
        public delegate void SocketConnectionErrorHandler(Socket socket, ErrorCondition errorCondition, Exception exception);

        /// <summary>
        /// Delegate used for specifying a callback when a message is received.
        /// </summary>
        /// <param name="msgBuffer">Message buffer that contains the message that was received.</param>
        public delegate void MessageReceivedHandler(byte[] msgBuffer);

        /// <summary>
        /// Sends any object that can be serialized through an active socket connection.
        /// The message is formatted as follow: 4 Byte Message Length + object binary serialized.
        /// </summary>
        /// <param name="socket">Socket used to send the msgObject.</param>
        /// <param name="msgObject">Object to serialize and send through the socket.</param>
        /// <param name="timeout">Max time allowed to send the message in milliseconds.</param>
        /// <returns>errorCondition</returns>
        /// <exception cref="ArgumentNullException">Thrown if the msgObject parameter is null.</exception>
        public static ErrorCondition SendMessage(Socket socket, object msgObject, uint timeout)
        {
            if(msgObject == null)
            {
                throw new ArgumentNullException("msgObject", "msgObject parameter cannot be null.");
            }

            var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();

            try
            {
                binaryFormatter.Serialize(memoryStream, msgObject);
            }
            catch(SerializationException)
            {
                return ErrorCondition.SerializationError;
            }

            var byteBuffer = memoryStream.GetBuffer();
            return SendMessage(socket, byteBuffer, byteBuffer.Length, timeout);
        }

        /// <summary>
        /// Sends a byte buffer through an active socket. The message has the
        /// following format: 4 Byte Message Length + passed in byte buffer.
        /// </summary>
        /// <param name="socket">Socket used to send the byte buffer.</param>
        /// <param name="buffer">Byte buffer to send through the socket.</param>
        /// <param name="size">Number of bytes from the buffer to send.</param>
        /// <param name="timeout">Max time allowed to send the message in milliseconds.</param>
        /// <returns>errorCondition</returns>
        /// <exception cref= "ArgumentNullException">Thrown when the buffer parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when size parameter is equal or less than 0, or larger than the buffer length.
        /// </exception>
        public static ErrorCondition SendMessage(Socket socket, byte[] buffer, int size, uint timeout)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException("buffer", "buffer parameter cannot be null.");
            }

            if(size <= 0 || size > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("size",
                                                      "size parameter must be greater than 0 and less than the buffer length.");
            }

            byte[] reqLenArray = BitConverter.GetBytes(size);

            // send the length value
            var sendStatus = SendMessage(socket, reqLenArray, 0, 4, timeout);
            if(sendStatus != ErrorCondition.Ok)
            {
                return sendStatus;
            }

            // send the message buffer.
            return SendMessage(socket, buffer, 0, size, timeout);
        }

        /// <summary>
        /// Sends a byte buffer through an active socket.
        /// </summary>
        /// <param name="socket">Socket used to send the byte buffer.</param>
        /// <param name="buffer">Byte buffer to send through the socket.</param>
        /// <param name="offset">Offset of the byte buffer to start sending bytes.</param>
        /// <param name="size">Number of bytes from the buffer to send.</param>
        /// <param name="timeout">
        /// Max time allowed to send the message in milliseconds, 0 indicates timeout is ignored.</param>
        /// <returns>errorCondition</returns>
        private static ErrorCondition SendMessage(Socket socket, byte[] buffer, int offset, int size, uint timeout)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes are already sent
            do
            {
                if(timeout > 0 && Environment.TickCount - startTickCount > timeout)
                {
                    return ErrorCondition.OperationTimedOut;
                }

                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch(ObjectDisposedException)
                {
                    return ErrorCondition.SocketDisposed;
                }
                catch(SocketException ex)
                {
                    // socket buffer is probably full, wait and try again
                    if(ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        Thread.Sleep(30);
                    }
                    // If any other error occurs assume the socket has become disconnected.
                    else
                    {
                        return ErrorCondition.SocketDisconnected;
                    }
                }
                catch(NullReferenceException)
                {
                    return ErrorCondition.SocketDisposed;
                }
            } while(sent < size);

            return ErrorCondition.Ok;
        }

        #region Data Recieved State Object

        /// <summary>
        /// Custom Asynchronous State Object used for passing data in the
        /// BeginReceive function call.
        /// </summary>
        private class DataReceivedStateObject
        {
            /// <summary>
            /// Constructor for DataReceivedStateObject
            /// </summary>
            /// <param name="socket">Socket used to call BeginReceive.</param>
            /// <param name="messageReceivedHandler">Callback when a complete message has been received.</param>
            /// <param name="socketConnectionErrorEvent">Callback if a socket connection error is encountered.</param>
            /// <param name="timeOut">
            /// Max time allowed to receive a message in milliseconds, 0 indicates timeout is ignored.
            /// </param>
            public DataReceivedStateObject(Socket socket,
                                           MessageReceivedHandler messageReceivedHandler,
                                           SocketConnectionErrorHandler socketConnectionErrorEvent,
                                           uint timeOut)
            {
                Socket = socket;
                SocketConnectionErrorEvent = socketConnectionErrorEvent;
                MessageReceivedHandler = messageReceivedHandler;
                TimeOut = timeOut;
            }

            /// <summary>
            /// Socket used to call BeginReceive.
            /// </summary>
            public Socket Socket { get; private set; }

            /// <summary>
            /// 4 byte buffer that contains total length of the expected message.
            /// </summary>
            public byte[] ExpectedMsgSizeBuffer = new byte[4];

            /// <summary>
            /// Callback if a socket connection error is encountered.
            /// </summary>
            public SocketConnectionErrorHandler SocketConnectionErrorEvent { get; private set; }

            /// <summary>
            /// Callback when a complete message has been received.
            /// </summary>
            public MessageReceivedHandler MessageReceivedHandler { get; private set; }

            /// <summary>
            /// Max time allowed to receive a message in milliseconds, 0 indicates timeout is ignored.
            /// </summary>
            public uint TimeOut { get; private set; }
        }

        #endregion

        /// <summary>
        /// Begin asynchronously receiving messages.
        /// </summary>
        /// <param name="socket">Socket to receive message through.</param>
        /// <param name="messageReceivedEvent">Callback when a full message has been received.</param>
        /// <param name="socketConnectionErrorEvent">Callback if a socket connection error is encountered.</param>
        /// <param name="timeOut">
        /// Max time allowed to receive a message in milliseconds, 0 indicates timeout is ignored.
        /// </param>
        public static void AsyncReceiveMessages(Socket socket,
                                                MessageReceivedHandler messageReceivedEvent,
                                                SocketConnectionErrorHandler socketConnectionErrorEvent,
                                                uint timeOut)
        {
            var dataReceivedStateObject = new DataReceivedStateObject(socket, messageReceivedEvent, socketConnectionErrorEvent, timeOut);

            try
            {
                socket.BeginReceive(dataReceivedStateObject.ExpectedMsgSizeBuffer,
                                    0,
                                    dataReceivedStateObject.ExpectedMsgSizeBuffer.Length,
                                    SocketFlags.None,
                                    new AsyncCallback(OnBeginReceive),
                                    dataReceivedStateObject);
            }
            catch(SocketException exception)
            {
                //If a socket exception occurs disconnect from the client and start accepting new connections.
                var socketConnectionErrorDelegate = socketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisconnected, exception);
                }
            }
            catch(ObjectDisposedException exception)
            {
                var socketConnectionErrorDelegate = socketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisposed, exception);
                }
            }
            catch(NullReferenceException exception)
            {
                var socketConnectionErrorDelegate = socketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisposed, exception);
                }
            }
            catch(Exception exception)
            {
                var socketConnectionErrorDelegate = socketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.UnknownException, exception);
                }
            }
        }

        /// <summary>
        /// Asynchronous callback when a message is in the socket buffer waiting to be processed.
        /// </summary>
        /// <param name="asyn">DataReceivedStateObject</param>
        private static void OnBeginReceive(IAsyncResult asyn)
        {
            DataReceivedStateObject dataReceivedStateObject = (DataReceivedStateObject)asyn.AsyncState;
            Socket socket = dataReceivedStateObject.Socket;
            int bytesRecieved = 0;

            try
            {
                bytesRecieved = socket.EndReceive(asyn);
            }
            catch(SocketException exception)
            {
                //If a socket exception occurs disconnect from the client and start accepting new connections.
                var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisconnected, exception);
                }
                return;
            }
            catch(ObjectDisposedException exception)
            {
                var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisposed, exception);
                }
                return;
            }
            catch(NullReferenceException exception)
            {
                var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisposed, exception);
                }
                return;
            }
            catch(Exception exception)
            {
                var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.UnknownException, exception);
                }
                return;
            }

            // If 0 bytes have been received then that should mean the socket is closing.
            if(bytesRecieved == 0)
            {
                var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                if(socketConnectionErrorDelegate != null)
                {
                    socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisconnected, null);
                }
                return;
            }

            var messageSize = BitConverter.ToInt32(dataReceivedStateObject.ExpectedMsgSizeBuffer, 0);
            var dataBuffer = new byte[messageSize];
            var received = 0;  // how many bytes is already received
            var startTickCount = Environment.TickCount;

            do
            {
                if(Environment.TickCount - startTickCount > dataReceivedStateObject.TimeOut)
                {
                    var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                    if(socketConnectionErrorDelegate != null)
                    {
                        socketConnectionErrorDelegate(socket, ErrorCondition.OperationTimedOut, null);
                    }
                    return;
                }

                try
                {
                    int newBytesReceived = socket.Receive(dataBuffer, received, dataBuffer.Length - received, SocketFlags.None);
                    //If 0 bytes are received then close the socket.
                    if(newBytesReceived == 0)
                    {
                        var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                        if(socketConnectionErrorDelegate != null)
                        {
                            socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisconnected, null);
                        }
                        return;
                    }

                    received += newBytesReceived;
                }

                catch(SocketException exception)
                {
                    if(exception.SocketErrorCode == SocketError.WouldBlock ||
                        exception.SocketErrorCode == SocketError.IOPending ||
                        exception.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                    {
                        var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                        if(socketConnectionErrorDelegate != null)
                        {
                            socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisconnected, exception);
                        }
                        return;
                    }
                }
                catch(ObjectDisposedException exception)
                {
                    var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                    if(socketConnectionErrorDelegate != null)
                    {
                        socketConnectionErrorDelegate(socket, ErrorCondition.SocketDisposed, exception);
                    }
                }
                catch(Exception exception)
                {
                    var socketConnectionErrorDelegate = dataReceivedStateObject.SocketConnectionErrorEvent;
                    if(socketConnectionErrorDelegate != null)
                    {
                        socketConnectionErrorDelegate(socket, ErrorCondition.UnknownException, exception);
                    }
                }
            } while(received < messageSize);

            var messageReceivedDelegate = dataReceivedStateObject.MessageReceivedHandler;
            if(messageReceivedDelegate != null)
            {
                messageReceivedDelegate(dataBuffer);
            }

            AsyncReceiveMessages(socket,
                                 dataReceivedStateObject.MessageReceivedHandler,
                                 dataReceivedStateObject.SocketConnectionErrorEvent,
                                 dataReceivedStateObject.TimeOut);
        }
    }
}
