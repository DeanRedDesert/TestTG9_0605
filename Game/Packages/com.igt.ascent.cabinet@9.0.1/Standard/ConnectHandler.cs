//-----------------------------------------------------------------------
// <copyright file = "ConnectHandler.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Xml.Serialization;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;
    using CSI.Schemas.Serializers;
    using Threading;

    /// <summary>
    /// Category which handles the initial CSI connection.
    /// </summary>
    internal class ConnectHandler : IConnectHandler
    {
        #region Private Fields

        /// <summary>
        /// Get the serializer for connect messages.
        /// </summary>
        public XmlSerializer MessageSerializer { get; }

        /// <summary>
        /// Reset event which is used to block on requests made to the CSI Manager.
        /// </summary>
        private readonly AutoResetEvent connectBlock = new AutoResetEvent(false);

        /// <summary>
        /// Response from the CSI Manager.
        /// </summary>
        private CsiConnect response;

        /// <summary>
        /// Categories being requested.
        /// </summary>
        private readonly List<ICabinetCategory> requestedCategories = new List<ICabinetCategory>();

        /// <summary>
        /// Token used by the CSI Manager to identify the game associated with this connection.
        /// </summary>
        private readonly string token;

        /// <summary>
        /// The client type of the application that is connecting
        /// </summary>
        private readonly ClientType clientType;

        /// <summary>
        /// Flag which indicates if this object has been disposed.
        /// </summary>
        private bool disposed;

        #endregion Private Fields

        #region Events

        /// <summary>
        /// Event which is fired when a category is installed.
        /// </summary>
        public event EventHandler<CategoryInstalledEventArgs> CategoryInstalledEvent;

        #endregion Events

        #region Constructor

        /// <summary>
        /// Construct an instance of the connect handler.
        /// </summary>
        /// <param name="token">Token to identify this instance with the CSI.</param>
        /// <param name="clientType">The client type of the connecting application.</param>
        public ConnectHandler(string token, ClientType clientType)
        {
            this.token = token;
            this.clientType = clientType;
            MessageSerializer = XmlSerializerContract.Instance.CanSerialize(typeof(CsiConnect))
                ? XmlSerializerContract.Instance.GetSerializer(typeof(CsiConnect))
                : new XmlSerializer(typeof(CsiConnect));
        }

        #endregion Constructor

        #region IConnectHandler Implementation

        /// <inheritdoc/>
        public void HandleMessage(CsiConnect connectMessage)
        {
            if(connectMessage == null)
            {
                throw new ArgumentNullException(nameof(connectMessage));
            }
            if(connectMessage.Item == null)
            {
                //Throw a message body exception.
                throw new UnknownConnectMessageException("Could not decode message body of connect message.");
            }

            var itemType = connectMessage.Item.GetType();

            if(itemType == typeof(CsiConnectInitializeCsiResponse))
            {
                response = connectMessage;
                connectBlock.Set();
            }
            else if(itemType == typeof(CsiConnectShutdownSend))
            {
                throw new CsiShutdownException("CSI Connect Handler received a shutdown message.");
            }
            else
            {
                throw new UnknownConnectMessageException("Unknown message received: " + connectMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public void Initialize(CsiTransport transport)
        {
            var initRequest = new CsiConnectInitializeCsiRequest
            {
                Categories =
                    new CsiConnectInitializeCsiRequestCategories {CategoryVersion = new List<CategoryVersion>()},
                ClientInfo =
                    new ClientConnection {ClientType = clientType, Token = token}
            };

            var request = new CsiConnect
            {
                Item = initRequest
            };

            var categoryList = initRequest.Categories.CategoryVersion;
            categoryList.AddRange(requestedCategories.Select(requestedCategory => new CategoryVersion
            {
                Category = requestedCategory.Category,
                MajorVersion = requestedCategory.VersionMajor,
                MinorVersion = requestedCategory.VersionMinor
            }));

            CsiTransportTracing.Log.SendConnectRequestStart();
            var message = SerializeCsiConnectRequest(request, MessageSerializer);
            transport.SendMessage(message);
            CsiTransportTracing.Log.SendConnectRequestStop(message.Length);
            connectBlock.WaitOne(transport.TransportExceptionMonitor);

            if(response != null)
            {
                if(response.Item is CsiConnectInitializeCsiResponse initResponse)
                {
                    // Check for any problems with the connection attempt.
                    if(initResponse.ConnectResponse.ErrorCode != ConnectErrorCode.NONE)
                    {
                        throw new CsiFailedToConnectException(initResponse.ConnectResponse.ErrorCode.ToString(),
                            initResponse.ConnectResponse.ErrorDescription);
                    }

                    foreach(var category in initResponse.Categories.CategoryVersion)
                    {
                        var categoryToInstall = (from requestedCategory in requestedCategories
                            where requestedCategory.Category == category.Category
                            select requestedCategory).FirstOrDefault();

                        if(categoryToInstall != null)
                        {
                            if(category.MajorVersion == categoryToInstall.VersionMajor &&
                               categoryToInstall.VersionMinor <= category.MinorVersion)
                            {
                                // Install the handler.
                                transport.InstallCategory(categoryToInstall);
                                CategoryInstalledEvent?.Invoke(this, new CategoryInstalledEventArgs(categoryToInstall));
                            }
                            else
                            {
                                throw new CategoryVersionMismatchException(categoryToInstall.VersionMajor,
                                    categoryToInstall.VersionMinor,
                                    category.MajorVersion,
                                    category.MinorVersion,
                                    categoryToInstall.Category);
                            }
                        }

                        //TODO: Determine if anything should be done about categories which were not requested.
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Deinitialize(CsiTransport transport)
        {
            var shutdownRequest = new CsiConnectShutdownSend
            {
                ShutdownDescription = "Normal client disconnect."
            };

            var request = new CsiConnect
            {
                Item = shutdownRequest
            };

            CsiTransportTracing.Log.SendShutdownRequestStart();
            // This is an event-type message sent to the CSI. The CSI will not send a reply
            // to this event, regardless of what happens.
            var message = SerializeCsiConnectRequest(request, MessageSerializer);
            transport.SendMessage(message);
            CsiTransportTracing.Log.SendShutdownRequestStop(message.Length);
        }

        #endregion IConnectHandler Implementation

        #region Public Methods

        /// <summary>
        /// Add a category to request from the CSI Manager.
        /// </summary>
        /// <param name="category">The category to request.</param>
        /// <exception cref="ArgumentNullException">Thrown if the category is null.</exception>
        public void RequestCategory(ICabinetCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }
            requestedCategories.Add(category);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Serialize a CSI connect request object into a string.
        /// </summary>
        /// <param name="request">The CSI connect request object.</param>
        /// <param name="serializer">The serializer used to serialize the request.</param>
        /// <returns>The serialized string.</returns>
        private static string SerializeCsiConnectRequest(CsiConnect request, XmlSerializer serializer)
        {
            CsiSerializationTracing.Log.SerializeCsiConnectMessageStart();
            var xml = XmlHelpers.GetXmlString(request, serializer);
            CsiSerializationTracing.Log.SerializeCsiConnectMessageStop(xml.Length);
            return xml;
        }

        #endregion Private Methods

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
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                //The manual reset event implements IDisposable, so there is no need to check if it converted.
                (connectBlock as IDisposable).Dispose();
                disposed = true;
            }
        }

        #endregion Disposable Implementation
    }
}