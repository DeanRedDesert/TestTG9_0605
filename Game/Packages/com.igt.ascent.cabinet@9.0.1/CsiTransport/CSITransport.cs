//-----------------------------------------------------------------------
// <copyright file = "CsiTransport.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using CSI.Schemas.Internal;
    using CSI.Schemas.Serializers;
    using Foundation.Transport;
    using Threading;
    using CsiMessageType = CSI.Schemas.Internal.MessageType;

    /// <summary>
    /// Manages the communication between the game and the CSI Manager.
    /// </summary>
    public class CsiTransport : ICsiTransport, IDisposable
    {
        #region Fields

        /// <summary>
        /// Dictionary containing categories which are handled by the transport.
        /// </summary>
        private readonly Dictionary<Category, ICabinetCategory> categoryHandlers =
            new Dictionary<Category, ICabinetCategory>();

        /// <summary>
        /// Serializer for the outer xml of a message.
        /// </summary>
        private readonly XmlSerializer messageSerializer;

        /// <summary>
        /// Underlying transport used by the CsiTransport.
        /// </summary>
        private readonly ITransport transport;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Class which manages category negotiation.
        /// </summary>
        private IConnectHandler connectHandler;

        /// <summary>
        /// The current request number for the client.
        /// </summary>
        private ulong clientRequestNumber;

        /// <summary>
        /// Object used for locking application message handling.
        /// </summary>
        private readonly object applicationHandlerLock = new object();

        /// <summary>
        /// The UTF8NoBom encoding instance.
        /// </summary>
        private static volatile UTF8Encoding utf8NoBom;

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets the UTF8NoBom encoding instance with delayed initialization.
        /// </summary>
        private static UTF8Encoding Utf8NoBom => utf8NoBom ?? (utf8NoBom = new UTF8Encoding());

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of a <see cref="CsiTransport"/>.
        /// </summary>
        /// <param name="transport">The transport to use for the CSI connection.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="transport"/> is null.</exception>
        public CsiTransport(ITransport transport)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport), "Parameter may not be null.");
            messageSerializer = XmlSerializerContract.Instance.CanSerialize(typeof(Csi))
                ? XmlSerializerContract.Instance.GetSerializer(typeof(Csi))
                : new XmlSerializer(typeof(Csi));            
        }

        #endregion

        #region Implement ICsiTransport

        /// <inheritdoc />
        public IExceptionMonitor TransportExceptionMonitor => transport;

        /// <inheritdoc />
        public void Connect()
        {
            if(connectHandler == null)
            {
                throw new InvalidOperationException("CsiTransport cannot be connected without a ConnectHandler.");
            }

            transport.SetMessageHandler(HandleMessage);
            transport.Connect();

            // Initial request and install category handlers.
            connectHandler.Initialize(this);
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if(connectHandler == null)
            {
                throw new InvalidOperationException("CsiTransport cannot be disconnected without a ConnectHandler.");
            }

            // Let the transport know that the socket may be closed first from the server side as a result
            // of the connect handler de-initializing.
            transport.PrepareToDisconnect();
            connectHandler.Deinitialize(this);
            transport.Disconnect();
            transport.SetMessageHandler(null);
            UninstallCategoryHandlers();
        }

        /// <inheritdoc />
        public void SendResponse(Csi response, ulong csiManagerRequestNumber)
        {
            if(response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            response.MessageId = csiManagerRequestNumber;
            SendMessage(response);
        }

        /// <inheritdoc />
        public void SendRequest(Csi request)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            request.MessageId = clientRequestNumber;
            clientRequestNumber++;
            SendMessage(request);
        }

        /// <inheritdoc />
        public void SendMessage(string xmlMessage)
        {
            if(xmlMessage == null)
            {
                throw new ArgumentNullException(nameof(xmlMessage), "Parameter may not be null.");
            }

            CsiTransportTracing.Log.EncodeMessageStart();
            //Encode the XML to bytes to put in the message and to determine the message length.
            var xmlBytes = Utf8NoBom.GetBytes(xmlMessage);
            CsiTransportTracing.Log.EncodeMessageStop(xmlBytes.Length);

            // create a builder to hold the message
            var messageBuilder = new BinaryMessage(new RawBinaryMessageSegment(xmlBytes));

            // send the message
            transport.SendMessage(messageBuilder);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Install a CSI category handler.
        /// </summary>
        /// <param name="category">The callback interface to handle incoming messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when the category parameter is null.</exception>
        /// <exception cref="DuplicateCategoryException">
        /// Thrown when there is already a handler installed for the specified category.
        /// </exception>
        public void InstallCategory(ICabinetCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category), "Parameter may not be null.");
            }
            lock(categoryHandlers)
            {
                if(categoryHandlers.ContainsKey(category.Category))
                {
                    throw new DuplicateCategoryException(
                        "Multiple handlers may not be installed for a single category.",
                        category.GetType(), category.Category);
                }
                category.InstallTransport(this);
                categoryHandlers[category.Category] = category;
            }
        }

        /// <summary>Install a connect handler used for negotiating categories.</summary>
        /// <param name="newConnectHandler">Handler which manages the initial connection and negotiates categories.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="newConnectHandler"/> is null.</exception>
        public void InstallConnectHandler(IConnectHandler newConnectHandler)
        {
            connectHandler = newConnectHandler ?? throw new ArgumentNullException(nameof(newConnectHandler));
        }

        /// <summary>
        /// Uninstall all category handlers.
        /// This should be called when the CSI is disconnected.  Otherwise, we'll get
        /// <see cref="DuplicateCategoryException"/> upon re-connection.
        /// </summary>
        private void UninstallCategoryHandlers()
        {
            lock(categoryHandlers)
            {
                categoryHandlers.Clear();
            }
        }

        /// <summary>
        /// Handle a transport message from the CSI manager.
        /// </summary>
        /// <param name="messageReader">A reader containing the complete message.</param>
        /// <remarks>
        /// CA1031:DoNotCatchGeneralExceptionTypes is suppressed because this method propagates the exception to
        /// another thread.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void HandleMessage(IBinaryMessageReader messageReader)
        {
            lock(applicationHandlerLock)
            {
                CsiTransportTracing.Log.DecodeMessageStart();
                var xml = Utf8NoBom.GetString(messageReader.GetBytes(), 0, messageReader.Length).TrimEnd('\0');
                CsiTransportTracing.Log.DecodeMessageStop(messageReader.Length);
                
                using(var outerXmlReader = new StringReader(xml))
                {
                    XmlReader reader = new XmlTextReader(outerXmlReader);
                    if(messageSerializer.CanDeserialize(reader))
                    {
                        Csi message;
                        try
                        {
                            CsiSerializationTracing.Log.DeserializeCsiContainerStart(xml.Length);
                            message = (Csi)messageSerializer.Deserialize(reader);
                            CsiSerializationTracing.Log.DeserializeCsiContainerStop(message.Category);
                        }
                        catch(Exception exception)
                        {

                            throw new CsiMessageDeserializationException(xml, exception);
                        }

                        CsiTransportTracing.Log.HandleMessageStart(message.MessageId, message.Category);
                        HandleCsiMessage(message, message.MessageId);
                        CsiTransportTracing.Log.HandleMessageStop(message.MessageId);
                    }
                    else if(connectHandler.MessageSerializer.CanDeserialize(reader))
                    {
                        CsiSerializationTracing.Log.DeserializeCsiConnectStart(xml.Length);
                        var message = (CsiConnect)connectHandler.MessageSerializer.Deserialize(reader);
                        CsiSerializationTracing.Log.DeserializeCsiConnectStop();
                        CsiTransportTracing.Log.HandleConnectResponseStart();
                        connectHandler.HandleMessage(message);
                        CsiTransportTracing.Log.HandleConnectResponseStop();
                    }
                    else
                    {
                        throw new InvalidMessageException(
                            "Message received by CsiTransport was neither a CSI or CsiConnect message.");
                    }
                }
            }
        }

        /// <summary>
        /// Handle messages which are of the CSI type.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="requestId">ID for the request if the message is a request.</param>
        private void HandleCsiMessage(Csi message, ulong requestId)
        {
            using(var xmlReader = new XmlNodeReader(message.CsiBody))
            {
                ICabinetCategory categoryHandler;
                try
                {
                    lock(categoryHandlers)
                    {
                        categoryHandler = categoryHandlers[message.Category];
                    }
                }
                catch(KeyNotFoundException exception)
                {
                    var exceptionHeaderMessage =
                        $"Category handler not found, categoryHandlers count == {categoryHandlers.Count}. ";

                    var innerException = new KeyNotFoundException(exceptionHeaderMessage, exception);
                    throw new UnhandledCsiMessageCategoryException(message, innerException);
                }

                object categoryMessage;

                try
                {
                    CsiSerializationTracing.Log.DeserializeInnerMessageStart(message.Category);
                    categoryMessage = categoryHandler.MessageSerializer.Deserialize(xmlReader);
                    CsiSerializationTracing.Log.DeserializeInnerMessageStop(message.Category);
                }
                catch(Exception exception)
                {
                    throw new CsiMessageDeserializationException(message.CsiBody.InnerText, exception);
                }

                switch(message.MessageType)
                {
                    case CsiMessageType.Event:
                        categoryHandler.HandleEvent(categoryMessage);
                        break;
                    case CsiMessageType.Request:
                        categoryHandler.HandleRequest(categoryMessage, requestId);
                        break;
                    case CsiMessageType.Response:
                        categoryHandler.HandleResponse(categoryMessage);
                        break;
                    default:
                        throw new InvalidMessageException("The specified type of the CSI message cannot be handled.");
                }
            }
        }

        /// <summary>
        /// Send the given CSI message.
        /// </summary>
        /// <param name="message">The CSI message to send.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="message"/> parameter is null.</exception>
        private void SendMessage(Csi message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            CsiTransportTracing.Log.SendMessageStart(message.MessageId, message.Category);
            var xmlMessage = TraceableXmlSerializer.GetXmlString(message, messageSerializer);
            SendMessage(xmlMessage);
            CsiTransportTracing.Log.SendMessageStop(message.MessageId, xmlMessage.Length);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// </summary>
        /// <param name="disposing">
        /// Flag indicating if the object is being disposed. If true Dispose was called, if false the finalizer called
        /// this function. If the finalizer called the function, then only unmanaged resources should be released.
        /// </param>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                if(transport is IDisposable disposableTransport)
                {
                    disposableTransport.Dispose();
                }

                // Always check the connect handler for IDisposable, even
                // when sometimes it does not implement the interface.
                // This is to cover future expansions to the code base.
                // ReSharper disable once SuspiciousTypeConversion.Global
                if(connectHandler is IDisposable disposableConnectHandler)
                {
                    disposableConnectHandler.Dispose();
                }

                lock(categoryHandlers)
                {
                    foreach(var categoryHandler in categoryHandlers)
                    {
                        if(categoryHandler.Value is IDisposable disposableCategory)
                        {
                            disposableCategory.Dispose();
                        }
                    }
                }
                disposed = true;
            }
        }

        #endregion
    }
}