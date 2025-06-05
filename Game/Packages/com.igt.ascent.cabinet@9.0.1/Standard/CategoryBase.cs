//-----------------------------------------------------------------------
// <copyright file = "CategoryBase.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;
    using CsiTransport;
    using CSI.Schemas.Internal;
    using CSI.Schemas.Serializers;
    using Foundation.Transport;
    using Threading;
    using CsiMessageType = CSI.Schemas.Internal.MessageType;

    /// <summary>
    /// Base class to use for cabinet categories. Handles functionality common to the categories.
    /// </summary>
    /// <typeparam name="TCategory">The message type associated with this category.</typeparam>
    public abstract class CategoryBase<TCategory> : ICabinetCategory, IDisposable where TCategory : class
    {
        #region Private Fields

        /// <summary>
        /// Reset event which is used to block when waiting for a response.
        /// </summary>
        private readonly AutoResetEvent responseBlock = new AutoResetEvent(false);

        /// <summary>
        /// Response from the CSI Manager.
        /// </summary>
        private TCategory response;

        /// <summary>
        /// Transport the category uses for communication.
        /// </summary>
        private ICsiTransport transport;

        /// <summary>
        /// Flag which indicates if this object has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Xml serializer for the category messages.
        /// </summary>
        private XmlSerializer messageSerializer;

        #endregion Private Fields

        /// <summary>
        /// Transport the category uses to communicate.
        /// </summary>
        protected ICsiTransport Transport
        {
            private set => transport = value;
            get
            {
                if(transport == null)
                {
                    throw new InvalidOperationException("Category cannot be used if not installed.");
                }
                return transport;
            }
        }

        /// <summary>
        /// Get a reply from the CSI manager.
        /// </summary>
        /// <remarks>This is not a property because it does not behave as would be expected from a property.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected TCategory GetResponse()
        {
            responseBlock.WaitOne(Transport.TransportExceptionMonitor);
            var responseMessage = response;
            response = null;

            return responseMessage;
        }

        /// <summary>
        /// Creates a custom inheritance of <see cref="XmlSerializer"/> for the current category if available.
        /// Otherwise, the default <see cref="XmlSerializer"/> will be created.
        /// </summary>
        /// <returns>An <see cref="XmlSerializer"/> to use for messages.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        protected static XmlSerializer CreateMessageSerializer()
        {
            var categoryType = typeof(TCategory);
            var xmlSerializer = XmlSerializerContract.Instance.CanSerialize(categoryType)
                ? XmlSerializerContract.Instance.GetSerializer(categoryType)
                : new XmlSerializer(categoryType);
            return xmlSerializer;
        }

        /// <summary>
        /// Put the given request in a Csi message.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <param name="serializer">Serializer for the request.</param>
        /// <param name="category">Category of the request.</param>
        /// <param name="request">The request.</param>
        /// <returns>A Csi message with the given request.</returns>
        protected Csi MakeCsiMessageFromRequest<TRequest>(XmlSerializer serializer, Category category,
                                                          TRequest request)
        {
           var requestBody = new Csi { Category = category, MessageType = CsiMessageType.Request };

            //Write the request into an XmlDocument which can be placed in the request body.
            var doc = SerializeToXmlDocument(serializer, request);

            requestBody.CsiBody = doc.DocumentElement;
            return requestBody;
        }

        /// <summary>
        /// Put the given response in a Csi message.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="serializer">Serializer for the response.</param>
        /// <param name="category">Category of the response.</param>
        /// <param name="innerResponse">The response.</param>
        /// <returns>A Csi message with the given response.</returns>
        protected Csi MakeCsiMessageFromResponse<TResponse>(XmlSerializer serializer, Category category,
                                                            TResponse innerResponse)
        {
           var requestBody = new Csi { Category = category, MessageType = CsiMessageType.Response };

            //Write the request into an XmlDocument which can be placed in the request body.
            var doc = SerializeToXmlDocument(serializer, innerResponse);

            requestBody.CsiBody = doc.DocumentElement;
            return requestBody;
        }

        /// <summary>
        /// Serialize an XML serializable object to an XmlElement.
        /// </summary>
        /// <typeparam name="TObject">the type of the object to serialize.</typeparam>
        /// <param name="serializer">The serializer to use.</param>
        /// <param name="serializeObject">The object to serialize.</param>
        /// <returns>An XmlDocument containing the serialized object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either serializer or serializeObject are null.</exception>
        /// <exception cref="CategoryXmlSerializationException">
        /// Thrown if there is an error trying to serialize the object to XML.
        /// </exception>
        private XmlDocument SerializeToXmlDocument<TObject>(XmlSerializer serializer, TObject serializeObject)
        {
            if(serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            if(serializeObject == null)
            {
                throw new ArgumentNullException(nameof(serializeObject));
            }

            CsiSerializationTracing.Log.SerializeInnerMessageStart(Category);
            var doc = new XmlDocument();

            try
            {
                var nav = doc.CreateNavigator();
                var writer = nav.AppendChild();
                serializer.Serialize(writer, serializeObject);
                writer.Flush();
                writer.Close();
            }
            catch(Exception exception)
            {
                throw new CategoryXmlSerializationException(Category, serializeObject.GetType(), exception);
            }

            CsiSerializationTracing.Log.SerializeInnerMessageStop(Category);
            return doc;
        }

        #region ICabinetCategory Members

        /// <inheritdoc/>
        public abstract Category Category { get; }

        /// <inheritdoc/>
        public abstract ushort VersionMajor { get; }

        /// <inheritdoc/>
        public abstract ushort VersionMinor { get; }

        /// <inheritdoc/>
        public virtual XmlSerializer MessageSerializer => messageSerializer ?? (messageSerializer = CreateMessageSerializer());

        /// <inheritdoc/>
        public void InstallTransport(ICsiTransport transportToInstall)
        {
            Transport = transportToInstall;
        }

        /// <inheritdoc/>
        public abstract void HandleEvent(object message);

        /// <inheritdoc/>
        public abstract void HandleRequest(object message, ulong requestId);

        /// <inheritdoc/>
        public virtual void HandleResponse(object message)
        {
            response = message as TCategory;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message), "Response cannot be null.");
            }

            if(response == null)
            {
                throw new UnexpectedReplyTypeException("Unexpected response for category.", typeof(TCategory),
                                                       message.GetType());
            }

            responseBlock.Set();
        }

        #endregion ICabinetCategory Members

        #region IDisposable Implementation

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
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                //The manual reset event implements IDisposable, so there is no need to check if it converted.
                (responseBlock as IDisposable).Dispose();
                disposed = true;
            }
        }

        #endregion IDisposable Implementation
    }
}