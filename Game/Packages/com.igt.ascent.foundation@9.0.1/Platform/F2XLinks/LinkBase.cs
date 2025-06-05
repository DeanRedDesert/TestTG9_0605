// -----------------------------------------------------------------------
// <copyright file = "LinkBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.DiscoveryContextTypes;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using Game.Core.Threading;
    using Interfaces;
    using F2XLinkControl = Game.Core.Communication.Foundation.F2X.Schemas.Internal.LinkControl;

    /// <summary>
    /// Base class for all F2X client links, which usually use the F2X Link Control category
    /// for the first layer of category negotiation as well as link status handling.
    /// </summary>
    /// <devdoc>
    /// Modified based on <see cref="IGT.Game.Core.Communication.Foundation.Standard.F2XLinks.LinkBase"/>.
    /// Differences include:
    /// 1. Handling of ActionResponse is transferred to Transactional Event Queue.
    /// </devdoc>
    internal abstract class LinkBase : ILinkStatusCallbacks,
                                       ITransactionCallbacks,
                                       IDisposable
    {
        #region Fields

        /// <summary>
        /// The controller for negotiating and installing message categories.
        /// </summary>
        /// <devdoc>
        /// This is to be set by the derived class.  So it cannot be read only.
        /// </devdoc>
        protected LinkController LinkController;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        protected bool IsDisposed;

        #endregion

        #region Properties

        /// <devdoc>
        /// This is needed by ITransactionCallbacks until LinkController can be refactored
        /// not to take ITransactionCallbacks in its constructor.
        /// </devdoc>
        protected abstract IEventCallbacks EventCallbacks { get; set; }

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
        public IExtensionImportCollection ExtensionImportCollection { get; protected set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the Foundation shuts down the link.
        /// </summary>
        public event EventHandler LinkShutDownEvent;

        #endregion

        #region Public Methods

        /// <summary>
        /// Establishes a connection to the Foundation.
        /// </summary>
        /// <remarks>
        /// The base implementation returns as soon as the link level negotiation is complete.
        /// However, that may not work for derived link classes.  The derived link should override
        /// this method and do not return until the time is right for that particular link.
        /// </remarks>
        /// <returns>
        /// True if connection is established successfully; False otherwise.
        /// </returns>
        public virtual bool Connect()
        {
            return LinkController.Connect();
        }

        /// <summary>
        /// Disconnects from the Foundation.
        /// </summary>
        public virtual void Disconnect()
        {
            LinkController.Disconnect();
        }

        /// <summary>
        /// Gets an extended interface if it was requested and installed on any of th negotiation levels.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. <see langword="null"/> if none was found.
        /// </returns>
        public virtual TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class
        {
            return LinkController.GetInterface<TExtendedInterface>();
        }

        #endregion

        #region ILinkStatusCallbacks Members

        /// <inheritdoc/>
        public virtual void ProcessLinkStart(string jurisdiction, string connectToken,
                                             ICollection<DiscoveryContext> discoveryContexts,
                                             ICollection<F2XLinkControl.ExtensionImport> extensionImports)
        {
            Jurisdiction = jurisdiction;
            Token = connectToken;

            var mountPointContext = discoveryContexts.SingleOrDefault(
                discoveryContext => discoveryContext.DiscoveryType == MountPointDiscoveryType);

            if(mountPointContext == null)
            {
                throw new ApplicationException("Failed to locate the discovery context of type " + MountPointDiscoveryType);
            }

            MountPoint = mountPointContext.MountPath;

            ExtensionImportCollection = new ExtensionImportCollection(
                extensionImports.Select(import => new ExtensionImport(import.Extension.ExtensionIdentifier,
                                                                      new Version((int)import.Extension.ExtensionVersion.MajorVersion,
                                                                                  (int)import.Extension.ExtensionVersion.MinorVersion,
                                                                                  (int)import.Extension.ExtensionVersion.PatchVersion),
                                                                      import.ResourceDirectoryBase)));
        }

        /// <inheritdoc/>
        public abstract void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);

        /// <inheritdoc/>
        public virtual void ProcessLinkShutDown()
        {
            LinkShutDownEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public virtual void ProcessLinkPark()
        {
            EventCallbacks.PostEvent(new ParkEventArgs());
        }

        #endregion

        #region ITransactionCallbacks Members

        /// <devdoc>
        /// LinkBase has to implement this interface unless LinkController can be refactored
        /// not to take ITransactionCallbacks in its constructor.
        /// </devdoc>
        /// <inheritdoc/>
        public void ProcessActionResponse(byte[] payload)
        {
            if(EventCallbacks != null)
            {
                var transactionName = payload == null
                                          ? null
                                          : Encoding.ASCII.GetString(payload);

                EventCallbacks.PostEvent(new ActionResponseEventArgs(transactionName));
            }
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
            if(!IsDisposed && disposing)
            {
                LinkController.Dispose();

                IsDisposed = true;
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
        /// The list of API Managers to search.
        /// </param>
        /// <returns>
        /// The implementation of <typeparamref name="TCategoryCallbacks"/>.
        /// Null if not found.
        /// </returns>
        protected static TCategoryCallbacks Retrieve<TCategoryCallbacks>(IEnumerable<IApiManager> managers)
        {
            return managers.OfType<TCategoryCallbacks>().FirstOrDefault();
        }

        /// <summary>
        /// In a list of message category handlers, finds and returns the one that
        /// implements the given category interface and for the given category number.
        /// </summary>
        /// <typeparam name="TCategory">
        /// The type of the category to find.
        /// </typeparam>
        /// <param name="categoryHandlers">
        /// The list of message category handlers to search.
        /// </param>
        /// <param name="messageCategory">
        /// The number of the message category to find.
        /// </param>
        /// <returns>
        /// The implementation of <typeparamref name="TCategory"/>.
        /// Null if not found.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Thrown when the category found for the given <paramref name="messageCategory"/> is not
        /// of the given return type of <typeparamref name="TCategory"/>.
        /// </exception>
        protected static TCategory Retrieve<TCategory>(IDictionary<MessageCategory, IApiCategory> categoryHandlers,
                                                       MessageCategory messageCategory) where TCategory : class
        {
            TCategory result;

            if(categoryHandlers.TryGetValue(messageCategory, out var installedHandler))
            {
                result = installedHandler as TCategory ??
                         throw new ApplicationException($"The installed handler for {messageCategory} category is not of the expected type {typeof(TCategory)}.");
            }
            else
            {
                result = default;
            }

            return result;
        }

        #endregion
    }
}