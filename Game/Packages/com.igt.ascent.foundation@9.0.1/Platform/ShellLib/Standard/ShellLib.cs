// -----------------------------------------------------------------------
// <copyright file = "ShellLib.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using F2XLinks;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Transport.Sessions;
    using Game.Core.Threading;
    using IGT.Ascent.Communication.Platform.Standard;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// Standard implementation of interfaces for a shell to communicate with the Foundation.
    /// </summary>
    public sealed class ShellLib : StandardAppLibBase, IShellLibRestricted, IShellLib, IGameModeQuery
    {
        #region Private Fields

        private readonly ShellLink shellLink;

        /// <summary>
        /// The cached interface for accessing shell's critical data.
        /// </summary>
        private readonly ShellStore shellStore;

        /// <summary>
        /// The cached interface for accessing shell's history critical data in play mode.
        /// </summary>
        private readonly ShellHistoryStore shellHistoryStore;

        /// <summary>
        /// The cached interface for the shell to parcel communicate with the extension.
        /// </summary>
        private readonly GameParcelComm gameParcelComm;

        /// <summary>
        /// The cached interface for the bank play.
        /// </summary>
        private readonly BankPlay bankPlay;

        /// <summary>
        /// The cached interface for the game play status.
        /// </summary>
        private readonly GamePlayStatus gamePlayStatus;

        /// <summary>
        /// The cached interface for the game presentation behavior.
        /// </summary>
        private readonly GamePresentationBehavior gamePresentationBehavior;

        /// <summary>
        /// The cached interface for the localization.
        /// </summary>
        private readonly Localization<IShellContext> localization;

        /// <summary>
        /// The cached interface for the game culture.
        /// </summary>
        private readonly GameCulture gameCulture;

        /// <summary>
        /// The cached interface for accessing chooser services.
        /// </summary>
        private readonly ChooserServices<IShellContext> chooserServices;

        /// <summary>
        /// The cached interface for accessing show demo functionality.
        /// </summary>
        private readonly ShowDemo showDemo;

        /// <summary>
        /// The cached interface for accessing shell's history context and critical data in history mode.
        /// </summary>
        private readonly ShellHistoryControl shellHistoryControl;

        /// <summary>
        /// The tilt controller used for managing tilts.
        /// </summary>
        private readonly ShellTiltController shellTiltController;

        /// <summary>
        /// The pending shell context to be activated later.
        /// </summary>
        private ShellContext pendingContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellLib"/>.
        /// </summary>
        /// <param name="foundationTarget">
        /// The foundation version to target.
        /// </param>
        /// <param name="transportSession">
        /// The transport session for this shell lib to use to communicate with the Foundation.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// List of additional interface configurations to install in this shell lib.
        /// </param>
        /// <remarks>
        /// Requested interface configurations may not be available if not supported by the underlying platform.
        /// </remarks>
        public ShellLib(FoundationTarget foundationTarget,
                        ISession transportSession,
                        IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations)
        {
            Context = new ShellContext();

            var transactionVerifier = new TransactionVerifier(TransEventQueue);

            // Set up connection.
            var baseExtensionDependencies = new InterfaceExtensionDependencies
                                                {
                                                    TransactionWeightVerification = transactionVerifier,
                                                    TransactionalEventDispatcher = TransEventQueue,
                                                    NonTransactionalEventDispatcher = NonTransEventQueue,
                                                    LayeredContextActivationEvents = this,
                                                    GameModeQuery = this
                                                };

            shellLink = new ShellLink(foundationTarget, transportSession,
                                      baseExtensionDependencies: baseExtensionDependencies,
                                      additionalInterfaceConfigurations: interfaceExtensionConfigurations,
                                      eventCallbacks: TransEventQueue,
                                      nonTransactionalEventCallbacks: NonTransEventQueue);
            DisposableCollection.Add(shellLink);

            shellLink.LinkShutDownEvent += HandleLinkShutDown;

            // Create built-in interface implementations.
            gameParcelComm = new GameParcelComm(this, TransEventQueue, NonTransEventQueue);
            bankPlay = new BankPlay(this, TransEventQueue, NonTransEventQueue);
            gamePlayStatus = new GamePlayStatus(this, TransEventQueue);
            gamePresentationBehavior = new GamePresentationBehavior();
            localization = new Localization<IShellContext>();
            gameCulture = new GameCulture(this, TransEventQueue);
            chooserServices = new ChooserServices<IShellContext>(this, NonTransEventQueue);
            showDemo = new ShowDemo();
            shellHistoryControl = new ShellHistoryControl();
            shellTiltController = new ShellTiltController(transactionVerifier, TransEventQueue);

            // Create critical data stores.
            var gameEntityStoreAccessValidator = new GameEntityStoreAccessValidator(this);
            shellStore = new ShellStore(gameEntityStoreAccessValidator);
            shellHistoryStore = new ShellHistoryStore(this, TransEventQueue, gameEntityStoreAccessValidator);

            // Add critical data stores to collection.  No need to check types.  The collection object will do it.
            CachedStoreCollection.Add(shellStore);
            CachedStoreCollection.Add(shellHistoryStore);

            // Create event table.
            CreateEventTable();

            // Subscribe event handlers.
            DisplayControlEvent += HandleDisplayControl;
            NewShellContextEvent += HandleNewShellContext;
            ActivateShellContextEvent += HandleActivateShellContext;
            InactivateShellContextEvent += HandleInactivateShellContext;
        }

        #endregion

        #region IShellLibRestricted Implementation

        /// <inheritdoc/>
        public event EventHandler<ActionResponseEventArgs> ActionResponseEvent;

        /// <inheritdoc/>
        public event EventHandler<ActionResponseLiteEventArgs> ActionResponseLiteEvent;

        /// <inheritdoc/>
        public event EventHandler<NewShellContextEventArgs> NewShellContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ActivateShellContextEventArgs> ActivateShellContextEvent;

        /// <inheritdoc/>
        public event EventHandler<InactivateShellContextEventArgs> InactivateShellContextEvent;

        /// <inheritdoc/>
        public event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <inheritdoc/>
        public event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <inheritdoc/>
        public event EventHandler<ParkEventArgs> ParkEvent;

        /// <inheritdoc/>
        public string Token => shellLink.Token;

        /// <inheritdoc/>
        public string MountPoint => shellLink.MountPoint;

        /// <inheritdoc/>
        public IExceptionMonitor ExceptionMonitor => shellLink.TransportExceptionMonitor;

        /// <inheritdoc/>
        public DisplayControlState DisplayControlState { get; private set; }
        
        /// <inheritdoc/>
        public IShellHistoryStore ShellHistoryStore => shellHistoryStore;

        /// <inheritdoc />
        public IShellHistoryControl ShellHistoryControl => shellHistoryControl;

        /// <inheritdoc/>
        public bool ConnectToFoundation()
        {
            var connected = shellLink.Connect();
            if(connected)
            {
                shellStore.Initialize(shellLink.ShellStoreCategory);
                shellHistoryStore.Initialize(shellLink.ShellHistoryStoreCategory);
                gameParcelComm.Initialize(shellLink.ParcelCommCategory);
                bankPlay.Initialize(shellLink.BankPlayCategory);
                gamePlayStatus.Initialize(shellLink.GamePlayStatusCategory);
                gamePresentationBehavior.Initialize(shellLink.GamePresentationBehaviorCategory);
                localization.Initialize(shellLink.LocalizationCategory);
                gameCulture.Initialize(shellLink.CultureReadCategory);
                chooserServices.Initialize(shellLink.ChooserServicesCategory);
                showDemo.Initialize(shellLink.ShowDemoCategory);
                shellHistoryControl.Initialize(shellLink.ShellHistoryControlCategory);
                shellTiltController.Initialize(shellLink.TiltControlCategory);
            }

            return connected;
        }

        /// <inheritdoc/>
        public IList<ShellThemeInfo> GetSelectableThemes()
        {
            var replyList = shellLink.ShellThemeControlCategory.GetSelectableThemes();

            return replyList == null
                       ? new List<ShellThemeInfo>()
                       : replyList.Select(entry => new ShellThemeInfo(entry.ThemeIdentifier.ToToken(),
                                                                      entry.G2SThemeIdentifier,
                                                                      entry.ThemeTag,
                                                                      entry.ThemeTagDataFile,
                                                                      entry.Denoms.ConvertAll(
                                                                          d => new ShellThemeDenomInfo(d.Value, d.IsProgressiveDenom)),
                                                                      entry.DefaultDenom))
                                  .ToList();
        }

        /// <inheritdoc/>
        public IList<CoplayerInfo> GetCoplayers()
        {
            var replyList = shellLink.CoplayerManagementCategory.GetCoplayers();

            return replyList == null
                       ? new List<CoplayerInfo>()
                       : replyList.Select(entry => entry.ThemeSelector == null
                                                       ? new CoplayerInfo(entry.Coplayer, null, 0)
                                                       : new CoplayerInfo(entry.Coplayer,
                                                                          entry.ThemeSelector.ThemeIdentifier.ToToken(),
                                                                          entry.ThemeSelector.Denom))
                                  .ToList();
        }

        /// <inheritdoc/>
        public ShellThemeInfo GetThemeInformation(IdentifierToken themeIdentifier)
        {
            var themeInfo = shellLink.ShellThemeControlCategory.GetThemeInformation(themeIdentifier.ToThemeIdentifier());

            return new ShellThemeInfo(themeInfo.ThemeIdentifier.ToToken(),
                                      themeInfo.G2SThemeIdentifier,
                                      themeInfo.ThemeTag,
                                      themeInfo.ThemeTagDataFile,
                                      themeInfo.Denoms.ConvertAll(d => new ShellThemeDenomInfo(d.Value, d.IsProgressiveDenom)), themeInfo.DefaultDenom);
        }

        /// <inheritdoc/>
        public IList<int> CreateCoplayers(int coplayerCount)
        {
            return shellLink.CoplayerManagementCategory.CreateCoplayers(coplayerCount).ToList();
        }

        /// <inheritdoc/>
        public bool SwitchCoplayerTheme(int coplayerId, IdentifierToken themeIdentifier, long denomination)
        {
            return shellLink.ShellThemeControlCategory.SwitchTheme(coplayerId,
                                                                   new Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types.ThemeSelector
                                                                       {
                                                                           ThemeIdentifier = themeIdentifier.ToThemeIdentifier(),
                                                                           Denom = (uint)denomination,
                                                                       });
        }

        /// <inheritdoc/>
        public bool RemoveCoplayerTheme(int coplayerId)
        {
            return shellLink.ShellThemeControlCategory.SwitchTheme(coplayerId,
                                                                   null);
        }

        /// <inheritdoc/>
        public int CreateSession()
        {
            var reply = shellLink.SessionManagementCategory.CreateSession();

            if(!reply.SessionSpecified)
            {
                throw new ApplicationException("There is no Session Id specified in the F2X CreateSession reply");
            }

            return reply.Session;
        }

        /// <inheritdoc/>
        public void BindCoplayerSession(int coplayerId, int sessionId)
        {
            shellLink.CoplayerManagementCategory.BindCoplayerSession(coplayerId, sessionId);
        }

        /// <inheritdoc/>
        public void LaunchSession(int sessionId)
        {
            shellLink.SessionManagementCategory.LaunchSession(sessionId);
        }

        /// <inheritdoc/>
        public void DestroySession(int sessionId)
        {
            shellLink.SessionManagementCategory.DestroySession(sessionId);
        }

        /// <inheritdoc/>
        public void ActionRequest(string transactionName)
        {
            var payload = transactionName == null
                              ? new byte[0]
                              : Encoding.ASCII.GetBytes(transactionName);

            shellLink.ActionRequestCategory.ActionRequest(payload);
        }

        /// <inheritdoc/>
        public void ActionRequestLite(string transactionName)
        {
            var payload = transactionName == null
                              ? new byte[0]
                              : Encoding.ASCII.GetBytes(transactionName);

            shellLink.ActionRequestLiteCategory.ActionRequestLite(payload);
        }

        #endregion

        #region IShellLib Implementation

        /// <inheritdoc/>
        public IShellContext Context { get; private set; }

        /// <inheritdoc/>
        public int MaxNumCoplayers { get; private set; }

        /// <inheritdoc/>
        public ICriticalDataStore ShellStore => shellStore;

        /// <inheritdoc/>
        public IGameParcelComm GameParcelComm => gameParcelComm;

        /// <inheritdoc/>
        public IBankPlay BankPlay => bankPlay;

        /// <inheritdoc/>
        public IGamePlayStatus GamePlayStatus => gamePlayStatus;

        /// <inheritdoc/>
        public IGamePresentationBehavior GamePresentationBehavior => gamePresentationBehavior;

        /// <inheritdoc/>
        public ILocalization Localization => localization;

        /// <inheritdoc/>
        public IGameCulture GameCulture => gameCulture;

        /// <inheritdoc/>
        public IChooserServices ChooserServices => chooserServices;

        /// <inheritdoc/>
        public IShowDemo ShowDemo => showDemo;

        /// <inheritdoc />
        public IExtensionImportCollection ExtensionImportCollection => shellLink.ExtensionImportCollection;

        /// <inheritdoc/>
        public IShellTiltController TiltController => shellTiltController;

        /// <inheritdoc/>
        public TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class
        {
            return shellLink.GetInterface<TExtendedInterface>();
        }

        #endregion

        #region IGameModeQuery Implementation

        /// <inheritdoc/>
        public GameMode GameMode => Context.GameMode;

        #endregion

        #region Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventTable()
        {
            // Events should be listed in alphabetical order!

            EventTable[typeof(ActionResponseEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActionResponseEvent);

            EventTable[typeof(ActionResponseLiteEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActionResponseLiteEvent);

            EventTable[typeof(ActivateShellContextEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActivateShellContextEvent);

            EventTable[typeof(DisplayControlEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, DisplayControlEvent);

            EventTable[typeof(InactivateShellContextEventArgs)] =
                (sender, eventArgs) => InterceptInactivateShellContext(sender, eventArgs as InactivateShellContextEventArgs);

            EventTable[typeof(NewShellContextEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, NewShellContextEvent);

            EventTable[typeof(ShutDownEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ShutDownEvent);

            EventTable[typeof(ParkEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ParkEvent);
        }

        /// <summary>
        /// Intercepts an Inactive Shell Context event and replaces it with a new instance with more information.
        /// The original event is constructed by category callback handler who does not have the knowledge
        /// to fill out some of the fields in the event, but here Shell Lib does.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The original event arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private void InterceptInactivateShellContext(object sender, InactivateShellContextEventArgs eventArgs)
        {
            var newEventArgs = new InactivateShellContextEventArgs(new ShellContext(Context));

            ExecuteEventHandler(sender, newEventArgs, InactivateShellContextEvent);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Actions performed when the Foundation shuts down the connection.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleLinkShutDown(object sender, EventArgs eventArgs)
        {
            RaiseEvent(this, typeof(ShutDownEventArgs), new ShutDownEventArgs());
        }

        /// <summary>
        /// Actions performed when the display control state is changed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleDisplayControl(object sender, DisplayControlEventArgs eventArgs)
        {
            DisplayControlState = eventArgs.DisplayControlState;
        }

        /// <summary>
        /// Actions performed when a new shell context is about to start.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleNewShellContext(object sender, NewShellContextEventArgs eventArgs)
        {
            pendingContext = new ShellContext(shellLink.MountPoint,
                                              eventArgs.GameMode,
                                              shellLink.ShellTag,
                                              shellLink.ShellTagDataFile);
        }

        /// <summary>
        /// Actions performed when a new shell context is activated.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleActivateShellContext(object sender, ActivateShellContextEventArgs eventArgs)
        {
            Context = pendingContext;

            if(Context.GameMode == GameMode.Play ||
               Context.GameMode == GameMode.Utility)
            {
                MaxNumCoplayers = shellLink.CoplayerManagementCategory.GetConfigDataMaxNumOfCoplayers();

                shellHistoryStore.NewContext(Context);
                bankPlay.NewContext(Context);
                gamePlayStatus.NewContext(Context);
                gamePresentationBehavior.NewContext(Context);
                localization.NewContext(Context);
                gameCulture.NewContext(Context);
                chooserServices.NewContext(Context);
                showDemo.NewContext(Context);
            }
            CachedStoreCollection.ResetAll();

            ActivateLayeredContext(ContextLayer.Shell);
        }

        /// <summary>
        /// Actions performed when the current shell context is inactivated.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleInactivateShellContext(object sender, InactivateShellContextEventArgs eventArgs)
        {
            pendingContext = (ShellContext)Context;
            Context = new ShellContext();

            // This might be redundant since it is called in Activate, too.
            CachedStoreCollection.ResetAll();

            // Clear all tilts that are currently being tracked.
            shellTiltController.ClearTiltInfo();

            InactivateLayeredContext(ContextLayer.Shell);
        }

        #endregion

        #endregion
    }
}