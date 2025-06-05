//-----------------------------------------------------------------------
// <copyright file = "LinkController.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using F2X;
    using F2X.Schemas.Internal.DiscoveryContextTypes;
    using F2X.Schemas.Internal.LinkControl;
    using F2XCallbacks;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;
    using Threading;
    using Transport;

    /// <summary>
    /// This controller negotiates the API category versions and
    /// installs the category handlers for the communication links
    /// that uses the F2X protocol, e.g. F2R and F2E etc..
    /// </summary>
    public class LinkController : IConnectApiCallbacks,
                                  ILinkApiCallbacks,
                                  IDisposable
    {
        #region Public Properties

        /// <summary>
        /// Gets the monitor to monitor the exceptions on the transport callback threads.
        /// </summary>
        public IExceptionMonitor TransportExceptionMonitor => transport;

        /// <summary>
        /// Gets the id of the last opened transaction.
        /// </summary>
        public uint LastTransactionId => linkTransport.TransactionId;

        #endregion

        #region Fields

        /// <summary>
        /// Transport used by this link.
        /// </summary>
        private readonly IF2XTransport linkTransport;

        /// <summary>
        /// Underlying socket transport used by the F2X.
        /// </summary>
        private readonly ITransport transport;

        /// <summary>
        /// Event used to block until connecting is finished.
        /// </summary>
        private readonly AutoResetEvent connectComplete = new AutoResetEvent(false);

        /// <summary>
        /// The callback interface for handling link status.
        /// </summary>
        private readonly ILinkStatusCallbacks linkStatusCallbacks;

        /// <summary>
        /// List of API managers of different negotiation levels.
        /// </summary>
        private readonly IList<IApiManager> apiManagers;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Flag indicating if the link connection has been established.
        /// </summary>
        private volatile bool isConnected;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="LinkController"/> that uses
        /// a transport of the specified address and port.
        /// </summary>
        /// <param name="address">
        /// Foundation address to connect to.
        /// </param>
        /// <param name="port">
        /// Foundation port to connect to.
        /// </param>
        /// <param name="foundationTarget">
        /// Foundation version to target.
        /// </param>
        /// <param name="linkStatusCallbacks">
        /// The callback interface for handling link status.
        /// </param>
        /// <param name="categoryManagers">
        /// List of API managers of different <see cref="CategoryNegotiationLevel"/>s.
        /// This list must include at least <see cref="LinkApiManager"/>,
        /// but cannot include <see cref="ConnectApiManager"/>.
        /// </param>
        /// <param name="categoryDependencies">
        /// The dependencies required for creating categories.
        /// </param>
        /// <param name="baseExtensionDependencies">
        /// The common dependencies for extensions.
        /// This parameter is optional.  If not specified, it is default to null.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// List of interface extension configurations to request.
        /// This parameter is optional.  If not specified, it is default to null.
        /// </param>
        /// <remarks>
        /// Link Controller will be responsible for disposing all elements in <paramref name="categoryManagers"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="linkStatusCallbacks"/>, <paramref name="categoryManagers"/> or
        /// <paramref name="categoryDependencies"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="categoryDependencies"/> does not have a valid
        /// <see cref="IEventCallbacks"/> field.
        /// Thrown when <paramref name="categoryManagers"/> contains a <see cref="ConnectApiManager"/>,
        /// or does not contain a <see cref="LinkApiManager"/>.
        /// </exception>
        internal LinkController(string address, ushort port, FoundationTarget foundationTarget,
                                ILinkStatusCallbacks linkStatusCallbacks,
                                IList<IApiManager> categoryManagers,
                                CategoryNegotiationDependencies categoryDependencies,
                                IInterfaceExtensionDependencies baseExtensionDependencies = null,
                                IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations = null)
            : this(new SocketTransport(address, port),
                   foundationTarget,
                   linkStatusCallbacks,
                   categoryManagers,
                   categoryDependencies,
                   baseExtensionDependencies,
                   interfaceExtensionConfigurations)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="LinkController"/> that uses
        /// a specified transport.
        /// </summary>
        /// <param name="transport">
        /// This transport to be used for communication with Foundation.
        /// </param>
        /// <param name="foundationTarget">
        /// Foundation version to target.
        /// </param>
        /// <param name="linkStatusCallbacks">
        /// The callback interface for handling link status.
        /// </param>
        /// <param name="categoryManagers">
        /// List of API managers of different <see cref="CategoryNegotiationLevel"/>s.
        /// This list must include at least <see cref="LinkApiManager"/>,
        /// but cannot include <see cref="ConnectApiManager"/>.
        /// </param>
        /// <param name="categoryDependencies">
        /// The dependencies required for creating categories.
        /// </param>
        /// <param name="baseExtensionDependencies">
        /// The common dependencies for extensions.
        /// This parameter is optional.  If not specified, it is default to null.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// List of interface extension configurations to request.
        /// This parameter is optional.  If not specified, it is default to null.
        /// </param>
        /// <remarks>
        /// Link Controller will be responsible for disposing all elements in <paramref name="categoryManagers"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the nullable parameters is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="categoryDependencies"/> does not have a valid
        /// <see cref="IEventCallbacks"/> field.
        /// Thrown when <paramref name="categoryManagers"/> contains a <see cref="ConnectApiManager"/>,
        /// or does not contain a <see cref="LinkApiManager"/>.
        /// </exception>
        internal LinkController(ITransport transport,
                                FoundationTarget foundationTarget,
                                ILinkStatusCallbacks linkStatusCallbacks,
                                IList<IApiManager> categoryManagers,
                                CategoryNegotiationDependencies categoryDependencies,
                                IInterfaceExtensionDependencies baseExtensionDependencies = null,
                                IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations = null)
        {
            if(categoryDependencies == null)
            {
                throw new ArgumentNullException(nameof(categoryDependencies));
            }

            if(categoryDependencies.EventCallbacks == null)
            {
                throw new ArgumentException("Category Dependencies does not have a valid EventCallbacks field.");
            }

            if(categoryManagers == null)
            {
                throw new ArgumentNullException(nameof(categoryManagers));
            }

            if(categoryManagers.Any(manager => manager is ConnectApiManager))
            {
                throw new ArgumentException("Link client is not allowed to configure a Connect API Manager.");
            }

            // There must be a LinkApiManager in the passed in list.
            var linkApiManager = categoryManagers.OfType<LinkApiManager>().FirstOrDefault();
            if(linkApiManager == null)
            {
                throw new ArgumentException("Link client must provide a Link API Manager.");
            }

            // Link API Manager needs a special initialization.
            linkApiManager.SetLinkApiCallbacks(this);

            // Save the callback interfaces.
            this.linkStatusCallbacks = linkStatusCallbacks ?? throw new ArgumentNullException(nameof(linkStatusCallbacks));

            // Instantiate transport.
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            linkTransport = new F2XTransport(transport);

            // Add transport to the category negotiation/creation dependencies.
            categoryDependencies.Transport = linkTransport;

            // Instantiate Connect API Manager, and add it to the API Manager list.
            var connectApiManager = new ConnectApiManager(this);

            apiManagers = categoryManagers;
            apiManagers.Add(connectApiManager);

            // Instantiate and install the connect category.
            var connectCategory = new ConnectCategory(linkTransport, connectApiManager);
            linkTransport.InstallCategoryHandler(connectCategory);

            // Initialize all API Managers.
            var interfaceConfigurationList = interfaceExtensionConfigurations?.ToList();
            foreach(var apiManager in apiManagers)
            {
                apiManager.Initialize(foundationTarget, categoryDependencies, baseExtensionDependencies, interfaceConfigurationList);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Establishes a connect to the Foundation.
        /// </summary>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        public bool Connect()
        {
            linkTransport.Connect();
            connectComplete.WaitOne(TransportExceptionMonitor);

            return isConnected;
        }

        /// <summary>
        /// Disconnects from the Foundation.
        /// </summary>
        public void Disconnect()
        {
            linkTransport.Disconnect();

            // Clean up just to make sure.
            isConnected = false;
            connectComplete.Reset();
        }

        /// <summary>
        /// Gets an extended interface if it was requested and installed on any of the negotiation levels.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of. 
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. <see langword="null"/> if none was found.
        /// </returns>
        public TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class
        {
            TExtendedInterface result = null;

            foreach(var apiManager in apiManagers)
            {
                result = apiManager.GetInterface<TExtendedInterface>();

                if(result != null)
                {
                    break;
                }
            }
            return result;
        }

        #endregion

        #region IConnectApiCallbacks Members

        /// <inheritdoc />
        public void ProcessConnectShutDown()
        {
            // If the theme level negotiation in LinkControlCategory fails,
            // Foundation would send a Shut Down message, while the game is
            // still being blocked in the ConnectToFoundation method.
            // Unblock connectComplete so that the game can properly shut down.
            connectComplete.Set();

            linkStatusCallbacks.ProcessLinkShutDown();
        }

        #endregion

        #region ILinkApiCallbacks Members

        /// <inheritdoc />
        public void ProcessLinkApiStart(string jurisdiction, string connectToken,
                                        ICollection<DiscoveryContext> discoveryContexts,
                                        ICollection<ExtensionImport> extensionImports)
        {
            linkStatusCallbacks.ProcessLinkStart(jurisdiction, connectToken, discoveryContexts, extensionImports);
        }

        /// <inheritdoc />
        public void ProcessLinkApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            linkStatusCallbacks.ProcessLinkNegotiation(installedHandlers);

            isConnected = true;

            // Signal the connect complete event.
            // In case of theme level re-negotiations, signaling this event is redundant but OK.
            connectComplete.Set();
        }

        /// <inheritdoc />
        public void ProcessLinkPark()
        {
            linkStatusCallbacks.ProcessLinkPark();
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                // The auto reset event implements IDisposable, so there is no need to check if it converted.
                (connectComplete as IDisposable).Dispose();

                // Dispose transport.
                if(transport is IDisposable disposableTransport)
                {
                    disposableTransport.Dispose();
                }

                // Dispose link transport.
                // It is kept as an interface, so needs a conversion first.
                if(linkTransport is IDisposable disposableLinkTransport)
                {
                    disposableLinkTransport.Dispose();
                }

                // Dispose all API Managers.
                foreach(var apiManager in apiManagers)
                {
                    if(apiManager is IDisposable disposableManager)
                    {
                        disposableManager.Dispose();
                    }
                }

                disposed = true;
            }
        }

        #endregion
    }
}
