// -----------------------------------------------------------------------
// <copyright file = "LinkBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.DiscoveryContextTypes;
    using F2X.Schemas.Internal.LinkControl;
    using F2XCallbacks;
    using F2XTransport;
    using Threading;

    /// <summary>
    /// Base class for all F2X client links, which usually use the F2X Link Control category
    /// for the first layer of category negotiation as well as link status handling.
    /// 
    /// This base class also encapsulates some common functionality such as transactions and
    /// transactional events handling.
    /// </summary>
    internal abstract class LinkBase : ILinkStatusCallbacks,
                                       ITransactionEventLink,
                                       ITransactionCallbacks, IEventCallbacks,
                                       IDisposable
    {
        #region Fields

        /// <summary>
        /// The controller for negotiating and installing message categories.
        /// </summary>
        /// <devdoc>
        /// This is to be set by the derived class.  So it cannot be readonly.
        /// </devdoc>
        protected LinkController LinkController;

        /// <summary>
        /// The category for requesting a client initiated transaction.
        /// </summary>
        private IActionRequestCategory actionRequestCategory;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the discovery context whose path is going to be used as
        /// the <see cref="MountPoint"/> of the client package.
        /// </summary>
        protected abstract DiscoveryType MountPointDiscoveryType { get; }

        /// <summary>
        /// Gets the mount point of the client package.
        /// </summary>
        public string MountPoint { get; private set; }

        /// <summary>
        /// Gets the jurisdiction string.
        /// </summary>
        public string Jurisdiction { get; private set; }

        /// <summary>
        /// Gets the token assigned by the Foundation which is used to 
        /// identify this client instance.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Gets the monitor to monitor the exceptions on the transport callback threads.
        /// </summary>
        public IExceptionMonitor TransportExceptionMonitor => LinkController.TransportExceptionMonitor;

        /// <summary>
        /// Gets the information on imported extensions linked to the client application.
        /// </summary>
        public IExtensionImportCollection ExtensionImportCollection;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the Foundation shuts down the link.
        /// </summary>
        public event EventHandler LinkShutDownEvent;

        /// <summary>
        /// Occurs when the Foundation tells the binary to park.
        /// </summary>
        public event EventHandler ParkEvent;

        #endregion

        #region Public Methods

        /// <summary>
        /// Establishes a connection to the Foundation.
        /// </summary>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        public bool Connect()
        {
            return LinkController.Connect();
        }

        /// <summary>
        /// Disconnects from the Foundation.
        /// </summary>
        public void Disconnect()
        {
            LinkController.Disconnect();
        }

        /// <summary>
        /// Gets an extended interface if it was requested and installed.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of. 
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed, then <see langword="null"/> will
        /// be returned.
        /// </returns>
        public TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class
        {
            return LinkController.GetInterface<TExtendedInterface>();
        }

        #endregion

        #region ILinkStatusCallbacks Members

        /// <inheritdoc/>
        public virtual void ProcessLinkStart(string jurisdiction, string connectToken,
                                             ICollection<DiscoveryContext> discoveryContexts,
                                             ICollection<ExtensionImport> extensionImports)
        {
            Jurisdiction = jurisdiction;
            Token = connectToken;

            var mountPointContext = discoveryContexts.SingleOrDefault(
                discoveryContext => discoveryContext.DiscoveryType == MountPointDiscoveryType);

            if(mountPointContext != null)
            {
                MountPoint = mountPointContext.MountPath;
            }

            ExtensionImportCollection = new ExtensionImportCollection(
                extensionImports.Select(import => import.ToPublic()));
        }

        /// <inheritdoc/>
        public virtual void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            actionRequestCategory = installedHandlers.ContainsKey(MessageCategory.ActionRequest)
                                        ? installedHandlers[MessageCategory.ActionRequest] as IActionRequestCategory
                                        : null;
        }

        /// <inheritdoc/>
        public virtual void ProcessLinkShutDown()
        {
            var handler = LinkShutDownEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public virtual void ProcessLinkPark()
        {
            var handler = ParkEvent;
            handler?.Invoke(this, EventArgs.Empty);

            ((IEventCallbacks)this).PostEvent(new ParkEventArgs());
        }

        #endregion

        #region ITransactionEventLink Members

        /// <inheritdoc/>
        public virtual bool ActionRequest(byte[] payload)
        {
            actionRequestCategory.ActionRequest(payload);
            return true;
        }

        /// <inheritdoc/>
        public event EventHandler PostingEvent;

        /// <inheritdoc/>
        public event EventHandler ActionResponseEvent;

        #endregion

        #region ITransactionCallbacks Members

        /// <inheritdoc/>
        public void ProcessActionResponse(byte[] payload)
        {
            var handler = ActionResponseEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IEventCallbacks Members

        /// <inheritdoc/>
        void IEventCallbacks.PostEvent(EventArgs foundationEvent)
        {
            var handler = PostingEvent;
            handler?.Invoke(this, foundationEvent);
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
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                LinkController.Dispose();

                disposed = true;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// In a list of API Managers, finds and returns the one that
        /// implements the given category callbacks interface.
        /// </summary>
        /// <typeparam name="TCategoryCallbacks">
        /// The type of the category callbacks to find.
        /// </typeparam>
        /// <param name="managers">
        /// The list of API Managers for search.
        /// </param>
        /// <returns>
        /// The implementation of <typeparamref name="TCategoryCallbacks"/>.
        /// Null if not found.
        /// </returns>
        protected static TCategoryCallbacks Retrieve<TCategoryCallbacks>(IEnumerable<IApiManager> managers)
        {
            return managers.OfType<TCategoryCallbacks>().FirstOrDefault();
        }

        #endregion
    }
}