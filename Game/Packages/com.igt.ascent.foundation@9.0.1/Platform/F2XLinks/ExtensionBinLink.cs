// -----------------------------------------------------------------------
// <copyright file = "ExtensionBinLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System.Collections.Generic;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.DiscoveryContextTypes;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using Game.Core.Communication.Foundation.Transport;

    /// <summary>
    /// This class provides a link to the Foundation for extension bin executables.
    /// </summary>
    /// <devdoc>
    /// This class manages the F2X communication on the Link level.
    /// It also manages a set of inner links, each for a specific negotiation level other than Link.
    /// </devdoc>
    internal sealed class ExtensionBinLink : LinkBase
    {
        #region Private Fields

        /// <summary>
        /// The list of message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> linkCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.SystemApiControl, true),
                    new CategorySubscription(MessageCategory.AscribedGameApiControl, true),
                    new CategorySubscription(MessageCategory.TsmApiControl, true),
                    new CategorySubscription(MessageCategory.AppApiControl, false),
                    new CategorySubscription(MessageCategory.Activation, true),
                    new CategorySubscription(MessageCategory.ActionRequest, true),
                    new CategorySubscription(MessageCategory.CustomConfigurationRead, true),
                    new CategorySubscription(MessageCategory.Localization, true),
                    new CategorySubscription(MessageCategory.CultureRead, true),
                    new CategorySubscription(MessageCategory.NonTransactionalCritDataRead, true),
                    new CategorySubscription(MessageCategory.TransactionalCritDataRead, true),
                    new CategorySubscription(MessageCategory.TransactionalCritDataWrite, true),
                    new CategorySubscription(MessageCategory.GameInformation, true),
                    new CategorySubscription(MessageCategory.TiltControl, false),
                    new CategorySubscription(MessageCategory.ParcelComm, true),
                    new CategorySubscription(MessageCategory.BankStatus, false),
                };

        private readonly IApiManager linkApiManager;

        #endregion

        #region Inner Links

        /// <summary>
        /// Gets the link on System negotiation level.
        /// </summary>
        public ISystemExtensionLink SystemExtensionLink { get; }

        /// <summary>
        /// Gets the link on AscribedGame negotiation level.
        /// </summary>
        public IAscribedGameExtensionLink AscribedGameExtensionLink { get; }

        /// <summary>
        /// Gets the link on Tsm negotiation level.
        /// </summary>
        public ITsmExtensionLink TsmExtensionLink { get; }

        /// <summary>
        /// Gets the link on App negotiation level.
        /// </summary>
        public IAppExtensionLink AppExtensionLink { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the category for requesting client initiated transactions.
        /// </summary>
        public IActionRequestCategory ActionRequestCategory { get; private set; }

        /// <summary>
        /// Gets the category for requesting information about custom configuration items.
        /// </summary>
        public ICustomConfigurationReadCategory CustomConfigurationReadCategory { get; private set; }

        /// <summary>
        /// Gets the category for localization.
        /// </summary>
        public ILocalizationCategory LocalizationCategory { get; private set; }

        /// <summary>
        /// Gets the category for culture read.
        /// </summary>
        public ICultureReadCategory CultureReadCategory { get; private set; }

        /// <summary>
        /// Gets the category for non-transactional critical data read.
        /// </summary>
        public INonTransactionalCritDataReadCategory NonTransactionalCritDataReadCategory { get; private set; }

        /// <summary>
        /// Gets the category for transactional critical data read.
        /// </summary>
        public ITransactionalCritDataReadCategory TransactionalCritDataReadCategory { get; private set; }

        /// <summary>
        /// Gets the category for transactional critical data write.
        /// </summary>
        public ITransactionalCritDataWriteCategory TransactionalCritDataWriteCategory { get; private set; }

        /// <summary>
        /// Gets the category for requesting information about custom configuration items.
        /// </summary>
        public IGameInformationCategory GameInformationCategory { get; private set; }

        /// <summary>
        /// Gets the category for unified parcel communication.
        /// </summary>
        public IParcelCommCategory ParcelCommCategory { get; private set; }

        /// <summary>
        /// Gets the category for extension tilts support.
        /// </summary>
        public ITiltControlCategory TiltControlCategory { get; private set; }

        /// <summary>
        /// Gets the category for bank status support.
        /// </summary>
        public IBankStatusCategory BankStatusCategory { get; private set; }

        /// <summary>
        /// Gets the id of the transaction that was last opened.
        /// If a transaction is closed during this call it will return the last opened id.
        /// </summary>
        public uint LastTransactionId => LinkController.LastTransactionId;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionBinLink"/>.
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
        public ExtensionBinLink(FoundationTarget foundationTarget,
                                ITransport transportSession,
                                IInterfaceExtensionDependencies baseExtensionDependencies,
                                IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations,
                                IEventCallbacks eventCallbacks,
                                INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            EventCallbacks = eventCallbacks;

            linkApiManager = new LinkApiManager(linkCategorySubscriptions);

            // Inner links
            SystemExtensionLink = new SystemExtensionLink();
            AscribedGameExtensionLink = new AscribedGameExtensionLink();
            TsmExtensionLink = new TsmExtensionLink();
            AppExtensionLink = new AppExtensionLink();

            var apiManagers = new List<IApiManager>
                                  {
                                      linkApiManager,
                                      SystemExtensionLink.ApiManager,
                                      AscribedGameExtensionLink.ApiManager,
                                      TsmExtensionLink.ApiManager,
                                      AppExtensionLink.ApiManager
                                  };

            var categoryDependencies = new CategoryNegotiationDependencies
                                           {
                                               EventCallbacks = eventCallbacks,
                                               NonTransactionalEventCallbacks = nonTransactionalEventCallbacks,
                                               TransactionCallbacks = this,
                                               LinkControlCategoryCallbacks = Retrieve<ILinkControlCategoryCallbacks>(apiManagers),
                                               SystemApiControlCategoryCallbacks = Retrieve<ISystemApiControlCategoryCallbacks>(apiManagers),
                                               AscribedGameApiControlCategoryCallbacks = Retrieve<IAscribedGameApiControlCategoryCallbacks>(apiManagers),
                                               TsmApiControlCategoryCallbacks = Retrieve<ITsmApiControlCategoryCallbacks>(apiManagers),
                                               AppApiControlCategoryCallbacks = Retrieve<IAppApiControlCategoryCallbacks>(apiManagers),
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

        /// <inheritdoc />
        protected override IEventCallbacks EventCallbacks { get; set; }

        /// <inheritdoc />
        protected override DiscoveryType MountPointDiscoveryType => DiscoveryType.ExecutableExtensionBin;

        /// <inheritdoc />
        public override void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            ActionRequestCategory = Retrieve<IActionRequestCategory>(installedHandlers, MessageCategory.ActionRequest);
            CustomConfigurationReadCategory = Retrieve<ICustomConfigurationReadCategory>(installedHandlers, MessageCategory.CustomConfigurationRead);
            LocalizationCategory = Retrieve<ILocalizationCategory>(installedHandlers, MessageCategory.Localization);
            CultureReadCategory = Retrieve<ICultureReadCategory>(installedHandlers, MessageCategory.CultureRead);
            NonTransactionalCritDataReadCategory = Retrieve<INonTransactionalCritDataReadCategory>(installedHandlers, MessageCategory.NonTransactionalCritDataRead);
            TransactionalCritDataReadCategory = Retrieve<ITransactionalCritDataReadCategory>(installedHandlers, MessageCategory.TransactionalCritDataRead);
            TransactionalCritDataWriteCategory = Retrieve<ITransactionalCritDataWriteCategory>(installedHandlers, MessageCategory.TransactionalCritDataWrite);
            GameInformationCategory = Retrieve<IGameInformationCategory>(installedHandlers, MessageCategory.GameInformation);
            TiltControlCategory = Retrieve<ITiltControlCategory>(installedHandlers, MessageCategory.TiltControl);
            ParcelCommCategory = Retrieve<IParcelCommCategory>(installedHandlers, MessageCategory.ParcelComm);
            BankStatusCategory = Retrieve<IBankStatusCategory>(installedHandlers, MessageCategory.BankStatus);
        }

        /// <inheritdoc />
        /// <remarks>
        /// The extension bin link only returns the link level interface extensions.
        /// the interface extensions on other negotiation levels are managed by inner links.
        /// </remarks>
        public override TExtendedInterface GetInterface<TExtendedInterface>()
        {
            return linkApiManager.GetInterface<TExtendedInterface>();
        }

        #endregion
    }
}