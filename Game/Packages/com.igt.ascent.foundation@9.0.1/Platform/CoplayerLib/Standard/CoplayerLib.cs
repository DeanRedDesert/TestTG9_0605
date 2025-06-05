// -----------------------------------------------------------------------
// <copyright file = "CoplayerLib.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using F2XLinks;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Transport.Sessions;
    using Game.Core.Threading;
    using Interfaces;
    using Platform.Interfaces;
    using Platform.Standard;

    /// <summary>
    /// Standard implementation of interfaces for a coplayer to communicate with the Foundation.
    /// </summary>
    public sealed class CoplayerLib : StandardAppLibBase, ICoplayerLibRestricted, ICoplayerLib, IGameModeQuery, IGameCycleStateQuery
    {
        #region Private Fields

        /// <summary>
        /// The link object that handles category negotiations for the coplayer.
        /// </summary>
        private readonly CoplayerLink coplayerLink;

        /// <summary>
        /// The pending context that is about to be activated.
        /// </summary>
        private CoplayerContext pendingContext;

        /// <summary>
        /// The cached interface of game cycle play.
        /// </summary>
        private readonly GameCyclePlay gameCyclePlay;

        /// <summary>
        /// The cached interface of game cycle betting.
        /// </summary>
        private readonly GameCycleBetting gameCycleBetting;

        /// <summary>
        /// The cached interface for access to the critical data store for the theme.
        /// </summary>
        private readonly ThemeStore themeStore;

        /// <summary>
        /// The cached interface for access to the critical data store for the payvar.
        /// </summary>
        private readonly PayvarStore payvarStore;

        /// <summary>
        /// The cached interface for access to the critical data store for the coplayer history.
        /// </summary>
        private readonly CoplayerHistoryStore coplayerHistoryStore;

        /// <summary>
        /// The cached interface for access to the critical data store for the coplayer game play store.
        /// </summary>
        private readonly GamePlayStore gamePlayStore;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerLib"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The id of the coplayer.
        /// </param>
        /// <param name="foundationTarget">
        /// The foundation version to target.
        /// </param>
        /// <param name="transportSession">
        /// The transport session for this coplayer lib to use to communicate with the Foundation.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// List of additional interface configurations to install in this coplayer lib.
        /// </param>
        /// <remarks>
        /// Requested interface configurations may not be available if not supported by the underlying platform.
        /// </remarks>
        public CoplayerLib(int coplayerId,
                           FoundationTarget foundationTarget,
                           ISession transportSession,
                           IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations)
        {
            CoplayerId = coplayerId;
            Context = new CoplayerContext();

            // Set up connection.
            var baseExtensionDependencies = new InterfaceExtensionDependencies
                                                {
                                                    TransactionWeightVerification = new TransactionVerifier(TransEventQueue),
                                                    TransactionalEventDispatcher = TransEventQueue,
                                                    NonTransactionalEventDispatcher = NonTransEventQueue,
                                                };

            coplayerLink = new CoplayerLink(foundationTarget, transportSession,
                                            baseExtensionDependencies: baseExtensionDependencies,
                                            additionalInterfaceConfigurations: interfaceExtensionConfigurations,
                                            eventCallbacks: TransEventQueue,
                                            nonTransactionalEventCallbacks: NonTransEventQueue);
            DisposableCollection.Add(coplayerLink);

            coplayerLink.LinkShutDownEvent += HandleLinkShutDown;

            // Create built-in interface implementations and critical data stores.
            // Pay attention to their construction dependencies.
            gameCyclePlay = new GameCyclePlay(this, TransEventQueue, NonTransEventQueue);

            var gameEntityStoreAccessValidator = new GameEntityStoreAccessValidator(this);
            themeStore = new ThemeStore(gameEntityStoreAccessValidator);
            payvarStore = new PayvarStore(gameEntityStoreAccessValidator);
            gamePlayStore = new GamePlayStore(gameEntityStoreAccessValidator, gameCyclePlay);

            var coplayerHistoryStoreValidator = new CoplayerHistoryStoreValidator(this, this);
            coplayerHistoryStore = new CoplayerHistoryStore(coplayerHistoryStoreValidator, gameCyclePlay);

            gameCycleBetting = new GameCycleBetting(gamePlayStore);

            // Add critical data stores to collection.  No need to check types.  The collection object will do it.
            CachedStoreCollection.Add(themeStore);
            CachedStoreCollection.Add(payvarStore);
            CachedStoreCollection.Add(gamePlayStore);
            CachedStoreCollection.Add(coplayerHistoryStore);

            // Create event table.
            CreateEventTable();

            // Subscribe event handlers.
            NewCoplayerContextEvent += HandleNewCoplayerContext;
            ActivateCoplayerContextEvent += HandleActivateCoplayerContext;
            InactivateCoplayerContextEvent += HandleInactivateCoplayerContext;
        }

        #endregion

        #region ICoplayerLibRestricted Implementation

        ///<inheritdoc/>
        public event EventHandler<NewCoplayerContextEventArgs> NewCoplayerContextEvent;

        ///<inheritdoc/>
        public event EventHandler<ActivateCoplayerContextEventArgs> ActivateCoplayerContextEvent;

        ///<inheritdoc/>
        public event EventHandler<InactivateCoplayerContextEventArgs> InactivateCoplayerContextEvent;

        /// <inheritdoc/>
        public event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <inheritdoc/>
        public event EventHandler<ActionResponseEventArgs> ActionResponseEvent;

        /// <inheritdoc/>
        public event EventHandler<ActionResponseLiteEventArgs> ActionResponseLiteEvent;

        ///<inheritdoc/>
        public int CoplayerId { get; }

        /// <inheritdoc/>
        public IExceptionMonitor ExceptionMonitor => coplayerLink.TransportExceptionMonitor;

        /// <inheritdoc/>
        public IGameCyclePlayRestricted GameCyclePlayRestricted => gameCyclePlay;

        /// <inheritdoc/>
        public bool ConnectToFoundation()
        {
            var connected = coplayerLink.Connect();
            if(connected)
            {
                // Initialize the category interface.
                gameCyclePlay.Initialize(coplayerLink.GameCyclePlayCategory);
                gameCycleBetting.Initialize(coplayerLink.GameCycleBettingCategory);
                themeStore.Initialize(coplayerLink.ThemeStoreCategory);
                payvarStore.Initialize(coplayerLink.PayvarStoreCategory);
                coplayerHistoryStore.Initialize(coplayerLink.CoplayerHistoryStoreCategory);
                gamePlayStore.Initialize(coplayerLink.GamePlayStoreCategory);
            }

            return connected;
        }

        /// <inheritdoc/>
        public void ActionRequest(string transactionName)
        {
            var payload = transactionName == null
                              ? new byte[0]
                              : Encoding.ASCII.GetBytes(transactionName);

            coplayerLink.ActionRequestCategory.ActionRequest(payload);
        }

        /// <inheritdoc/>
        public void ActionRequestLite(string transactionName)
        {
            var payload = transactionName == null
                              ? new byte[0]
                              : Encoding.ASCII.GetBytes(transactionName);

            coplayerLink.ActionRequestLiteCategory.ActionRequestLite(payload);
        }

        #endregion

        #region ICoplayerLib Implementation

        ///<inheritdoc/>
        public ICoplayerContext Context { get; private set; }

        /// <inheritdoc />
        public IGameCyclePlay GameCyclePlay => gameCyclePlay;

        /// <inheritdoc />
        public IGameCycleBetting GameCycleBetting => gameCycleBetting;

        /// <inheritdoc/>
        public ICriticalDataStore ThemeStore => themeStore;

        /// <inheritdoc/>
        public ICriticalDataStore PayvarStore => payvarStore;

        /// <inheritdoc/>
        public ICriticalDataStore GamePlayStore => gamePlayStore;

        /// <inheritdoc/>
        public ICriticalDataStore HistoryStore => coplayerHistoryStore;

        /// <inheritdoc/>
        public TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class
        {
            return coplayerLink.GetInterface<TExtendedInterface>();
        }

        #endregion

        #region IGameModeQuery Implementation

        /// <inheritdoc/>
        public GameMode GameMode => Context.GameMode;

        #endregion

        #region IGameCycleStateQuery Implementation

        /// <inheritdoc/>
        public GameCycleState GameCycleState => GameCyclePlay.GameCycleState;

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

            EventTable[typeof(ActivateCoplayerContextEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActivateCoplayerContextEvent);

            EventTable[typeof(InactivateCoplayerContextEventArgs)] =
                (sender, eventArgs) => InterceptInactivateCoplayerContext(sender, eventArgs as InactivateCoplayerContextEventArgs);

            EventTable[typeof(NewCoplayerContextEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, NewCoplayerContextEvent);

            EventTable[typeof(ShutDownEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ShutDownEvent);
        }

        /// <summary>
        /// Intercepts an Inactive Coplayer Context event and replaces it with a new instance with more information.
        /// The original event is constructed by category callback handler who does not have the knowledge
        /// to fill out some of the fields in the event, but here Coplayer Lib does.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The original event arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private void InterceptInactivateCoplayerContext(object sender, InactivateCoplayerContextEventArgs eventArgs)
        {
            var newEventArgs = new InactivateCoplayerContextEventArgs(new CoplayerContext(Context));

            ExecuteEventHandler(sender, newEventArgs, InactivateCoplayerContextEvent);
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
        /// Actions performed when a new coplayer context is about to start.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleNewCoplayerContext(object sender, NewCoplayerContextEventArgs eventArgs)
        {
            pendingContext = new CoplayerContext(CoplayerId,
                                                 coplayerLink.MountPoint,
                                                 eventArgs.GameMode,
                                                 coplayerLink.G2SThemeId,
                                                 coplayerLink.ThemeTag,
                                                 coplayerLink.ThemeTagDataFile,
                                                 eventArgs.Denomination,
                                                 eventArgs.PayvarTag,
                                                 eventArgs.PayvarTagDataFile);
        }

        /// <summary>
        /// Actions performed when a new coplayer context is activated.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleActivateCoplayerContext(object sender, ActivateCoplayerContextEventArgs eventArgs)
        {
            Context = pendingContext;

            if(Context.GameMode == GameMode.Play ||
               Context.GameMode == GameMode.Utility)
            {
                gameCyclePlay.NewContext(Context);
                gameCycleBetting.NewContext(Context);
            }

            CachedStoreCollection.ResetAll();
        }

        /// <summary>
        /// Actions performed when the current coplayer context is inactivated.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleInactivateCoplayerContext(object sender, InactivateCoplayerContextEventArgs eventArgs)
        {
            // Save the current context in case it is re-activated.
            pendingContext = (CoplayerContext)Context;
            Context = new CoplayerContext();

            // This might be redundant since it is called in Activate, too.
            CachedStoreCollection.ResetAll();
        }

        #endregion

        #endregion
    }
}