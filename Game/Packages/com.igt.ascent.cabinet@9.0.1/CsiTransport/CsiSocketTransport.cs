//-----------------------------------------------------------------------
// <copyright file = "CsiSocketTransport.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using System.Collections.Generic;
    using Foundation.Transport;

    /// <summary>
    /// Class for handling CSI messages from the socket.
    /// </summary>
    public class CsiSocketTransport : SocketTransport
    {
        /// <summary>
        /// Queue to contain all the CSI messages from foundation in the received order.
        /// </summary>
        private readonly Queue<IBinaryMessageReader> incomingMessageQueue = new Queue<IBinaryMessageReader>();

        /// <summary>
        /// Create an instance of the CSI socket transport.
        /// </summary>
        /// <param name="address">The IP address to use to connect to the foundation.</param>
        /// <param name="port">The port to connect on.</param>
        public CsiSocketTransport(string address, ushort port) : base(address, port)
        {
        }

        ///<inheritdoc/>
        /// <remarks>
        /// This function enqueues the messages from foundation to avoid race conditions.
        /// This had to be done because CSI events don't wait for any acknowledgments from the client.
        /// </remarks>
        /// <exception cref="InvalidMessageException">
        /// Thrown when the message type in the header is invalid or an application message is not of a known type.
        /// </exception>
        protected override void ProcessReceive(IAsyncResult asyncResult)
        {
            try
            {
                // End the current asynchronous read.
                EndReceive(asyncResult);

                // Do nothing if in the process of disconnecting.
                if(IsDisconnecting)
                {
                    return;
                }

                var messageReader = ReceiveCompleteMessage(out var messageType);

                switch(messageType)
                {
                    case MessageType.Transport:
                        var body = messageReader.Read<TransportBodyHeaderSegment>();

                        if(body.BodyType == BodyType.ConnectionAccepted)
                        {
                            //The connection has been accepted.
                        }
                        break;
                    case MessageType.Application:
                        lock(incomingMessageQueue)
                        {
                            // Check to see if disconnect happened while waiting to acquire the lock.
                            if(IsDisconnecting)
                            {
                                return;
                            }

                            incomingMessageQueue.Enqueue(messageReader);
                        }
                        break;
                    default:
                        throw new InvalidMessageException("Invalid message type: " + messageType);
                }

                // Allow receive while processing message.
                StartReceive();

                // Process any messages that have been queued up.
                ProcessQueuedMessages();
            }
            // If we are currently disconnecting then ignore all exceptions.
            catch(Exception)
            {
                if(!IsDisconnecting)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Process queued up messages. The locking thread will process the entire queue, in case the queueing thread
        /// ends before processing it (during a socket disconnection or related.)
        /// </summary>
        private void ProcessQueuedMessages()
        {
            lock(incomingMessageQueue)
            {
                while(incomingMessageQueue.Count > 0)
                {
                    var messageReader = incomingMessageQueue.Dequeue();

                    var tempHandler = HandleMessage;
                    tempHandler?.Invoke(messageReader);
                }
            }
        }

        #region ITransport Overrides

        /// <inheritdoc />
        public override void Connect()
        {
            IsDisconnecting = false;
            base.Connect();
        }

        /// <inheritdoc />
        public override void Disconnect()
        {
            IsDisconnecting = true;

            // Clear any queued messages.
            lock(incomingMessageQueue)
            {
                incomingMessageQueue.Clear();
            }

            // Close the lower level socket.
            base.Disconnect();
        }

        #endregion

    }
}