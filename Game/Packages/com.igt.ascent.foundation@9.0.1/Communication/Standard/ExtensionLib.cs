//-----------------------------------------------------------------------
// <copyright file = "ExtensionLib.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces.TiltControl;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2XLinks;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// Standard implementation of <see cref="IExtensionLib"/> that
    /// communicates with the Foundation for extension executables.
    /// </summary>
    public class ExtensionLib : IExtensionLib,
                                IExtensionLibRestricted,
                                ITransactionVerification,
                                IConfigurationAccessContext,
                                ICriticalDataAccessValidation,
                                ITransactionWeightVerificationDependency,
                                ILayeredContextActivationEventsDependency,
                                IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Object used to communicate with the foundation.
        /// </summary>
        private readonly ExtensionLink extensionLink;

        /// <summary>
        /// Object used to manage extension initiated transactions and Foundation events.
        /// </summary>
        private readonly TransactionManager transactionManager;

        /// <summary>
        /// Object used to process events.
        /// </summary>
        private readonly EventCoordinator eventCoordinator;

        /// <summary>
        /// Object used to manage Foundation non transactional events.
        /// </summary>
        private readonly NonTransactionalEventManager nonTransactionalEventManager;

        /// <summary>
        /// Object used to request information about custom configuration items.
        /// </summary>
        private readonly ConfigurationRead configurationRead;

        /// <summary>
        /// Object used for getting localization information.
        /// </summary>
        private readonly LocalizationInformation localizationInformation;

        /// <summary>
        /// Object used for getting culture information.
        /// </summary>
        private readonly CultureRead cultureRead;

        /// <summary>
        /// Object used for accessing the critical data.
        /// </summary>
        private readonly CriticalDataAccessor criticalDataAccessor;

        /// <summary>
        /// Object used to request information about game themes and paytables.
        /// </summary>
        private readonly GameInformation gameInformation;

        /// <summary>
        /// Object used for extension tilt management.
        /// </summary>
        private readonly ExtensionTiltController extensionTiltController;

        /// <summary>
        /// Object used for unified extension parcel communication.
        /// </summary>
        private readonly ExtensionParcelComm extensionParcelComm;

        /// <summary>
        /// Object used for bank status.
        /// </summary>
        private readonly BankStatus bankStatus;

        /// <summary>
        /// Dictionary of available Extension Lib services keyed by their interface type.
        /// </summary>
        private readonly Dictionary<Type, object> builtinServices = new Dictionary<Type, object>();

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// Flag indicating if the extension is in active context.
        /// </summary>
        private bool isContextActive;

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="ExtensionLib"/> that
        /// targets a specific version of Foundation, requests optional interfaces, and indicates if the configuration cache
        /// should be enabled or not.
        /// </summary>
        /// <remarks>
        /// The configuration cache is used by the <see cref="IConfigurationRead"/> implementation to request information
        /// about custom configuration items from the Foundation. For non-system extension, the clearing of configuration
        /// cache is taken care of by Extension Lib when the application context is activated/re-activated. However, that
        /// is not sufficient for system extensions. System extensions must actively clear the cache at appropriate times
        /// (depending on specific extension implementation) if configuration cache is used.
        /// </remarks>
        /// <param name="foundationTarget">Foundation version to target.
        /// </param>
        /// <param name="additionalInterfaceConfigurations">
        /// List of additional interface configurations to install in this Extension Lib. This list can be null if no
        /// additional interface configuration is needed.
        /// </param>
        /// <param name="enableConfigurationCaching">
        /// A boolean flag that indicates if configuration caching should be enabled or not. The configuration cache is
        /// used by the <see cref="IConfigurationRead"/> implementation to request information about custom configuration items
        /// from the Foundation. Also refer to the documentation remarks on this constructor.
        /// </param>
        public ExtensionLib(FoundationTarget foundationTarget,
                            IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations = null,
                            bool enableConfigurationCaching = false)
        {
            ParseFoundationArguments(out var address, out var port);

            var baseExtensionDependencies = new InterfaceExtensionDependencies
                                            {
                                                TransactionWeightVerification = this,
                                                LayeredContextActivationEvents = this,
                                            };

            // Create the non transactional event manager.
            nonTransactionalEventManager = new NonTransactionalEventManager();
            nonTransactionalEventManager.EventDispatchedEvent += HandleEventDispatchedEvent;
            baseExtensionDependencies.NonTransactionalEventDispatcher = nonTransactionalEventManager;

            // Create the extension link.
            extensionLink = new ExtensionLink(address, port, foundationTarget,
                                              baseExtensionDependencies, additionalInterfaceConfigurations,
                                              nonTransactionalEventManager);
            extensionLink.LinkShutDownEvent += HandleLinkShutDown;

            CreateEventLookupTable();

            // Create the transaction manager.
            transactionManager = new TransactionManager(extensionLink, extensionLink.TransportExceptionMonitor);
            transactionManager.EventDispatchedEvent += HandleEventDispatchedEvent;
            baseExtensionDependencies.TransactionalEventDispatcher = transactionManager;

            // Create the event coordinator.
            eventCoordinator = new EventCoordinator(transactionManager, extensionLink.TransportExceptionMonitor);
            builtinServices[typeof(IEventCoordinator)] = eventCoordinator;
            builtinServices[typeof(IEventProcessing)] = eventCoordinator;

            // Register other event sources with the event coordinator.
            eventCoordinator.RegisterEventSource(nonTransactionalEventManager);

            // Create interface implementations.
            configurationRead = new ConfigurationRead(this, this, enableConfigurationCaching);
            localizationInformation = new LocalizationInformation(this);
            cultureRead = new CultureRead(this, transactionManager);
            extensionParcelComm = new ExtensionParcelComm(this, transactionManager, nonTransactionalEventManager);
            criticalDataAccessor = new CriticalDataAccessor(this);
            gameInformation = new GameInformation(this);
            extensionTiltController = new ExtensionTiltController(this, transactionManager);
            bankStatus = new BankStatus(this, transactionManager);

            // Subscribe handlers for Foundation events:
            // Outer context
            ActivateContextEvent += HandleActivateContextEvent;
            InactivateContextEvent += HandleInactivateContextEvent;

            // System context
            ActivateSystemExtensionContextEvent += ResetLinkLevelContextCache;

            // AscribedShell context
            NewAscribedShellContextEvent += HandleNewAscribedShellExtensionContextEvent;
            ActivateAscribedShellContextEvent += ResetLinkLevelContextCache;
            InactivateAscribedShellContextEvent += HandleInactivateAscribedShellExtensionContextEvent;

            // Theme context
            NewAscribedThemeContextEvent += HandleNewThemeExtensionContextEvent;
            ActivateAscribedThemeContextEvent += ResetLinkLevelContextCache;
            InactivateAscribedThemeContextEvent += HandleInactivateThemeExtensionContextEvent;

            // Tsm context
            ActivateTsmExtensionContextEvent += ResetLinkLevelContextCache;

            // DisplayControl
            DisplayControlEvent += HandleDisplayControlEvent;
        }

        #endregion

        #region IExtensionLib Members

        /// <inheritdoc/>
        public event EventHandler<NewContextEventArgs> NewContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateContextEventArgs> InactivateContextEvent;

        /// <inheritdoc/>
        public event EventHandler<NewSystemExtensionContextEventArgs> NewSystemExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateSystemExtensionContextEventArgs> ActivateSystemExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateSystemExtensionContextEventArgs> InactivateSystemExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<NewAscribedThemeContextEventArgs> NewAscribedThemeContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateAscribedThemeContextEventArgs> ActivateAscribedThemeContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateAscribedThemeContextEventArgs> InactivateAscribedThemeContextEvent;

        /// <inheritdoc/>
        public event EventHandler<NewAscribedShellContextEventArgs> NewAscribedShellContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateAscribedShellContextEventArgs> ActivateAscribedShellContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateAscribedShellContextEventArgs> InactivateAscribedShellContextEvent;

        /// <inheritdoc/>
        public event EventHandler<NewTsmExtensionContextEventArgs> NewTsmExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateTsmExtensionContextEventArgs> ActivateTsmExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateTsmExtensionContextEventArgs> InactivateTsmExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<SwitchThemeExtensionContextEventArgs> SwitchThemeExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<NewAppExtensionContextEventArgs> NewAppExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateAppExtensionContextEventArgs> ActivateAppExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateAppExtensionContextEventArgs> InactivateAppExtensionContextEvent;

        /// <inheritdoc/>
        public event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <inheritdoc/>
        public event EventHandler ShutDownEvent;

        /// <inheritdoc/>
        public AscribedGameEntity AscribedGameEntity => extensionLink.AscribedGameEntity;

        /// <inheritdoc/>
        public GameMode AscribedGameMode { get; private set; }

        /// <inheritdoc/>
        public DisplayControlState DisplayControlState { get; private set; }

        /// <inheritdoc/>
        public IConfigurationRead ConfigurationRead => configurationRead;

        /// <inheritdoc/>
        public ILocalizationInformation LocalizationInformation => localizationInformation;

        /// <inheritdoc/>
        public ICultureRead CultureRead => cultureRead;

        /// <inheritdoc/>
        public IExtensionParcelComm ExtensionParcelComm => extensionParcelComm;

        /// <inheritdoc/>
        public ICriticalDataAccessor CriticalDataAccessor => criticalDataAccessor;

        /// <inheritdoc/>
        public IExtensionImportCollection ExtensionImportCollection => extensionLink.ExtensionImportCollection;

        /// <inheritdoc/>
        public IGameInformation GameInformation => gameInformation;

        /// <inheritdoc/>
        public IExtensionTiltController ExtensionTiltController => extensionTiltController;

        /// <inheritdoc/>
        public IBankStatus BankStatus => bankStatus;

        /// <inheritdoc/>
        public IAppExtensionContext AppExtensionContext => extensionLink.AppExtensionContext;

        /// <inheritdoc/>
        public TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class
        {
            return extensionLink.GetInterface<TExtendedInterface>();
        }

        /// <inheritdoc/>
        public void ClearConfigurationCache()
        {
            ResetLinkLevelContextCache(null, EventArgs.Empty);
        }

        #endregion

        #region IExtensionLibRestricted Members

        /// <inheritdoc />
        public string Token => extensionLink.Token;

        /// <inheritdoc />
        public bool TransactionOpen => transactionManager.GameTransactionOpen;

        /// <inheritdoc />
        public uint LastTransactionId => extensionLink.LastTransactionId;

        /// <inheritdoc/>
        public bool ConnectToFoundation()
        {
            var isConnected = extensionLink.Connect();

            if(isConnected)
            {
                configurationRead.Initialize(
                    new ConfigurationReadLink(extensionLink.CustomConfigurationReadCategory));
                localizationInformation.Initialize(extensionLink.LocalizationCategory);
                cultureRead.Initialize(extensionLink.CultureReadCategory);
                criticalDataAccessor.Initialize(extensionLink.NonTransactionalCritDataReadCategory,
                                                extensionLink.TransactionalCritDataReadCategory,
                                                extensionLink.TransactionalCritDataWriteCategory);
                gameInformation.Initialize(extensionLink.GameInformationCategory,
                                           null,
                                           extensionLink.MountPoint);
                extensionTiltController.Initialize(extensionLink.TiltControlCategory);
                extensionParcelComm.Initialize(extensionLink.ParcelCommCategory);
                bankStatus.Initialize(extensionLink.BankStatusCategory);
            }

            return isConnected;
        }

        /// <inheritdoc/>
        public bool DisconnectFromFoundation()
        {
            extensionLink.Disconnect();
            return true;
        }

        /// <inheritdoc />
        public ErrorCode CreateTransaction()
        {
            return CreateTransaction(null);
        }

        /// <inheritdoc />
        public ErrorCode CreateTransaction(string name)
        {
            // A transaction may only be opened when the extension context is active.
            return isContextActive
                       ? transactionManager.CreateTransaction(name)
                       : ErrorCode.GeneralError;
        }

        /// <inheritdoc />
        public ErrorCode CloseTransaction()
        {
            return transactionManager.CloseTransaction();
        }
        /// <inheritdoc/>
        public WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles)
        {
            return eventCoordinator.ProcessEvents(timeout, waitHandles);
        }

        /// <inheritdoc/>
        public TServiceInterface GetServiceInterface<TServiceInterface>() where TServiceInterface : class
        {
            return builtinServices.ContainsKey(typeof(TServiceInterface))
                       ? builtinServices[typeof(TServiceInterface)] as TServiceInterface
                       : null;
        }

        #endregion

        #region ITransactionVerification Members

        /// <inheritdoc/>
        public void MustHaveOpenTransaction()
        {
            if(!transactionManager.TransactionOpen)
            {
                throw new InvalidTransactionException("No open transaction is available for this operation.");
            }
        }

        #endregion

        #region IConfigurationAccessContext Members

        /// <inheritdoc/>
        public bool IsConfigurationScopeIdentifierRequired => true;

        /// <inheritdoc/>
        void IConfigurationAccessContext.ValidateConfigurationAccess(ConfigurationScope configurationScope)
        {
            if(configurationScope != ConfigurationScope.Extension)
            {
                throw new ConfigurationAccessDeniedException(
                    $"Configuration data is not accessible for {configurationScope} scope.");
            }
        }

        #endregion

        #region ICriticalDataAccessValidation Members

        /// <inheritdoc/>
        void ICriticalDataAccessValidation.ValidateCriticalDataAccess(IList<CriticalDataSelector> criticalDataSelectors,
                                                                      DataAccessing dataAccessing)
        {
            var selectors =
                criticalDataSelectors.Where(selector =>
                                                !SupportedCriticalDataScopeTable.IsScopeAllowed(
                                                    CriticalDataScopeClientType.Extension,
                                                    selector.Scope)).ToArray();
            if(selectors.Any())
            {
                var allSelectors = string.Join(",", selectors.Select(selector => selector.Scope.ToString()).ToArray());
                throw new CriticalDataAccessDeniedException(
                    $"The critical data scope [{allSelectors}] are not accessible to extensions.");
            }
        }

        #endregion

        #region ITransactionWeightVerificationDependency Members

        /// <inheritdoc/>
        void ITransactionWeightVerificationDependency.MustHaveOpenTransaction()
        {
            MustHaveOpenTransaction();
        }

        /// <inheritdoc/>
        void ITransactionWeightVerificationDependency.MustHaveHeavyweightTransaction()
        {
            // So far we don't have lightweight transactions on the F2E connection yet.
            MustHaveOpenTransaction();
        }

        #endregion

        #region ILayeredContextActivationEventsDependency Implementation

        private static readonly object ActivateLayeredContextEventFieldLocker = new object();
        private event EventHandler<LayeredContextActivationEventArgs> ActivateLayeredContextEventField;

        /// <inheritdoc />
        event EventHandler<LayeredContextActivationEventArgs> ILayeredContextActivationEventsDependency.ActivateLayeredContextEvent
        {
            add
            {
                lock(ActivateLayeredContextEventFieldLocker)
                {
                    ActivateLayeredContextEventField += value;
                }
            }

            remove
            {
                lock(ActivateLayeredContextEventFieldLocker)
                {
                    ActivateLayeredContextEventField -= value;
                }
            }
        }

        private static readonly object InactivateLayeredContextEventFieldLocker = new object();
        private event EventHandler<LayeredContextActivationEventArgs> InactivateLayeredContextEventField;

        /// <inheritdoc />
        event EventHandler<LayeredContextActivationEventArgs> ILayeredContextActivationEventsDependency.InactivateLayeredContextEvent
        {
            add
            {
                lock(InactivateLayeredContextEventFieldLocker)
                {
                    InactivateLayeredContextEventField += value;
                }
            }

            remove
            {
                lock(InactivateLayeredContextEventFieldLocker)
                {
                    InactivateLayeredContextEventField -= value;
                }
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Parses the command line arguments specified by the Foundation.
        /// </summary>
        /// <param name="address">Returns the Foundation address parsed.</param>
        /// <param name="port">Returns the Foundation port parsed.</param>
        private static void ParseFoundationArguments(out string address, out ushort port)
        {
            // '-s' means socket communication, which is the only mode supported.
            var socketArgument = CommandLineArguments.Environment.GetValue("s");

            if(socketArgument == null)
            {
                throw new InvalidOperationException("Missing command line argument for socket communication.");
            }

            // Address and port are in 'address:port' format.
            var argValues = socketArgument.Split(':');
            address = argValues[0];

            if(argValues.Length > 1)
            {
                port = ushort.TryParse(argValues[1], out var k) ? k : (ushort)0;
            }
            else
            {
                port = 0;
            }
        }

        #endregion

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        /// <remarks>
        /// The following events don't go to event table, since they are raised directly without going into event queue.
        /// <list type="bullet">
        ///     <item>ShutDownEvent</item>
        /// </list>
        /// </remarks>
        private void CreateEventLookupTable()
        {
            eventTable[typeof(NewContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewContextEvent);

            eventTable[typeof(ActivateContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ActivateContextEvent);

            eventTable[typeof(InactivateContextEventArgs)] =
                (sender, eventArgs) => ExecuteLayeredContextActivation(sender, eventArgs,
                                                                       InactivateContextEvent,
                                                                       InactivateLayeredContextEventField, ContextLayer.Application);

            eventTable[typeof(NewSystemExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewSystemExtensionContextEvent);

            eventTable[typeof(ActivateSystemExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteLayeredContextActivation(sender, eventArgs,
                                                                       ActivateSystemExtensionContextEvent,
                                                                       ActivateLayeredContextEventField, ContextLayer.SystemExtension);

            eventTable[typeof(InactivateSystemExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteLayeredContextActivation(sender, eventArgs,
                                                                       InactivateSystemExtensionContextEvent,
                                                                       InactivateLayeredContextEventField, ContextLayer.SystemExtension);

            eventTable[typeof(NewAscribedShellContextEventArgs)] =
                (sender, eventArgs) => InterceptNewAscribedShellContext(sender, eventArgs as NewAscribedShellContextEventArgs);
            eventTable[typeof(ActivateAscribedShellContextEventArgs)] =
                (sender, eventArgs) => InterceptActivateAscribedShellContext(sender, eventArgs as ActivateAscribedShellContextEventArgs);
            eventTable[typeof(InactivateAscribedShellContextEventArgs)] =
                (sender, eventArgs) => InterceptInactivateAscribedShellContext(sender, eventArgs as InactivateAscribedShellContextEventArgs);

            eventTable[typeof(NewAscribedThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewAscribedThemeContextEvent);
            eventTable[typeof(ActivateAscribedThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ActivateAscribedThemeContextEvent);
            eventTable[typeof(SwitchThemeExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, SwitchThemeExtensionContextEvent);
            eventTable[typeof(InactivateAscribedThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, InactivateAscribedThemeContextEvent);
            eventTable[typeof(NewTsmExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewTsmExtensionContextEvent);
            eventTable[typeof(ActivateTsmExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ActivateTsmExtensionContextEvent);
            eventTable[typeof(InactivateTsmExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, InactivateTsmExtensionContextEvent);
            eventTable[typeof(NewAppExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewAppExtensionContextEvent);
            eventTable[typeof(ActivateAppExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ActivateAppExtensionContextEvent);
            eventTable[typeof(InactivateAppExtensionContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, InactivateAppExtensionContextEvent);
            eventTable[typeof(DisplayControlEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, DisplayControlEvent);
        }

        /// <summary>
        /// This function is a generic way to invoke an event handler.
        /// </summary>
        /// <param name="sender">The object posting the event.</param>
        /// <param name="eventArgs">The event payload.</param>
        /// <param name="eventHandler">The handler for the event.</param>
        private static void ExecuteHandler<TEventArgs>(object sender, EventArgs eventArgs,
                                                       EventHandler<TEventArgs> eventHandler)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// This function is a generic way to invoke a context activation event handler.
        /// </summary>
        /// <param name="sender">The object posting the event.</param>
        /// <param name="eventArgs">The event payload.</param>
        /// <param name="eventHandler">The handler for the event.</param>
        /// <param name="levelEventHandler">The level context activation event handler.</param>
        /// <param name="contextLayer">The level of the context.</param>
        private static void ExecuteLayeredContextActivation<TEventArgs>(object sender, EventArgs eventArgs,
                                                                        EventHandler<TEventArgs> eventHandler,
                                                                        EventHandler<LayeredContextActivationEventArgs> levelEventHandler,
                                                                        ContextLayer contextLayer)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
            levelEventHandler?.Invoke(sender, new LayeredContextActivationEventArgs(contextLayer));
        }

        /// <summary>
        /// Intercepts a NewAscribedShellContextEvent and replaces it with a new instance with more information.
        /// The original event is constructed by category callback handler who does not have the knowledge
        /// to fill out some of the fields in the event, but here Extension Lib does.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The original event arguments.</param>
        private void InterceptNewAscribedShellContext(object sender, NewAscribedShellContextEventArgs eventArgs)
        {
            var newEventArgs = new NewAscribedShellContextEventArgs(eventArgs.GameMode, extensionLink.AscribedGameEntity);

            ExecuteHandler(sender, newEventArgs, NewAscribedShellContextEvent);
        }

        /// <summary>
        /// Intercepts an ActivateAscribedShellContextEvent event and replaces it with a new instance with more information.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The original event arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private void InterceptActivateAscribedShellContext(object sender, ActivateAscribedShellContextEventArgs eventArgs)
        {
            var newEventArgs = new ActivateAscribedShellContextEventArgs(extensionLink.AscribedGameEntity);

            ExecuteHandler(sender, newEventArgs, ActivateAscribedShellContextEvent);
        }

        /// <summary>
        /// Intercepts an InactivateAscribedShellContextEvent and replaces it with a new instance with more information.
        /// The original event is constructed by category callback handler who does not have the knowledge
        /// to fill out some of the fields in the event, but here Extension Lib does.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The original event arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private void InterceptInactivateAscribedShellContext(object sender, InactivateAscribedShellContextEventArgs eventArgs)
        {
            var newEventArgs = new InactivateAscribedShellContextEventArgs(extensionLink.AscribedGameEntity);

            ExecuteHandler(sender, newEventArgs, InactivateAscribedShellContextEvent);
        }

        #endregion

        #region ExtensionLink Event Handlers

        /// <summary>
        /// Actions performed when the Foundation shuts down the connection.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleLinkShutDown(object sender, EventArgs eventArgs)
        {
            ShutDownEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region TransactionManager Event Handlers

        /// <summary>
        /// Actions performed when a Foundation event needs processing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventDispatchedEvent(object sender, EventDispatchedEventArgs eventArgs)
        {
            if(eventTable.TryGetValue(eventArgs.DispatchedEventType, out var handler))
            {
                handler(this, eventArgs.DispatchedEvent);
                eventArgs.IsHandled = true;
            }
        }

        #endregion

        #region Category Callback Event Handlers

        /// <summary>
        /// Resets the Link level context caches.
        /// The config data of Link level categories could change while the link level context is being active.
        /// </summary>
        /// <param name="sender">Not uses.</param>
        /// <param name="eventArgs">Not used</param>
        private void ResetLinkLevelContextCache(object sender, EventArgs eventArgs)
        {
            // Clear cache of custom configuration items.
            configurationRead.ClearCache();

            // Clear config data of all Link level categories.
            localizationInformation.NewContext();

            // Notify link level interface extensions.
            ActivateLayeredContextEventField?.Invoke(sender, new LayeredContextActivationEventArgs(ContextLayer.Application));
        }

        /// <summary>
        /// Handles the event for when the extension is activated for a new context.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleActivateContextEvent(object sender, ActivateContextEventArgs eventArgs)
        {
            ResetLinkLevelContextCache(sender, eventArgs);
            isContextActive = true;
        }

        /// <summary>
        /// Handles the event for when the extension is inactivated.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleInactivateContextEvent(object sender, InactivateContextEventArgs eventArgs)
        {
            isContextActive = false;

            // Clear any pending game transaction. The foundation discards existing
            // requests when inactivating a context.
            transactionManager.ClearPendingTransaction();

            // Clear the tilt info when the extension goes inactive
            extensionTiltController.ClearTiltInfo();
        }

        /// <summary>
        /// Handles the event for when the extension is placed in a new AscribedShell-extension context.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleNewAscribedShellExtensionContextEvent(object sender, NewAscribedShellContextEventArgs eventArgs)
        {
            AscribedGameMode = eventArgs.GameMode;
        }

        /// <summary>
        /// Handles the event for when the extension should inactivate the AscribedShell-extension context
        /// and disables parcel communication.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleInactivateAscribedShellExtensionContextEvent(object sender, InactivateAscribedShellContextEventArgs eventArgs)
        {
            AscribedGameMode = GameMode.Invalid;
        }

        /// <summary>
        /// Handles the event for when the extension is placed in a new theme-extension context.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleNewThemeExtensionContextEvent(object sender, NewAscribedThemeContextEventArgs eventArgs)
        {
            AscribedGameMode = eventArgs.GameMode;
        }

        /// <summary>
        /// Handles the event for when the extension should inactivate the theme-extension context
        /// and disables parcel communication.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleInactivateThemeExtensionContextEvent(object sender, InactivateAscribedThemeContextEventArgs eventArgs)
        {
            AscribedGameMode = GameMode.Invalid;
        }

        /// <summary>
        /// Handles the event when the display control state is changed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleDisplayControlEvent(object sender, DisplayControlEventArgs eventArgs)
        {
            DisplayControlState = eventArgs.DisplayControlState;
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    // Clean up event source registrations.
                    eventCoordinator.UnregisterEventSource(nonTransactionalEventManager);

                    // Dispose internal managers.
                    transactionManager.Dispose();
                    nonTransactionalEventManager.Dispose();
                    extensionLink.Dispose();

                    disposed = true;
                }

                disposed = true;
            }
        }

        #endregion
    }
}
