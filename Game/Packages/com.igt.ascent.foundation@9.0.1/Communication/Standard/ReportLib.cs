//-----------------------------------------------------------------------
// <copyright file = "ReportLib.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2XCallbacks;
    using F2XLinks;
    using GameReport.Interfaces;

    /// <summary>
    /// Standard implementation of <see cref="IReportLib"/> that
    /// communicates with the Foundation for game reporting.
    /// </summary>
    public class ReportLib : IReportLib, IReportLibRestricted,
                             ITransactionVerification, IConfigurationAccessContext,
                             ICriticalDataAccessValidation, IDisposable
    {
        #region Fields

        /// <summary>
        /// Object used to communicate with the foundation.
        /// </summary>
        private readonly ReportLink reportLink;

        /// <summary>
        /// Object used to manage game initiated transactions and Foundation events.
        /// </summary>
        private readonly TransactionManager transactionManager;

        /// <summary>
        /// Object used to manage Foundation non transactional events.
        /// </summary>
        private readonly NonTransactionalEventManager nonTransactionalEventManager;

        /// <summary>
        /// Object used to process events.
        /// </summary>
        private readonly EventCoordinator eventCoordinator;

        /// <summary>
        /// Object used to request localization information.
        /// </summary>
        private readonly LocalizationInformation localizationInformation;

        /// <summary>
        /// Object used to request EGM config data.
        /// </summary>
        private readonly EgmConfigData egmConfigData;

        /// <summary>
        /// Object used to request information about custom configuration items.
        /// </summary>
        private readonly ConfigurationRead configurationRead;

        /// <summary>
        /// Object used to access critical data.
        /// </summary>
        private readonly CriticalDataAccessor criticalDataAccessor;

        /// <summary>
        /// Object used to query game information.
        /// </summary>
        private readonly GameInformation gameInformation;

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// Create the event lookup table.
        /// </summary>
        /// <remarks>
        /// The following events don't go to event table, since
        /// they are raised directly without going into event queue.
        /// <list type="bullet">
        ///     <item>ShutDownEvent</item>
        /// </list>
        /// </remarks>
        private void CreateEventLookupTable()
        {
            // Create the look up table in alphabetical order.
            eventTable[typeof(ActivateContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ActivateContextEvent);

            eventTable[typeof(GenerateGamePerformanceReportEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, GenerateGamePerformanceReportEvent);

            eventTable[typeof(GenerateInspectionReportEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, GenerateInspectionReportEvent);

            eventTable[typeof(GenerateHtmlInspectionReportEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, GenerateHtmlInspectionReportEvent);

            eventTable[typeof(GetInspectionReportTypeEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, GetInspectionReportTypeEvent);

            eventTable[typeof(GetGameLevelValuesEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, GetGameLevelValuesEvent);

            eventTable[typeof(InactivateContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, InactivateContextEvent);

            eventTable[typeof(InitializeGameLevelDataEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, InitializeGameLevelDataEvent);
 
            eventTable[typeof(ValidateThemeSetupEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ValidateThemeSetupEvent);

            eventTable[typeof(GetMinPlayableCreditBalanceEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, GetMinPlayableCreditBalanceEvent);
        }

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ReportLib"/> that
        /// targets a specific version of Foundation.
        /// </summary>
        /// <param name="foundationTarget">Foundation version to target.</param>
        public ReportLib(FoundationTarget foundationTarget)
        {
            ParseFoundationArguments(out var address, out var port);

            // Create the non transaction manager.
            nonTransactionalEventManager = new NonTransactionalEventManager();
            nonTransactionalEventManager.EventDispatchedEvent += HandleEventDispatched;

            // Create the report link.
            reportLink = new ReportLink(address, port, foundationTarget, nonTransactionalEventManager);
            reportLink.LinkShutDownEvent += HandleLinkShutDown;

            CreateEventLookupTable();

            // Create the transaction manager.
            transactionManager = new TransactionManager(reportLink, reportLink.TransportExceptionMonitor);
            transactionManager.EventDispatchedEvent += HandleEventDispatched;

            // Create the event coordinator.
            eventCoordinator = new EventCoordinator(transactionManager, reportLink.TransportExceptionMonitor);

            // Register other event sources with the event coordinator.
            eventCoordinator.RegisterEventSource(nonTransactionalEventManager);

            // Create interface implementations.
            localizationInformation = new LocalizationInformation(this);
            egmConfigData = new EgmConfigData(this);
            configurationRead = new ConfigurationRead(this, this, true);
            criticalDataAccessor = new CriticalDataAccessor(this);
            gameInformation = new GameInformation(this);

            // Subscribe handlers for Foundation events.
            ActivateContextEvent += HandleActivateContextEvent;
            GenerateGamePerformanceReportEvent += HandleGenerateGamePerformanceReportEvent;
            GenerateInspectionReportEvent += HandleGenerateInspectionReportEvent;
        }

        #endregion

        #region IReportLibRestricted Members

        /// <inheritdoc/>
        public bool ConnectToFoundation(IEnumerable<ReportingServiceType> reportingServiceTypes)
        {
            if(reportingServiceTypes == null)
            {
                throw new ArgumentNullException(nameof(reportingServiceTypes));
            }

            var connected = reportLink.Connect(reportingServiceTypes);

            if(connected)
            {
                // Initialize interface implementations with installed category handlers.
                localizationInformation.Initialize(reportLink.Localization);
                egmConfigData.Initialize(reportLink.EgmConfigData);
                configurationRead.Initialize(new ConfigurationReadLink(reportLink.CustomConfigurationRead));
                criticalDataAccessor.Initialize(reportLink.NonTransactionalCritDataRead,
                                                reportLink.TransactionalCritDataRead,
                                                reportLink.TransactionalCritDataWrite);
                gameInformation.Initialize(reportLink.GameInformation, reportLink.GameGroupInformation, MountPoint);
            }

            return connected;
        }

        /// <inheritdoc/>
        public bool DisconnectFromFoundation()
        {
            reportLink.Disconnect();
            return true;
        }

        /// <inheritdoc/>
        public WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles)
        {
            return eventCoordinator.ProcessEvents(timeout, waitHandles);
        }

        #endregion

        #region IReportLib Members

        /// <inheritdoc/>
        public event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateContextEventArgs> InactivateContextEvent;

        /// <inheritdoc/>
        public event EventHandler<GenerateInspectionReportEventArgs> GenerateInspectionReportEvent;

        /// <inheritdoc/>
        public event EventHandler<GenerateHtmlInspectionReportEventArgs> GenerateHtmlInspectionReportEvent;   

        /// <inheritdoc/>
        public event EventHandler<GetInspectionReportTypeEventArgs> GetInspectionReportTypeEvent; 

        /// <inheritdoc/>
        public event EventHandler<InitializeGameLevelDataEventArgs> InitializeGameLevelDataEvent;

        /// <inheritdoc/>
        public event EventHandler<GetGameLevelValuesEventArgs> GetGameLevelValuesEvent;

        /// <inheritdoc/>
        public event EventHandler<ValidateThemeSetupEventArgs> ValidateThemeSetupEvent;

        /// <inheritdoc/>
        public event EventHandler<GenerateGamePerformanceReportEventArgs> GenerateGamePerformanceReportEvent;

        /// <inheritdoc/>
        public event EventHandler<GetMinPlayableCreditBalanceEventArgs> GetMinPlayableCreditBalanceEvent;

        /// <inheritdoc/>
        public event EventHandler ShutDownEvent;

        /// <inheritdoc/>
        public string MountPoint => reportLink.MountPoint;

        /// <inheritdoc/>
        public string Jurisdiction => reportLink.Jurisdiction;

        /// <inheritdoc/>
        public ILocalizationInformation LocalizationInformation => localizationInformation;

        /// <inheritdoc/>
        public IEgmConfigData EgmConfigData => egmConfigData;

        /// <inheritdoc/>
        public IConfigurationRead ConfigurationRead => configurationRead;

        /// <inheritdoc/>
        public ICriticalDataAccessor CriticalDataAccessor => criticalDataAccessor;

        /// <inheritdoc/>
        public IGameInformation GameInformation => gameInformation;

        /// <inheritdoc/>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when the communication category required is not valid.
        /// </exception>
        public IDictionary<int, ProgressiveSettings> GetLinkedProgressiveSettings(string paytableIdentifier,
                                                                                  long denomination)
        {
            MustHaveOpenTransaction();

            if(reportLink.ProgressiveData == null)
            {
                throw new CommunicationInterfaceUninitializedException("Progressive Data Category is not valid.");
            }

            var settings = reportLink.ProgressiveData.QueryPayvarBasedProgressiveSettings(paytableIdentifier.ToPayvarIdentifier(),
                                                                             new List<uint> { (uint)denomination });

            var result = new Dictionary<int, ProgressiveSettings>();

            var denomSetting = settings.First(denom => denom.Denomination == denomination);

            if(denomSetting.ProgressiveLinkedGameLevelSettings != null)
            {
                // When building the result, we don't care if the denomination is enabled or not.
                result = denomSetting.ProgressiveLinkedGameLevelSettings
                                        .ToDictionary(level => (int)level.GameLevel,
                                                    level => level.ProgressiveSettings.ToPublic());
            }

            return result;
        }

        #endregion

        #region ITransactionVerification Members

        /// <inheritdoc/>
        void ITransactionVerification.MustHaveOpenTransaction()
        {
            MustHaveOpenTransaction();
        }

        #endregion

        #region IConfigurationAccessContext Members

        /// <inheritdoc/>
        public bool IsConfigurationScopeIdentifierRequired => true;

        /// <inheritdoc/>
        void IConfigurationAccessContext.ValidateConfigurationAccess(ConfigurationScope configurationScope)
        {
            if(configurationScope != ConfigurationScope.Theme &&
               configurationScope != ConfigurationScope.Payvar)
            {
                throw new ConfigurationAccessDeniedException($"Configuration data is not accessible for {configurationScope} scope.");
            }
        }

        #endregion

        #region ICriticalDataAccessValidation Members

        void ICriticalDataAccessValidation.ValidateCriticalDataAccess(
            IList<CriticalDataSelector> criticalDataSelectors, DataAccessing dataAccessing)
        {
            if(dataAccessing != DataAccessing.Read)
            {
                throw new CriticalDataAccessDeniedException($"The accessing {dataAccessing} is not supported in reports.");
            }

            var selectors =
                criticalDataSelectors.Where(selector =>
                                                !SupportedCriticalDataScopeTable.IsScopeAllowed(
                                                    CriticalDataScopeClientType.Report,
                                                    selector.Scope)).ToArray();
            if(selectors.Any())
            {
                throw new CriticalDataAccessDeniedException($"The critical data scope [{string.Join(",", selectors.Select(selector => selector.Scope.ToString()).ToArray())}] are not accessible to reports.");
            }
        }

        #endregion

        #region Private Methods

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
        /// Check if an open transaction is available for the operation.
        /// Should be called by all IReportLib methods.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when there is no open transaction available.
        /// </exception>
        private void MustHaveOpenTransaction()
        {
            if(!transactionManager.TransactionOpen)
            {
                throw new InvalidTransactionException("No open transaction is available for this operation.");
            }
        }

        #region ReportLink Event Handlers

        /// <summary>
        /// Actions performed when the Foundation shuts down the connection.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleLinkShutDown(object sender, EventArgs eventArgs)
        {
            var handler = ShutDownEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Event Dispatcher Handlers

        /// <summary>
        /// Actions performed when a Foundation event needs processing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventDispatched(object sender, EventDispatchedEventArgs eventArgs)
        {
            if(eventTable.ContainsKey(eventArgs.DispatchedEventType))
            {
                var handler = eventTable[eventArgs.DispatchedEventType];
                if(handler != null)
                {
                    handler(this, eventArgs.DispatchedEvent);

                    eventArgs.IsHandled = true;
                }
            }
        }

        #endregion

        #region Foundation Event Handlers

        /// <summary>
        /// Handles the event for when the report object is activated for a new context.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Event data.</param>
        private void HandleActivateContextEvent(object sender, ActivateContextEventArgs eventArgs)
        {
            configurationRead.ClearCache();
            egmConfigData.NewContext();
            localizationInformation.NewContext();
        }

        /// <summary>
        /// Handles the event for when the report object is requested to provide a game performance report.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleGenerateGamePerformanceReportEvent(object sender,
                                                              GenerateGamePerformanceReportEventArgs eventArgs)
        {
        }

        /// <summary>
        /// Handles the event for when the report object is requested to provide an inspection report.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleGenerateInspectionReportEvent(object sender, GenerateInspectionReportEventArgs eventArgs)
        {
            // ThemeIdentifier was not added to GenerateInspectionReport until 1.1.
            if(string.IsNullOrEmpty(eventArgs.ThemeIdentifier))
            {
                // Modify event data so all clients can access correct value.
                eventArgs.ThemeIdentifier = reportLink.ThemeIdentifier;
            }
        }
       
        #endregion

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
            if(!disposed && disposing)
            {
                eventCoordinator.UnregisterEventSource(nonTransactionalEventManager);
                transactionManager.Dispose();
                nonTransactionalEventManager.Dispose();
                reportLink.Dispose();

                disposed = true;
            }
        }

        #endregion
    }
}
