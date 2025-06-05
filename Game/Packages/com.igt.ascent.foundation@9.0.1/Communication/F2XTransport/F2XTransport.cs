//-----------------------------------------------------------------------
// <copyright file = "F2XTransport.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Threading;
    using Transport;

    /// <summary>
    /// Implementation of the F2X transport which uses sockets.
    /// </summary>
    public class F2XTransport : IF2XTransport, IDisposable
    {
        #region Fields

        /// <summary>
        /// List of categories that are not removable during
        /// control level negotiations.
        /// </summary>
        private static readonly MessageCategory[] ConnectionLevelCategories =
            {
                MessageCategory.F2LConnect,
                MessageCategory.F2XConnect,
                MessageCategory.F2LLinkControl,
                MessageCategory.F2XLinkControl
            };

        /// <summary>
        /// List of category handlers installed in the transport.
        /// </summary>
        private readonly Dictionary<MessageCategory, IApiCategory> categoryHandlers;

        /// <summary>
        /// List of numbers to use when sending replies to the foundation on either the
        /// foundation channel or the foundation non-transactional channel.
        /// </summary>
        /// <remarks>
        /// Reply numbers should always be even when sent. They are intended for
        /// replies to application messages.
        /// </remarks>
        private readonly Dictionary<Channel, uint> replyNumbers;

        /// <summary>
        /// List of running requests numbers used for sending requests to the foundation.
        /// </summary>
        /// <remarks>Request numbers should always be odd when sent.</remarks>
        private readonly Dictionary<Channel, uint> requestNumbers;

        /// <summary>
        /// The base transport for communication.
        /// </summary>
        private readonly ITransport transport;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        protected bool Disposed;

        /// <summary>
        /// Mutex which grants access to the game channel.
        /// </summary>
        private readonly Mutex gameChannelLock = new Mutex(false);

        /// <summary>
        /// Mutex which grants access to the foundation channel.
        /// </summary>
        private readonly Mutex foundationChannelLock = new Mutex(false);

        /// <summary>
        /// Mutex which grants access to the foundation non-transactional channel.
        /// </summary>
        private readonly Mutex foundationNonTransactionalChannelLock = new Mutex(false);

        /// <summary>
        /// The flag indicating whether current transaction (always on FI channel) is lightweight.
        /// </summary>
        private volatile bool isLightweightTransaction;

        /// <summary>
        /// The most recent transaction id.
        /// </summary>
        private volatile uint transactionId;

        /// <summary>
        /// The UTF8NoBom encoding instance.
        /// </summary>
        private static volatile UTF8Encoding utf8NoBom;

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets the UTF8NoBom encoding instance with delay initialization.
        /// </summary>
        private static UTF8Encoding Utf8NoBom
        {
            get { return utf8NoBom ?? (utf8NoBom = new UTF8Encoding()); }
        }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Create an instance of the transport.
        /// </summary>
        /// <param name="transport">The base transport used by the F2L Transport.</param>
        /// <exception cref="ArgumentNullException">Thrown when the address is null.</exception>
        public F2XTransport(ITransport transport)
        {
            categoryHandlers = new Dictionary<MessageCategory, IApiCategory>();

            replyNumbers = new Dictionary<Channel, uint>
                               {
                                   { Channel.Foundation, 0 },
                                   { Channel.FoundationNonTransactional, 0 }
                               };

            requestNumbers = new Dictionary<Channel, uint>
                                 {
                                     { Channel.Foundation, 1 },
                                     { Channel.Game, 1 },
                                     { Channel.FoundationNonTransactional, 1 }
                                 };

            this.transport = transport;
            transport.SetMessageHandler(HandleMessage);
        }

        #endregion

        #region IF2XTransport Members

        /// <inheritdoc/>
        public uint TransactionId
        {
            private set { transactionId = value; }
            get { return transactionId; }
        }

        /// <inheritdoc/>
        public IExceptionMonitor TransportExceptionMonitor
        {
            get { return transport; }
        }

        /// <inheritdoc/>
        public void SendFoundationChannelResponse(string xml, MessageCategory category)
        {
            SendMessage(xml, category, Channel.Foundation, TransactionId, GetReplyNumber(Channel.Foundation));
        }

        /// <inheritdoc/>
        public void SendFoundationChannelRequest(string xml, MessageCategory category)
        {
            SendMessage(xml, category, Channel.Foundation, 0, IncrementRequestNumber(Channel.Foundation));
        }

        /// <inheritdoc/>
        public void SendGameChannelRequest(string xml, MessageCategory category)
        {
            SendMessage(xml, category, Channel.Game, 0, IncrementRequestNumber(Channel.Game));
        }

        /// <inheritdoc/>
        public void SendFoundationNonTransactionalChannelResponse(string xml, MessageCategory category)
        {
            // Send an application message using a zero transaction id.
            SendMessage(xml, category, Channel.FoundationNonTransactional, 0,
                        GetReplyNumber(Channel.FoundationNonTransactional));
        }

        /// <inheritdoc/>
        public void SendFoundationNonTransactionalChannelRequest(string xml, MessageCategory category)
        {
            SendMessage(xml, category, Channel.FoundationNonTransactional, 0,
                        IncrementRequestNumber(Channel.FoundationNonTransactional));
        }

        /// <inheritdoc/>
        public void AcquireChannel(Channel channel)
        {
            if(channel == Channel.Foundation)
            {
                foundationChannelLock.WaitOne();
            }
            else if(channel == Channel.Game)
            {
                gameChannelLock.WaitOne();
            }
            else if(channel == Channel.FoundationNonTransactional)
            {
                foundationNonTransactionalChannelLock.WaitOne();
            }
            else
            {
                throw new InvalidChannelException(channel);
            }
        }

        /// <inheritdoc/>
        public void ReleaseChannel(Channel channel)
        {
            if(channel == Channel.Foundation)
            {
                foundationChannelLock.ReleaseMutex();
            }
            else if(channel == Channel.Game)
            {
                gameChannelLock.ReleaseMutex();
            }
            else if(channel == Channel.FoundationNonTransactional)
            {
                foundationNonTransactionalChannelLock.ReleaseMutex();
            }
            else
            {
                throw new InvalidChannelException(channel);
            }
        }

        /// <inheritdoc/>
        public void InstallCategoryHandler(IApiCategory categoryHandler)
        {
            lock(categoryHandlers)
            {
                if(categoryHandler == null)
                {
                    throw new ArgumentNullException("categoryHandler", "Parameter may not be null.");
                }

                if(categoryHandlers.ContainsKey(categoryHandler.Category))
                {
                    throw new DuplicateCategoryException("The specified category is already installed.",
                                                         categoryHandler.GetType(),
                                                         categoryHandler.Category);
                }

                categoryHandlers[categoryHandler.Category] = categoryHandler;
            }
        }

        /// <inheritdoc/>
        public void UninstallControlLevelCategoryHandlers()
        {
            // Exclude categories that are not removable at control level negotiations.
            var removableCategories = Enum.GetValues(typeof(MessageCategory))
                                          .Cast<MessageCategory>()
                                          .Where(category => !ConnectionLevelCategories.Contains(category));
            UninstallControlLevelCategoryHandlers(removableCategories);
        }

        /// <inheritdoc/>
        public void UninstallControlLevelCategoryHandlers(IEnumerable<MessageCategory> categoriesToRemove)
        {
            lock(categoryHandlers)
            {
                foreach(var category in categoriesToRemove)
                {
                    if(ConnectionLevelCategories.Contains(category))
                    {
                        throw new InvalidOperationException(
                            string.Format("The {0} category cannot be uninstalled.", category));
                    }
                    categoryHandlers.Remove(category);
                }
            }
        }

        /// <inheritdoc/>
        public void Connect()
        {
            transport.Connect();
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            transport.Disconnect();
        }

        /// <inheritdoc/>
        public void SetLightweightTransaction()
        {
            isLightweightTransaction = true;
        }

        /// <inheritdoc/>
        public void ClearLightweightTransaction()
        {
            isLightweightTransaction = false;
        }

        /// <inheritdoc/>
        public void MustHaveHeavyweightTransaction()
        {
            if(isLightweightTransaction)
            {
                // Get the caller method name.
                var methodName = new StackFrame(1).GetMethod().Name;
                var message = string.Format("Method {0} needs a heavyweight transaction, but is called in a lightweight one.",
                                            methodName);

                throw new InvalidTransactionWeightException(message);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handle a transport message from the foundation.
        /// </summary>
        /// <param name="messageReader">The message reader for the message to handle.</param>
        /// <exception cref="InvalidMessageException">Thrown when the message type is invalid.</exception>
        private void HandleMessage(IBinaryMessageReader messageReader)
        {
            F2XTransportTracing.Log.ProcessHeaderStart();
            var applicationHeader = messageReader.Read<ApplicationHeaderSegment>();

            //Check to see if the message indicates any errors. If there are errors then do not attempt
            //to process the message.
            if(applicationHeader.StatusCode != 0)
            {
                F2XTransportTracing.Log.ProcessHeaderStop(false);
                throw new F2XTransportStatusException(applicationHeader.StatusCode);
            }

            // TODO: Should get the transaction id a different way.
            // Don't allow transaction id to be updated by a non transactional message.
            // TODO: (AS-6945) Once the SDK minimum foundation version is M series or greater, throw an exception instead.
            if(applicationHeader.TransactionIdentifier != 0 && applicationHeader.Channel == 1)
            {
                TransactionId = applicationHeader.TransactionIdentifier;
            }

            var reply = false;

            //If this is a request then set the reply number, otherwise keep the current reply number.
            //The message is a request if its message number is odd.
            if((applicationHeader.MessageNumber & 1) != 0)
            {
                UpdateReplyNumber((Channel)applicationHeader.Channel, applicationHeader.MessageNumber);
            }
            else
            {
                reply = true;
            }
            F2XTransportTracing.Log.ProcessHeaderStop(true);
            F2XTransportTracing.Log.HandleMessageStart(applicationHeader.MessageNumber,
                                                       (MessageCategory)applicationHeader.ApiCategory,
                                                       (Channel)applicationHeader.Channel,
                                                       applicationHeader.TransactionIdentifier,
                                                       messageReader.Length);
            HandleApplicationMessage(applicationHeader,
                                     messageReader.GetBytes(),
                                     messageReader.Position,
                                     (MessageCategory)applicationHeader.ApiCategory,
                                     reply,
                                     (Channel)applicationHeader.Channel);
            F2XTransportTracing.Log.HandleMessageStop(applicationHeader.MessageNumber, true);
        }

        /// <summary>
        /// Handle an application message.
        /// </summary>
        /// <param name="applicationHeader">The <see cref="ApplicationHeaderSegment"/> that was read for this message.</param>
        /// <param name="bytes">The content of the message.</param>
        /// <param name="startIndex">
        /// The index into the buffer which represents the start of the application message content.
        /// </param>
        /// <param name="category">The API category the message is intended for.</param>
        /// <param name="reply">Flag indicating if this message is a reply.</param>
        /// <param name="channel">Channel the message was sent on.</param>
        /// <exception cref="UnhandledCategoryException">
        /// Thrown when a message for an unknown category is received.
        /// </exception>
        private void HandleApplicationMessage(ApplicationHeaderSegment applicationHeader,
                                              byte[] bytes,
                                              int startIndex,
                                              MessageCategory category,
                                              bool reply,
                                              Channel channel)
        {
            //Translate the message into a string so it can be parsed.
            F2XTransportTracing.Log.DecodeMessageStart(category);
            var xml = Utf8NoBom.GetString(bytes, startIndex, bytes.Length - startIndex).TrimEnd('\0');
            F2XTransportTracing.Log.DecodeMessageStop(category, xml.Length);

            IApiCategory handler;
            lock(categoryHandlers)
            {
                if(!categoryHandlers.ContainsKey(category))
                {
                    F2XTransportTracing.Log.HandleMessageStop(applicationHeader.MessageNumber, false);
                    throw new UnhandledCategoryException("Message category not supported or not installed yet.", category);
                }

                handler = categoryHandlers[category];
            }

            var serializer = handler.MessageSerializer;
            using(var xmlReader = new StringReader(xml))
            {
                ICategory message;

                try
                {
                    F2XSerializationTracing.Log.DeserializeMessageStart(category, bytes.Length);
                    message = (ICategory)serializer.Deserialize(xmlReader);
                    F2XSerializationTracing.Log.DeserializeMessageStop(category, true);
                }
                catch(Exception exception)
                {
                    F2XSerializationTracing.Log.DeserializeMessageStop(category, false);
                    F2XTransportTracing.Log.HandleMessageStop(applicationHeader.MessageNumber, false);
                    throw new XmlMessageDeserializationException(
                        applicationHeader.MessageNumber,
                        category,
                        channel,
                        applicationHeader.TransactionIdentifier,
                        xml,
                        exception);
                }

                if(reply)
                {
                    handler.HandleReply(message, channel);
                }
                else
                {
                    handler.HandleMessage(message);
                }
            }
        }

        /// <summary>
        /// Send an application message to the foundation.
        /// </summary>
        /// <param name="xml">The xml content of the message.</param>
        /// <param name="category">The category of the message.</param>
        /// <param name="channel">Channel to send the message on.</param>
        /// <param name="transactionId">Transaction ID for the binary message header.</param>
        /// <param name="messageNumber">The message number.</param>
        private void SendMessage(string xml, MessageCategory category, Channel channel, uint transactionId,
                                 uint messageNumber)
        {
            F2XTransportTracing.Log.SendMessageStart(messageNumber, category, channel, transactionId);
            var message = BuildApplicationMessage(xml, transactionId, messageNumber, category, channel);
            transport.SendMessage(message);
            F2XTransportTracing.Log.SendMessageStop(messageNumber, message.Size);
        }

        /// <summary>
        /// Build an application message with the given payload.
        /// </summary>
        /// <param name="xml">The application message payload.</param>
        /// <param name="transactionId">Transaction id for the message.</param>
        /// <param name="messageNumber">Message number of the message.</param>
        /// <param name="category">Category of the message.</param>
        /// <param name="channel">Channel the message will be sent on.</param>
        /// <returns>Byte buffer containing the message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the passed xml payload is null.</exception>
        private static BinaryMessage BuildApplicationMessage(string xml, uint transactionId, uint messageNumber,
                                                             MessageCategory category,
                                                             Channel channel)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml", "Argument may not be null.");
            }

            F2XTransportTracing.Log.BuildMessageStart(messageNumber);
            var applicationHeader = new ApplicationHeaderSegment
                                        {
                                            TransactionIdentifier = transactionId,
                                            MessageNumber = messageNumber,
                                            ApiCategory = (uint)category,
                                            Channel = (byte)channel
                                        };
            var messageBuilder = new BinaryMessage(applicationHeader);

            //Encode the XML to bytes to put in the message and to determine the message length.
            F2XTransportTracing.Log.EncodeMessageStart(category);
            var xmlBytes = Utf8NoBom.GetBytes(xml);
            F2XTransportTracing.Log.EncodeMessageStop(category, xmlBytes.Length);
            messageBuilder.Append(new RawBinaryMessageSegment(xmlBytes));
            F2XTransportTracing.Log.BuildMessageStop(messageNumber, messageBuilder.Size);
            return messageBuilder;
        }

        /// <summary>
        /// Increments the request number to the next valid request number on the specified channel.
        /// </summary>
        /// <param name="channel">The channel specified.</param>
        /// <returns>The incremented request number on the specified channel.</returns>
        private uint IncrementRequestNumber(Channel channel)
        {
            uint requestNumber;
            lock(requestNumbers)
            {
                requestNumbers[channel] += 2;
                requestNumber = requestNumbers[channel];
            }
            return requestNumber;
        }

        /// <summary>
        /// Updates the reply number on the specified channel.
        /// </summary>
        /// <param name="channel">The channel specified.</param>
        /// <param name="messageNumber">The message number to increment the reply number by.</param>
        private void UpdateReplyNumber(Channel channel, uint messageNumber)
        {
            lock(replyNumbers)
            {
                replyNumbers[channel] = messageNumber + 1;
            }
        }

        /// <summary>
        /// Gets the reply number on the specified channel.
        /// </summary>
        /// <param name="channel">The channel specified.</param>
        /// <returns>The reply number on the specified channel.</returns>
        private uint GetReplyNumber(Channel channel)
        {
            uint replyNumber;
            lock(replyNumbers)
            {
                replyNumber = replyNumbers[channel];
            }
            return replyNumber;
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
                (gameChannelLock as IDisposable).Dispose();
                (foundationChannelLock as IDisposable).Dispose();
                (foundationNonTransactionalChannelLock as IDisposable).Dispose();
            }

            Disposed = true;
        }

        #endregion
    }
}