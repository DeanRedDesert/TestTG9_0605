//-----------------------------------------------------------------------
// <copyright file = "CategoryBase.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Xml.Serialization;
    using Threading;
    using Transport;

    /// <summary>
    /// Class which implements base functionality of all foundation supported categories.
    /// </summary>
    public abstract class CategoryBase<TCategory> : IApiCategory, IDisposable
        where TCategory : class, ICategory, new()
    {
        #region Fields

        /// <summary>
        /// Auto reset event used to wait for replies from the foundation on the foundation channel.
        /// </summary>
        private readonly AutoResetEvent foundationChannelReplyBlock = new AutoResetEvent(false);

        /// <summary>
        /// Auto reset event used to wait for replies from the foundation on the game channel.
        /// </summary>
        private readonly AutoResetEvent gameChannelReplyBlock = new AutoResetEvent(false);

        /// <summary>
        /// Auto reset event used to wait for replies from the foundation on the foundation non-transactional channel.
        /// </summary>
        private readonly AutoResetEvent foundationNonTransactionalReplyBlock = new AutoResetEvent(false);

        /// <summary>
        /// Reply received from the foundation on the foundation channel.
        /// </summary>
        private object foundationChannelReply;

        /// <summary>
        /// Reply received from the foundation on the game channel.
        /// </summary>
        private object gameChannelReply;

        /// <summary>
        /// Reply received from the foundation on the foundation non-transactional channel.
        /// </summary>
        private object foundationNonTransactionalChannelReply;

        /// <summary>
        /// Transport object that this category is installed in.
        /// </summary>
        protected IF2XTransport Transport { private set; get; }

        /// <summary>
        /// Table of message handlers for this Category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> handlerTable = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        protected bool Disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the category with the given transport.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        /// <exception cref="ArgumentNullException">Thrown when the transport is null.</exception>
        protected CategoryBase(IF2XTransport transport)
        {
            if(transport == null)
            {
                throw new ArgumentNullException("transport", "Argument may not be null.");
            }

            Transport = transport;
        }

        #endregion

        #region IApiCategory Implementation

        /// <inheritdoc />
        public abstract uint MajorVersion { get; }

        /// <inheritdoc />
        public abstract uint MinorVersion { get; }

        /// <inheritdoc />
        public XmlSerializer MessageSerializer { get; protected set; }

        /// <inheritdoc />
        public abstract MessageCategory Category { get; }

        /// <inheritdoc />
        public virtual void HandleReply(object message, Channel channel)
        {
            if(message == null)
            {
                throw new ArgumentNullException("message", "Argument may not be null.");
            }

            switch(channel)
            {
                case Channel.Foundation:
                    {
                        foundationChannelReply = message;
                        foundationChannelReplyBlock.Set();
                    }
                    break;

                case Channel.Game:
                    {
                        gameChannelReply = message;
                        gameChannelReplyBlock.Set();
                    }
                    break;

                case Channel.FoundationNonTransactional:
                    {
                        foundationNonTransactionalChannelReply = message;
                        foundationNonTransactionalReplyBlock.Set();
                    }
                    break;
            }
        }

        /// <inheritdoc />
        public void HandleMessage(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException("message", "Argument may not be null.");
            }

            var categoryMessage = message as TCategory;

            if(categoryMessage != null)
            {
                //In order to improve performance a check is not made to see if the handler contains the given key.
                //If it does not contain the key, then an exception will be thrown and can be handled.
                try
                {
                    handlerTable[categoryMessage.Message.Item.GetType()](categoryMessage.Message.Item);
                }
                catch(KeyNotFoundException)
                {
                    //The inner exception is not passed because it does not add to the exceptions meaning.
                    throw new InvalidMessageException(string.Format("Category: {0}. Invalid category message type: {1}",
                                                                    Category, categoryMessage.Message.Item.GetType()));
                }
            }
            else
            {
                throw new InvalidMessageException(string.Format("Category: {0} cannot handle message type: " +
                                                                Category, message.GetType()));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a reply on the game channel.
        /// </summary>
        /// <typeparam name="TReply">The type of the reply to get.</typeparam>
        /// <returns>The requested reply from the foundation.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when there is no reply message content.
        /// </exception>
        private TReply GetGameChannelReply<TReply>() where TReply : class
        {
            gameChannelReplyBlock.WaitOne(Transport.TransportExceptionMonitor);

            var replyMessage = (TCategory)gameChannelReply;
            gameChannelReply = null;

            var replyContent = replyMessage.Message.Item as TReply;

            if(replyContent == null)
            {
                throw new UnexpectedReplyTypeException("Unexpected reply.", typeof(TReply),
                                                       replyMessage.Message.Item.GetType());
            }

            return replyContent;
        }

        /// <summary>
        /// Get a reply on the foundation channel.
        /// </summary>
        /// <typeparam name="TReply">The type of the reply.</typeparam>
        /// <returns>The requested reply from the foundation.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when there is no reply message content.
        /// </exception>
        private TReply GetFoundationChannelReply<TReply>() where TReply : class
        {
            foundationChannelReplyBlock.WaitOne(Transport.TransportExceptionMonitor);

            var replyMessage = (TCategory)foundationChannelReply;
            foundationChannelReply = null;

            var replyContent = replyMessage.Message.Item as TReply;

            if(replyContent == null)
            {
                throw new UnexpectedReplyTypeException("Unexpected reply.", typeof(TReply),
                                                       replyMessage.Message.Item.GetType());
            }
            return replyContent;
        }

        /// <summary>
        /// Get a reply on the foundation non-transactional channel.
        /// </summary>
        /// <typeparam name="TReply">The type of the reply.</typeparam>
        /// <returns>The requested reply from the foundation.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when there is no reply message content.
        /// </exception>
        private TReply GetFoundationNonTransactionalChannelReply<TReply>() where TReply : class
        {
            foundationNonTransactionalReplyBlock.WaitOne(Transport.TransportExceptionMonitor);

            var replyMessage = (TCategory)foundationNonTransactionalChannelReply;
            foundationNonTransactionalChannelReply = null;

            var replyContent = replyMessage.Message.Item as TReply;

            if(replyContent == null)
            {
                throw new UnexpectedReplyTypeException("Unexpected reply.", typeof(TReply),
                                                       replyMessage.Message.Item.GetType());
            }
            return replyContent;
        }

        #endregion

        #region Protected Methods
        
        /// <summary>
        /// Creates an <see cref="XmlSerializer"/> for the current category by attempting to use the
        /// specified <see cref="XmlSerializerImplementation"/> if possible. Otherwise, the default
        /// <see cref="XmlSerializer"/> will be created.
        /// </summary>
        /// <param name="xmlSerializerContract">Class that can create <see cref="XmlSerializer"/> objects.</param>
        /// <returns>An <see cref="XmlSerializer"/> to use for messages.</returns>
        protected static XmlSerializer CreateMessageSerializer(XmlSerializerImplementation xmlSerializerContract)
        {
            var categoryType = typeof(TCategory);
            var xmlSerializer = xmlSerializerContract.CanSerialize(categoryType)
                                    ? xmlSerializerContract.GetSerializer(categoryType)
                                    : new XmlSerializer(categoryType);
            return xmlSerializer;
        }

        /// <summary>
        /// Install a handler for a message type.
        /// </summary>
        /// <param name="action">The action to perform for the message.</param>
        protected void AddMessagehandler<TMessage>(Action<TMessage> action) where TMessage : class
        {
            handlerTable[typeof(TMessage)] = message => action(message as TMessage);
        }


        /// <summary>
        /// Send a message on the specified channel and wait for a reply.
        /// </summary>
        /// <typeparam name="TReply">The type of the reply to get.</typeparam>
        /// <param name="channel">The channel to send the message on.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>The requested reply from the foundation.</returns>
        /// <exception cref="ArgumentException">Thrown when the channel is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the message is null.</exception>
        protected TReply SendMessageAndGetReply<TReply>(Channel channel, TCategory message)
            where TReply : class
        {
            if(message == null)
            {
                throw new ArgumentNullException("message", "Cannot send a null message over the F2L.");
            }

            var xmlMessage = TraceableXmlSerializer.GetXmlString(Category, message, MessageSerializer);

            TReply reply;
            Transport.AcquireChannel(channel);

            try
            {
                switch(channel)
                {
                    case Channel.Foundation:
                        Transport.SendFoundationChannelRequest(xmlMessage, Category);
                        reply = GetFoundationChannelReply<TReply>();
                        break;

                    case Channel.Game:
                        Transport.SendGameChannelRequest(xmlMessage, Category);
                        reply = GetGameChannelReply<TReply>();
                        break;

                    case Channel.FoundationNonTransactional:
                        Transport.SendFoundationNonTransactionalChannelRequest(xmlMessage, Category);
                        reply = GetFoundationNonTransactionalChannelReply<TReply>();
                        break;

                    default:

                        //Should not be possible under normal use, but may be encountered when updating the code
                        //to support a new channel.
                        throw new ArgumentException("Unsupported channel.");
                }

            }
            finally
            {
                Transport.ReleaseChannel(channel);
            }


            return reply;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Create a category message reply with the given reply code and reply message.
        /// </summary>
        /// <typeparam name="TReply">The type of the reply.</typeparam>
        /// <param name="replyCode">The reply code for the reply.</param>
        /// <param name="replyMessage">The message for the reply.</param>
        /// <returns>A category message containing the reply.</returns>
        protected abstract TCategory CreateReply<TReply>(int replyCode, string replyMessage) where TReply : new();

        /// <summary>
        /// Create a basic category message message request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <returns>A category message containing the request.</returns>
        protected abstract TCategory CreateBasicRequest<TRequest>() where TRequest : new();

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Send a response to the foundation.
        /// </summary>
        /// <param name="response">The response to send.</param>
        protected virtual void SendFoundationChannelResponse(TCategory response)
        {
            Transport.SendFoundationChannelResponse(
                TraceableXmlSerializer.GetXmlString(Category, response, MessageSerializer), Category);
        }

        /// <summary>
        /// Send a response to the foundation on the foundation non-transactional channel.
        /// </summary>
        /// <param name="response">The response to send.</param>
        protected virtual void SendFoundationNonTransactionalChannelResponse(TCategory response)
        {
            Transport.SendFoundationNonTransactionalChannelResponse(
                TraceableXmlSerializer.GetXmlString(Category, response, MessageSerializer), Category);
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Object finalizer.
        /// </summary>
        ~CategoryBase()
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
            //The finalizer does not need to execute if the object has been disposed.
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
                //The auto reset event implements IDisposable, so there is no need to check if it converted.
                (foundationChannelReplyBlock as IDisposable).Dispose();
                (gameChannelReplyBlock as IDisposable).Dispose();
                (foundationNonTransactionalReplyBlock as IDisposable).Dispose();
                Disposed = true;
            }
        }

        #endregion
    }
}
