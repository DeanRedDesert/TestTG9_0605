// -----------------------------------------------------------------------
// <copyright file = "ShellLink.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.DiscoveryContextTypes;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.ShellApiControl;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using Game.Core.Communication.Foundation.Transport.Sessions;
    using Game.Core.Threading;

    /// <summary>
    /// This class provides a link to the Foundation for shell executables.
    /// </summary>
    internal sealed class ShellLink : LinkBase, IShellApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> linkCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.ActionRequest, true),
                    new CategorySubscription(MessageCategory.ActionRequestLite, true),
                    new CategorySubscription(MessageCategory.Activation, false),
                    new CategorySubscription(MessageCategory.ShellApiControl, true),
                    new CategorySubscription(MessageCategory.SessionManagement, true),
                    new CategorySubscription(MessageCategory.ParcelComm, true),
                };

        /// <summary>
        /// The list of shell message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> shellCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.DisplayControl, true),
                    new CategorySubscription(MessageCategory.ShellActivation, true),
                    new CategorySubscription(MessageCategory.CoplayerManagement, true),
                    new CategorySubscription(MessageCategory.ShellThemeControl, true),
                    new CategorySubscription(MessageCategory.ShellStore, true),
                    new CategorySubscription(MessageCategory.ShellHistoryStore, true),
                    new CategorySubscription(MessageCategory.BankPlay, true),
                    new CategorySubscription(MessageCategory.ChooserServices, true),
                    new CategorySubscription(MessageCategory.ShowDemo, false),
                    new CategorySubscription(MessageCategory.ShellHistoryControl, true),
                    new CategorySubscription(MessageCategory.GamePlayStatus, true),
                    new CategorySubscription(MessageCategory.GamePresentationBehavior, true),
                    new CategorySubscription(MessageCategory.Localization, true),
                    new CategorySubscription(MessageCategory.CultureRead, true),
                    new CategorySubscription(MessageCategory.TiltControl, true),
                };

        /// <summary>
        /// Event used to block until shell level connecting is finished.
        /// </summary>
        private readonly AutoResetEvent shellConnectComplete = new AutoResetEvent(false);

        /// <summary>
        /// Flag indicating if the shell level connection has been established.
        /// </summary>
        private volatile bool isShellConnected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Tag defined in the shell registry.
        /// </summary>
        public string ShellTag { get; private set; }

        /// <summary>
        /// Gets the TagDataFile defined in the shell registry.
        /// </summary>
        public string ShellTagDataFile { get; private set; }

        /// <summary>
        /// Gets the category for requesting client initiated transactions.
        /// </summary>
        public IActionRequestCategory ActionRequestCategory { get; private set; }

        /// <summary>
        /// Gets the category for requesting lightweight client initiated transactions.
        /// </summary>
        public IActionRequestLiteCategory ActionRequestLiteCategory { get; private set; }

        /// <summary>
        /// Gets the category for managing coplayers.
        /// </summary>
        public ICoplayerManagementCategory CoplayerManagementCategory { get; private set; }

        /// <summary>
        /// Gets the category for querying and switching themes for coplayers.
        /// </summary>
        public IShellThemeControlCategory ShellThemeControlCategory { get; private set; }

        /// <summary>
        /// Gets the category for managing sessions.
        /// </summary>
        public ISessionManagementCategory SessionManagementCategory { get; private set; }

        /// <summary>
        /// Gets the category for the parcel communication.
        /// </summary>
        public IParcelCommCategory ParcelCommCategory { get; private set; }

        /// <summary>
        /// Gets the category for accessing shell critical data.
        /// </summary>
        public IShellStoreCategory ShellStoreCategory { get; private set; }

        /// <summary>
        /// Gets the category for accessing shell's history critical data.
        /// </summary>
        public IShellHistoryStoreCategory ShellHistoryStoreCategory { get; private set; }

        /// <summary>
        /// Gets the category for the bank play.
        /// </summary>
        public IBankPlayCategory BankPlayCategory { get; private set; }

        /// <summary>
        /// Gets the category for chooser services.
        /// </summary>
        public IChooserServicesCategory ChooserServicesCategory { get; private set; }

        /// <summary>
        /// Gets the category for game play status.
        /// </summary>
        public IGamePlayStatusCategory GamePlayStatusCategory { get; private set; }

        /// <summary>
        /// Gets the category for show demo.
        /// </summary>
        public IShowDemoCategory ShowDemoCategory { get; private set; }

        /// <summary>
        /// Gets the category for shell history control.
        /// </summary>
        public IShellHistoryControlCategory ShellHistoryControlCategory { get; private set; }

        /// <summary>
        /// Gets the category for the game presentation behavior.
        /// </summary>
        public IGamePresentationBehaviorCategory GamePresentationBehaviorCategory { get; private set; }

        /// <summary>
        /// Gets the category for the localization.
        /// </summary>
        public ILocalizationCategory LocalizationCategory { get; private set; }

        /// <summary>
        /// Gets the category for the CultureRead.
        /// </summary>
        public ICultureReadCategory CultureReadCategory { get; private set; }

        /// <summary>
        /// Gets the category for the TiltControl.
        /// </summary>
        public ITiltControlCategory TiltControlCategory { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellLink"/>.
        /// </summary>
        /// <remarks>
        /// Since this is an internal class, argument validation is skipped for sake of performance.
        /// Callers are responsible for passing in valid arguments.
        /// </remarks>
        /// <param name="foundationTarget">
        /// The Foundation version the game targets to run with.
        /// </param>
        /// <param name="transportSession">
        /// The transport session the shell to run in.
        /// </param>
        /// <param name="baseExtensionDependencies">
        /// The base dependencies to be provided to interface extensions.
        /// </param>
        /// <param name="additionalInterfaceConfigurations">
        /// The configurations of interface extensions expected to be provided by the shell.
        /// </param>
        /// <param name="eventCallbacks">
        /// The object for handling transactional events sent from Foundation.
        /// </param>
        /// <param name="nonTransactionalEventCallbacks">
        /// The object for handling non-transactional events sent from Foundation.
        /// </param>
        public ShellLink(FoundationTarget foundationTarget,
                         ISession transportSession,
                         IInterfaceExtensionDependencies baseExtensionDependencies,
                         IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations,
                         IEventCallbacks eventCallbacks,
                         INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            EventCallbacks = eventCallbacks;

            var apiManagers = new List<IApiManager>
                                  {
                                      new LinkApiManager(linkCategorySubscriptions),
                                      new ShellApiManager(shellCategorySubscriptions, this),
                                  };

            var categoryDependencies = new CategoryNegotiationDependencies
                                           {
                                               EventCallbacks = eventCallbacks,
                                               NonTransactionalEventCallbacks = nonTransactionalEventCallbacks,
                                               TransactionCallbacks = this,
                                               LinkControlCategoryCallbacks = Retrieve<ILinkControlCategoryCallbacks>(apiManagers),
                                               ShellApiControlCategoryCallbacks = Retrieve<IShellApiControlCategoryCallbacks>(apiManagers),
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
                shellConnectComplete.WaitOne(TransportExceptionMonitor);
            }

            return isLinkConnected && isShellConnected;
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            base.Disconnect();

            // Clean up just to make sure.
            isShellConnected = false;
            shellConnectComplete.Reset();
        }

        /// <inheritdoc/>
        public override void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            ActionRequestCategory = Retrieve<IActionRequestCategory>(installedHandlers, MessageCategory.ActionRequest);
            ActionRequestLiteCategory = Retrieve<IActionRequestLiteCategory>(installedHandlers, MessageCategory.ActionRequestLite);
            SessionManagementCategory = Retrieve<ISessionManagementCategory>(installedHandlers, MessageCategory.SessionManagement);
            ParcelCommCategory = Retrieve<IParcelCommCategory>(installedHandlers, MessageCategory.ParcelComm);
        }

        /// <inheritdoc/>
        public override void ProcessLinkShutDown()
        {
            // If the any level negotiation fails, Foundation would send a Shut Down message,
            // while the application is still being blocked in the ConnectToFoundation method.
            // Unblock shellConnectComplete so that the application can properly shut down.
            shellConnectComplete.Set();

            base.ProcessLinkShutDown();
        }

        #endregion

        #region IShellApiCallbacks Implementation

        /// <inheritdoc/>
        public void ProcessShellApiStart(string shellTag, string shellTagDataFile, ICollection<LinkedExtension> linkedExtensions)
        {
            ShellTag = shellTag;
            ShellTagDataFile = shellTagDataFile;

            // Shell application gets the extension import collection from ShellApiStart, not LinkStart.
            ExtensionImportCollection = new ExtensionImportCollection(
                linkedExtensions.Select(linked => new ExtensionImport(linked.Extension.ExtensionIdentifier,
                                                                      new Version((int)linked.Extension.ExtensionVersion.MajorVersion,
                                                                                  (int)linked.Extension.ExtensionVersion.MinorVersion,
                                                                                  (int)linked.Extension.ExtensionVersion.PatchVersion),
                                                                      linked.ResourceDirectoryBase)));
        }

        /// <inheritdoc/>
        public void ProcessShellApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            CoplayerManagementCategory = Retrieve<ICoplayerManagementCategory>(installedHandlers, MessageCategory.CoplayerManagement);
            ShellThemeControlCategory = Retrieve<IShellThemeControlCategory>(installedHandlers, MessageCategory.ShellThemeControl);
            ShellStoreCategory = Retrieve<IShellStoreCategory>(installedHandlers, MessageCategory.ShellStore);
            ShellHistoryStoreCategory = Retrieve<IShellHistoryStoreCategory>(installedHandlers, MessageCategory.ShellHistoryStore);
            BankPlayCategory = Retrieve<IBankPlayCategory>(installedHandlers, MessageCategory.BankPlay);
            ChooserServicesCategory = Retrieve<IChooserServicesCategory>(installedHandlers, MessageCategory.ChooserServices);
            ShellHistoryControlCategory = Retrieve<IShellHistoryControlCategory>(installedHandlers, MessageCategory.ShellHistoryControl);
            GamePlayStatusCategory = Retrieve<IGamePlayStatusCategory>(installedHandlers, MessageCategory.GamePlayStatus);
            GamePresentationBehaviorCategory = Retrieve<IGamePresentationBehaviorCategory>(installedHandlers, MessageCategory.GamePresentationBehavior);
            LocalizationCategory = Retrieve<ILocalizationCategory>(installedHandlers, MessageCategory.Localization);
            CultureReadCategory = Retrieve<ICultureReadCategory>(installedHandlers, MessageCategory.CultureRead);
            TiltControlCategory = Retrieve<ITiltControlCategory>(installedHandlers, MessageCategory.TiltControl);

            // Process optional categories.
            ShowDemoCategory = Retrieve<IShowDemoCategory>(installedHandlers, MessageCategory.ShowDemo);

            isShellConnected = true;

            // Signal the connect complete event.
            // In case of shell level re-negotiations, signaling this event is redundant but OK.
            shellConnectComplete.Set();
        }

        #endregion

        #region IDisposable Overrides

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if(!IsDisposed && disposing)
            {
                (shellConnectComplete as IDisposable).Dispose();

                // Do not set IsDisposed.  Leave it to the base class.
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}