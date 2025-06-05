// -----------------------------------------------------------------------
// <copyright file = "CoplayerLink.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.DiscoveryContextTypes;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using Game.Core.Communication.Foundation.Transport.Sessions;
    using Game.Core.Threading;

    /// <summary>
    /// This class provides a link to the Foundation for coplayers.
    /// </summary>
    internal sealed class CoplayerLink : LinkBase, ICoplayerApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> categorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.ActionRequest, true),
                    new CategorySubscription(MessageCategory.ActionRequestLite, true),
                    new CategorySubscription(MessageCategory.CoplayerApiControl, true),
                };

        /// <summary>
        /// The list of coplayer message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> coplayerCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.CoplayerActivation, true),
                    new CategorySubscription(MessageCategory.GameCyclePlay, true),
                    new CategorySubscription(MessageCategory.GameCycleBetting, true),
                    new CategorySubscription(MessageCategory.ThemeStore, true),
                    new CategorySubscription(MessageCategory.PayvarStore, true),
                    new CategorySubscription(MessageCategory.CoplayerHistoryStore, true),
                    new CategorySubscription(MessageCategory.GamePlayStore, true),
                };

        /// <summary>
        /// Event used to block until coplayer level connecting is finished.
        /// </summary>
        private readonly AutoResetEvent coplayerConnectComplete = new AutoResetEvent(false);

        /// <summary>
        /// Flag indicating if the coplayer level connection has been established.
        /// </summary>
        private volatile bool isCoplayerConnected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the coplayer ID.
        /// </summary>
        public int CoplayerId { get; private set; }

        /// <summary>
        /// Gets the theme identifier being played in the coplayer.
        /// </summary>
        public string ThemeIdentifier { get; private set; }

        /// <summary>
        /// Gets the G2SThemeId defined in the theme registry.
        /// </summary>
        public string G2SThemeId { get; private set; }

        /// <summary>
        /// Gets the Tag defined in the theme registry.
        /// </summary>
        public string ThemeTag { get; private set; }

        /// <summary>
        /// Gets the TagDataFile defined in the theme registry.
        /// </summary>
        public string ThemeTagDataFile { get; private set; }

        /// <summary>
        /// Gets the category for requesting client initiated transactions.
        /// </summary>
        public IActionRequestCategory ActionRequestCategory { get; private set; }

        /// <summary>
        /// Gets the category for requesting lightweight client initiated transactions.
        /// </summary>
        public IActionRequestLiteCategory ActionRequestLiteCategory { get; private set; }

        /// <summary>
        /// Gets the category for game cycle play.
        /// </summary>
        public IGameCyclePlayCategory GameCyclePlayCategory { get; private set; }

        /// <summary>
        /// Gets the category for game cycle betting.
        /// </summary>
        public IGameCycleBettingCategory GameCycleBettingCategory { get; private set; }

        /// <summary>
        /// Gets the category for access to the critical data store of the theme.
        /// </summary>
        public IThemeStoreCategory ThemeStoreCategory { get; private set; }

        /// <summary>
        /// Gets the category for access to the critical data store of the payvar.
        /// </summary>
        public IPayvarStoreCategory PayvarStoreCategory { get; private set; }

        /// <summary>
        /// Gets the category for access to the critical data store of the coplayer history.
        /// </summary>
        public ICoplayerHistoryStoreCategory CoplayerHistoryStoreCategory { get; private set; }

        /// <summary>
        /// Gets the category for access to the critical data store of the coplayer game play store.
        /// </summary>
        public IGamePlayStoreCategory GamePlayStoreCategory { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerLink"/>.
        /// </summary>
        /// <remarks>
        /// Since this is an internal class, argument validation is skipped for sake of performance.
        /// Callers are responsible for passing in valid arguments.
        /// </remarks>
        /// <param name="foundationTarget">
        /// The Foundation version the game targets to run with.
        /// </param>
        /// <param name="transportSession">
        /// The transport session the coplayer to run in.
        /// </param>
        /// <param name="baseExtensionDependencies">
        /// The base dependencies to be provided to interface extensions.
        /// </param>
        /// <param name="additionalInterfaceConfigurations">
        /// The configurations of interface extensions expected to be provided by the coplayer.
        /// </param>
        /// <param name="eventCallbacks">
        /// The object for handling transactional events sent from Foundation.
        /// </param>
        /// <param name="nonTransactionalEventCallbacks">
        /// The object for handling non-transactional events sent from Foundation.
        /// </param>
        public CoplayerLink(FoundationTarget foundationTarget,
                            ISession transportSession,
                            IInterfaceExtensionDependencies baseExtensionDependencies,
                            IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations,
                            IEventCallbacks eventCallbacks,
                            INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            EventCallbacks = eventCallbacks;

            var apiManagers = new List<IApiManager>
                                  {
                                      new LinkApiManager(categorySubscriptions),
                                      new CoplayerApiManager(coplayerCategorySubscriptions, this),
                                  };

            var categoryDependencies = new CategoryNegotiationDependencies
                                           {
                                               EventCallbacks = eventCallbacks,
                                               NonTransactionalEventCallbacks = nonTransactionalEventCallbacks,
                                               TransactionCallbacks = this,
                                               LinkControlCategoryCallbacks = Retrieve<ILinkControlCategoryCallbacks>(apiManagers),
                                               CoplayerApiControlCategoryCallbacks = Retrieve<ICoplayerApiControlCategoryCallbacks>(apiManagers),
                                           };

            LinkController = new LinkController(transportSession,
                                                foundationTarget,
                                                linkStatusCallbacks: this,
                                                categoryManagers: apiManagers,
                                                categoryDependencies: categoryDependencies,
                                                baseExtensionDependencies: baseExtensionDependencies,
                                                interfaceExtensionConfigurations: additionalInterfaceConfigurations);
        }

        #endregion

        #region LinkBase Overrides

        /// <inheritdoc/>
        protected override IEventCallbacks EventCallbacks { get; set; }

        /// <inheritdoc/>
        protected override DiscoveryType MountPointDiscoveryType => DiscoveryType.Bin;

        /// <inheritdoc/>
        public override bool Connect()
        {
            var isLinkConnected = base.Connect();

            if(isLinkConnected)
            {
                coplayerConnectComplete.WaitOne(TransportExceptionMonitor);
            }

            return isLinkConnected && isCoplayerConnected;
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            base.Disconnect();

            // Clean up just to make sure.
            isCoplayerConnected = false;
            coplayerConnectComplete.Reset();
        }

        /// <inheritdoc/>
        public override void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            ActionRequestCategory = Retrieve<IActionRequestCategory>(installedHandlers, MessageCategory.ActionRequest);
            ActionRequestLiteCategory = Retrieve<IActionRequestLiteCategory>(installedHandlers, MessageCategory.ActionRequestLite);
        }

        /// <inheritdoc/>
        public override void ProcessLinkShutDown()
        {
            // If the any level negotiation fails, Foundation would send a Shut Down message,
            // while the application is still being blocked in the ConnectToFoundation method.
            // Unblock coplayerConnectComplete so that the application can properly shut down.
            coplayerConnectComplete.Set();

            base.ProcessLinkShutDown();
        }

        #endregion

        #region ICoplayerApiCallbacks Implementation

        /// <inheritdoc/>
        public void ProcessCoplayerApiStart(int coplayerId, string themeIdentifier, string g2SThemeId, string themeTag, string themeTagDataFile)
        {
            CoplayerId = coplayerId;
            ThemeIdentifier = themeIdentifier;
            G2SThemeId = g2SThemeId;
            ThemeTag = themeTag;
            ThemeTagDataFile = themeTagDataFile;
        }

        /// <inheritdoc/>
        public void ProcessCoplayerApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            GameCyclePlayCategory = Retrieve<IGameCyclePlayCategory>(installedHandlers, MessageCategory.GameCyclePlay);
            GameCycleBettingCategory = Retrieve<IGameCycleBettingCategory>(installedHandlers, MessageCategory.GameCycleBetting);
            ThemeStoreCategory = Retrieve<IThemeStoreCategory>(installedHandlers, MessageCategory.ThemeStore);
            PayvarStoreCategory = Retrieve<IPayvarStoreCategory>(installedHandlers, MessageCategory.PayvarStore);
            CoplayerHistoryStoreCategory = Retrieve<ICoplayerHistoryStoreCategory>(installedHandlers, MessageCategory.CoplayerHistoryStore);
            GamePlayStoreCategory = Retrieve<IGamePlayStoreCategory>(installedHandlers, MessageCategory.GamePlayStore);

            isCoplayerConnected = true;

            // Signal the connect complete event.
            // In case of shell level re-negotiations, signaling this event is redundant but OK.
            coplayerConnectComplete.Set();
        }

        #endregion

        #region IDisposable Overrides

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if(!IsDisposed && disposing)
            {
                (coplayerConnectComplete as IDisposable).Dispose();

                // Do not set IsDisposed.  Leave it to the base class.
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}