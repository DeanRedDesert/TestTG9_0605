// -----------------------------------------------------------------------
// <copyright file = "ExtensionBinLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ExtensionLib.Interfaces;
    using ExtensionLib.Interfaces.TiltControl;
    using F2XLinks;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Standard;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using Game.Core.Communication.Foundation.Transport;
    using Game.Core.Threading;
    using Interfaces;
    using Platform.Interfaces;
    using Platform.Standard;

    /// <summary>
    /// Standard implementation of the interface for an Extension Bin to communicate with the Foundation.
    /// </summary>
    public sealed class ExtensionBinLib : StandardAppLibBase,
                                          IExtensionBinLibRestricted, IExtensionBinLib
    {
        #region Private Fields

        private readonly ExtensionBinLink extensionBinLink;

        /// <summary>
        /// Object used for unified extension parcel communication.
        /// </summary>
        private readonly ExtensionParcelComm extensionParcelComm;

        /// <summary>
        /// Object used for getting localization information.
        /// It is on link level, thus not associated with any particular inner context.
        /// Therefore, use IInnerContext for place holder.
        /// </summary>
        private readonly Localization<IInnerContext> localization;

        /// <summary>
        /// Object used for getting culture information.
        /// </summary>
        private readonly CultureRead cultureRead;

        /// <summary>
        /// Object used to request information about custom configuration items.
        /// </summary>
        private readonly ConfigurationRead configurationRead;

        /// <summary>
        /// Object used for accessing the critical data.
        /// </summary>
        private readonly CriticalDataAccessor criticalDataAccessor;

        /// <summary>
        /// Object used to request information about game themes and paytables.
        /// </summary>
        private readonly GameInformation gameInformation;

        /// <summary>
        /// Object used for bank status.
        /// </summary>
        private readonly BankStatus bankStatus;

        /// <summary>
        /// Object used for extension tilt management.
        /// </summary>
        private readonly ExtensionTiltController extensionTiltController;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionBinLib"/>.
        /// </summary>
        /// <remarks>
        /// When <paramref name="enableConfigurationCaching"/> is true, the <see cref="IConfigurationRead"/> implementation
        /// will cache custom configuration items read from the Foundation for performance purposes.
        /// For non-system extension, the clearing of the cache is taken care of by Extension Bin Lib when the application
        /// context is activated/re-activated.  However, that is not sufficient for system extensions. System extensions
        /// must actively clear the cache at appropriate times (depending on specific extension implementation) by calling
        /// <see cref="ClearConfigurationCache"/> if configuration cache is used.
        /// </remarks>
        /// <param name="foundationTarget">
        /// The foundation version to target.
        /// </param>
        /// <param name="connectionConfigurator">
        /// The configurator used to establish connection to foundation.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// List of additional interface configurations to install in this extension bin lib.
        /// </param>
        /// <param name="enableConfigurationCaching">
        /// A boolean flag that indicates if configuration caching should be enabled or not. The configuration cache is
        /// used by the <see cref="IConfigurationRead"/> implementation to request information about custom configuration items
        /// from the Foundation. Also refer to the documentation remarks on this constructor.
        /// </param>
        /// <remarks>
        /// Requested interface configurations may not be available if not supported by the underlying platform.
        /// </remarks>
        public ExtensionBinLib(FoundationTarget foundationTarget,
                               IConnectionConfigurator connectionConfigurator,
                               IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations,
                               bool enableConfigurationCaching = false)
        {
            var dummyTransactionVerifier = new DummyTransactionVerifier();

            // Set up connection.
            var socketTransport = new SocketTransport(connectionConfigurator.TransportConfiguration.Address,
                                                      connectionConfigurator.TransportConfiguration.Port);
            DisposableCollection.Add(socketTransport);

            var baseExtensionDependencies = new InterfaceExtensionDependencies
                                                {
                                                    TransactionWeightVerification = dummyTransactionVerifier,
                                                    TransactionalEventDispatcher = TransEventQueue,
                                                    NonTransactionalEventDispatcher = NonTransEventQueue,
                                                    LayeredContextActivationEvents = this,
                                                };

            extensionBinLink = new ExtensionBinLink(foundationTarget,
                                                    socketTransport,
                                                    baseExtensionDependencies: baseExtensionDependencies,
                                                    additionalInterfaceConfigurations: interfaceExtensionConfigurations,
                                                    eventCallbacks: TransEventQueue,
                                                    nonTransactionalEventCallbacks: NonTransEventQueue);
            DisposableCollection.Add(extensionBinLink);

            extensionBinLink.LinkShutDownEvent += HandleLinkShutDown;

            // Create interface implementations available on Link level.
            extensionParcelComm = new ExtensionParcelComm(dummyTransactionVerifier, TransEventQueue, NonTransEventQueue);
            localization = new Localization<IInnerContext>();
            cultureRead = new CultureRead(dummyTransactionVerifier, TransEventQueue);
            configurationRead = new ConfigurationRead(dummyTransactionVerifier, new ExtensionConfigurationAccessContext(), enableConfigurationCaching);
            criticalDataAccessor = new CriticalDataAccessor(new ExtensionCriticalDataAccessValidation());
            bankStatus = new BankStatus(dummyTransactionVerifier, TransEventQueue);
            gameInformation = new GameInformation(dummyTransactionVerifier);
            extensionTiltController = new ExtensionTiltController(dummyTransactionVerifier, TransEventQueue);

            // Create inner libs.
            SystemExtensionLib = new SystemExtensionLib(extensionBinLink.SystemExtensionLink, TransEventQueue, NonTransEventQueue);
            AppExtensionLib = new AppExtensionLib(extensionBinLink.AppExtensionLink, TransEventQueue, NonTransEventQueue);
            AscribedGameExtensionLib = new AscribedGameExtensionLib(extensionBinLink.AscribedGameExtensionLink, TransEventQueue, NonTransEventQueue);
            AscribedChooserExtensionLib = new AscribedChooserExtensionLib(extensionBinLink.TsmExtensionLink, TransEventQueue, NonTransEventQueue);

            // Create event table.
            CreateEventTable();

            // Subscribe event handlers.
            // Outer context
            ActivateContextEvent += (s, e) => ResetLinkLevelContextCache();

            // System context
            SystemExtensionLib.ActivateContextEvent += (s, e) =>
                                                       {
                                                           ResetLinkLevelContextCache();
                                                           ActivateLayeredContext(ContextLayer.SystemExtension);
                                                       };
            SystemExtensionLib.InactivateContextEvent += (s, e) => InactivateLayeredContext(ContextLayer.SystemExtension);

            // App context
            AppExtensionLib.ActivateContextEvent += (s, e) =>
                                                    {
                                                        ResetLinkLevelContextCache();
                                                        ActivateLayeredContext(ContextLayer.AppExtension);
                                                    };
            AppExtensionLib.InactivateContextEvent += (s, e) => InactivateLayeredContext(ContextLayer.AppExtension);

            // Ascribed Game context
            AscribedGameExtensionLib.ActivateContextEvent += (s, e) => ResetLinkLevelContextCache();

            // Ascribed Chooser context
            AscribedChooserExtensionLib.ActivateContextEvent += (s, e) => ResetLinkLevelContextCache();
        }

        #endregion

        #region Implementation of IExtensionBinLibRestricted

        /// <inheritdoc />
        public event EventHandler<ActionResponseEventArgs> ActionResponseEvent;

        /// <inheritdoc />
        public event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <inheritdoc />
        public event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <inheritdoc />
        public event EventHandler<ParkEventArgs> ParkEvent;

        /// <inheritdoc />
        public string Token => extensionBinLink.Token;

        /// <inheritdoc />
        public string MountPoint => extensionBinLink.MountPoint;

        /// <inheritdoc />
        public IExceptionMonitor ExceptionMonitor => extensionBinLink.TransportExceptionMonitor;

        /// <inheritdoc />
        public uint LastTransactionId => extensionBinLink.LastTransactionId;

        /// <inheritdoc />
        public bool ConnectToFoundation()
        {
            var connected = extensionBinLink.Connect();
            if(connected)
            {
                configurationRead.Initialize(new ConfigurationReadLink(extensionBinLink.CustomConfigurationReadCategory));
                localization.Initialize(extensionBinLink.LocalizationCategory);
                cultureRead.Initialize(extensionBinLink.CultureReadCategory);
                criticalDataAccessor.Initialize(extensionBinLink.NonTransactionalCritDataReadCategory,
                                                extensionBinLink.TransactionalCritDataReadCategory,
                                                extensionBinLink.TransactionalCritDataWriteCategory);
                gameInformation.Initialize(extensionBinLink.GameInformationCategory,
                                           null,
                                           extensionBinLink.MountPoint);
                extensionTiltController.Initialize(extensionBinLink.TiltControlCategory);
                extensionParcelComm.Initialize(extensionBinLink.ParcelCommCategory);
                bankStatus.Initialize(extensionBinLink.BankStatusCategory);
            }

            return connected;
        }

        /// <inheritdoc />
        public bool DisconnectFromFoundation()
        {
            extensionBinLink.Disconnect();
            return true;
        }

        /// <inheritdoc />
        public void ActionRequest(string transactionName = null)
        {
            var payload = transactionName == null
                              ? new byte[0]
                              : Encoding.ASCII.GetBytes(transactionName);

            extensionBinLink.ActionRequestCategory.ActionRequest(payload);
        }

        #endregion

        #region IExtensionBinLib Implementation

        /// <inheritdoc />
        public ISystemExtensionLib SystemExtensionLib { get; }

        /// <inheritdoc />
        public IAppExtensionLib AppExtensionLib { get; }

        /// <inheritdoc />
        public IAscribedGameExtensionLib AscribedGameExtensionLib { get; }

        /// <inheritdoc />
        public IAscribedChooserExtensionLib AscribedChooserExtensionLib { get; }

        /// <inheritdoc />
        public IExtensionImportCollection ExtensionImportCollection => extensionBinLink.ExtensionImportCollection;

        /// <inheritdoc />
        public IExtensionParcelComm ExtensionParcelComm => extensionParcelComm;

        /// <inheritdoc />
        public ILocalization Localization => localization;

        /// <inheritdoc />
        public ICultureRead CultureRead => cultureRead;

        /// <inheritdoc />
        public IConfigurationRead ConfigurationRead => configurationRead;

        /// <inheritdoc />
        public ICriticalDataAccessor CriticalDataAccessor => criticalDataAccessor;

        /// <inheritdoc />
        public IGameInformation GameInformation => gameInformation;

        /// <inheritdoc />
        public IBankStatus BankStatus => bankStatus;

        /// <inheritdoc />
        public IExtensionTiltController ExtensionTiltController => extensionTiltController;

        /// <inheritdoc />
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            return extensionBinLink.GetInterface<TExtendedInterface>();
        }

        /// <inheritdoc />
        public void ClearConfigurationCache()
        {
            ResetLinkLevelContextCache();
        }

        #endregion

        #region Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventTable()
        {
            // Events should be listed in alphabetical order!

            EventTable[typeof(ActivateContextEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActivateContextEvent);

            EventTable[typeof(InactivateContextEventArgs)] =
                (sender, eventArgs) => HandleInactivateContext();

            EventTable[typeof(ActionResponseEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActionResponseEvent);

            EventTable[typeof(ShutDownEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ShutDownEvent);

            EventTable[typeof(ParkEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ParkEvent);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Resets the Link level context caches.
        /// The config data of Link level categories could change while the link level context is being active.
        /// </summary>
        private void ResetLinkLevelContextCache()
        {
            // Clear cache of custom configuration items.
            configurationRead.ClearCache();

            // Clear config data of all Link level categories.
            localization.NewContext(null);

            // Notify link level interface extensions.
            ActivateLayeredContext(ContextLayer.Application);
        }

        /// <summary>
        /// Actions performed when the extension bin application is inactivated.
        /// </summary>
        private void HandleInactivateContext()
        {
            // Clear the tilt info when the extension bin goes inactive
            extensionTiltController.ClearTiltInfo();

            InactivateLayeredContext(ContextLayer.Application);
        }

        /// <summary>
        /// Actions performed extension bin link is being shut down by Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleLinkShutDown(object sender, EventArgs eventArgs)
        {
            RaiseEvent(this, typeof(ShutDownEventArgs), new ShutDownEventArgs());
        }

        #endregion

        #endregion
    }
}