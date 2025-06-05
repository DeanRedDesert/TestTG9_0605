//-----------------------------------------------------------------------
// <copyright file = "ExtensionLink.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.AscribedGameApiControl;
    using F2X.Schemas.Internal.DiscoveryContextTypes;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// This class provides a link to the Foundation for extension executables.
    /// </summary>
    internal class ExtensionLink : LinkBase, IThemeApiCallbacks, ITsmApiCallbacks, IAscribedGameApiCallbacks, IAppApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> categorySubscriptions =
            // The category subscriptions defined here is not a complete list for link level API negotiation!
            // Different category subscription(s) may be appended to this list according to the foundation target
            // at the construction of extension link.
            new List<CategorySubscription>
            {
                new CategorySubscription(MessageCategory.ActionRequest, true),
                new CategorySubscription(MessageCategory.Activation, true),
                new CategorySubscription(MessageCategory.CustomConfigurationRead, true),
                new CategorySubscription(MessageCategory.Localization, true),
                new CategorySubscription(MessageCategory.CultureRead, true),
                new CategorySubscription(MessageCategory.SystemApiControl, true),
                new CategorySubscription(MessageCategory.TsmApiControl, true),
                new CategorySubscription(MessageCategory.AppApiControl, false),
                new CategorySubscription(MessageCategory.NonTransactionalCritDataRead, false),
                new CategorySubscription(MessageCategory.TransactionalCritDataRead, false),
                new CategorySubscription(MessageCategory.TransactionalCritDataWrite, false),
                new CategorySubscription(MessageCategory.GameInformation, true),
                new CategorySubscription(MessageCategory.TiltControl, false),
                new CategorySubscription(MessageCategory.ParcelComm, true),
                new CategorySubscription(MessageCategory.BankStatus, false),
            };

        /// <summary>
        /// The list of system message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> systemCategorySubscriptions =
            new List<CategorySubscription>
            {
                new CategorySubscription(MessageCategory.SystemActivation, false),
            };

        /// <summary>
        /// The list of theme message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> themeCategorySubscriptions =
            new List<CategorySubscription>
            {
                new CategorySubscription(MessageCategory.ThemeActivation, true),
            };

        /// <summary>
        /// The list of TSM message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> tsmCategorySubscriptions =
            new List<CategorySubscription>
            {
                new CategorySubscription(MessageCategory.TsmActivation, true),
            };

        /// <summary>
        /// The list of ascribed game message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> ascribedGameCategorySubscriptions =
            new List<CategorySubscription>
            {
                // Either ThemeActivation or AscribedShellActivation could be negotiated on AscribedGameApiControl
                // that is depending on the extension being linked to a theme or shell.
                new CategorySubscription(MessageCategory.ThemeActivation, false),
                new CategorySubscription(MessageCategory.AscribedShellActivation, false),
            };

        /// <summary>
        /// The list of APP message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> appCategorySubscriptions =
            new List<CategorySubscription>
            {
                new CategorySubscription(MessageCategory.AppActivation, true),
                new CategorySubscription(MessageCategory.DisplayControl, true),
            };

        /// <summary>
        /// List of extensions linked to the ascribed game.  The versions are what are compatible with 
        /// the importing component, not specifically the (minor) versions of the importing 
        /// component requested in the registry.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private List<Extension> linkedAscribedGameExtensions;

        /// <summary>
        /// List of extensions linked to the TSM.  The versions are what are compatible with the 
        /// importing component, not specifically the (minor) versions of the importing component 
        /// requested in the registry.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private List<Extension> linkedTsmExtensions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="ExtensionLink"/>.
        /// </summary>
        /// <param name="address">Foundation address to connect to.</param>
        /// <param name="port">Foundation port to connect to.</param>
        /// <param name="foundationTarget">Foundation version to target.</param>
        /// <param name="baseExtensionDependencies">The common dependencies for extensions.</param>
        /// <param name="additionalInterfaceConfigurations">List of additional interface configurations to request.</param>
        /// <param name="nonTransactionalEventCallbacks">The callback interface for handling non-transactional events.</param>
        internal ExtensionLink(string address, ushort port, FoundationTarget foundationTarget,
                               IInterfaceExtensionDependencies baseExtensionDependencies,
                               IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations,
                               INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            // As of AscentMSeries, we use AscribedGameApiManager instead of ThemeApiManager to negotiate
            // the API category for theme/shell extension.
            IApiManager ascribedGameApiManager;
            if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentMSeries))
            {
                categorySubscriptions.Add(new CategorySubscription(MessageCategory.AscribedGameApiControl, true));
                ascribedGameApiManager = new AscribedGameApiManager(ascribedGameCategorySubscriptions, this);
            }
            else
            {
                categorySubscriptions.Add(new CategorySubscription(MessageCategory.ThemeApiControl, true));
                ascribedGameApiManager = new ThemeApiManager(themeCategorySubscriptions, this);
            }

            var apiManagers = new List<IApiManager>
                                  {
                                      new LinkApiManager(categorySubscriptions),
                                      new SystemApiManager(systemCategorySubscriptions),
                                      new TsmApiManager(tsmCategorySubscriptions, this),
                                      new AppApiManager(appCategorySubscriptions, this),
                                      ascribedGameApiManager,
                                  };

            var categoryDependencies = new CategoryNegotiationDependencies
                                           {
                                               EventCallbacks = this,
                                               NonTransactionalEventCallbacks = nonTransactionalEventCallbacks,
                                               TransactionCallbacks = this,
                                               LinkControlCategoryCallbacks = Retrieve<ILinkControlCategoryCallbacks>(apiManagers),
                                               SystemApiControlCategoryCallbacks = Retrieve<ISystemApiControlCategoryCallbacks>(apiManagers),
                                               AscribedGameApiControlCategoryCallbacks = Retrieve<IAscribedGameApiControlCategoryCallbacks>(apiManagers),
                                               ThemeApiControlCategoryCallbacks = Retrieve<IThemeApiControlCategoryCallbacks>(apiManagers),
                                               TsmApiControlCategoryCallbacks = Retrieve<ITsmApiControlCategoryCallbacks>(apiManagers),
                                               AppApiControlCategoryCallbacks = Retrieve<IAppApiControlCategoryCallbacks>(apiManagers),
                                           };

            LinkController = new LinkController(address, port, foundationTarget,
                                                linkStatusCallbacks: this,
                                                categoryManagers: apiManagers,
                                                categoryDependencies: categoryDependencies,
                                                baseExtensionDependencies: baseExtensionDependencies,
                                                interfaceExtensionConfigurations: additionalInterfaceConfigurations);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the ascribed game entity with which this extension is linked.
        /// </summary>
        public AscribedGameEntity AscribedGameEntity { get; private set; }

        /// <summary>
        /// Gets the TSM identifier with which this extension is linked.
        /// </summary>
        public string TsmId { get; private set; }

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

        /// <summary>
        /// Gets the app extension context.
        /// </summary>
        /// <remarks>
        /// This is available for app-extension only.
        /// For non-app-extensions, it will always be null.
        /// </remarks>
        public AppExtensionContext AppExtensionContext { get; private set; }

        #endregion

        #region LinkBase Overrides

        /// <inheritdoc/>
        protected override DiscoveryType MountPointDiscoveryType => DiscoveryType.ExecutableExtensionBin;

        /// <inheritdoc/>
        public override void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            base.ProcessLinkNegotiation(installedHandlers);

            CustomConfigurationReadCategory = installedHandlers.ContainsKey(MessageCategory.CustomConfigurationRead)
                ? installedHandlers[MessageCategory.CustomConfigurationRead] as ICustomConfigurationReadCategory
                : null;

            LocalizationCategory = installedHandlers.ContainsKey(MessageCategory.Localization)
                ? installedHandlers[MessageCategory.Localization] as ILocalizationCategory
                : null;

            CultureReadCategory = installedHandlers.ContainsKey(MessageCategory.CultureRead)
                ? installedHandlers[MessageCategory.CultureRead] as ICultureReadCategory
                : null;

            NonTransactionalCritDataReadCategory = installedHandlers.ContainsKey(MessageCategory.NonTransactionalCritDataRead)
                ? installedHandlers[MessageCategory.NonTransactionalCritDataRead] as INonTransactionalCritDataReadCategory
                : null;

            TransactionalCritDataReadCategory = installedHandlers.ContainsKey(MessageCategory.TransactionalCritDataRead)
                ? installedHandlers[MessageCategory.TransactionalCritDataRead] as ITransactionalCritDataReadCategory
                : null;

            TransactionalCritDataWriteCategory = installedHandlers.ContainsKey(MessageCategory.TransactionalCritDataWrite)
                ? installedHandlers[MessageCategory.TransactionalCritDataWrite] as ITransactionalCritDataWriteCategory
                : null;

            GameInformationCategory = installedHandlers.ContainsKey(MessageCategory.GameInformation)
                ? installedHandlers[MessageCategory.GameInformation] as IGameInformationCategory
                : null;

            TiltControlCategory = installedHandlers.ContainsKey(MessageCategory.TiltControl)
                ? installedHandlers[MessageCategory.TiltControl] as ITiltControlCategory
                : null;

            ParcelCommCategory = installedHandlers.ContainsKey(MessageCategory.ParcelComm)
                ? installedHandlers[MessageCategory.ParcelComm] as IParcelCommCategory
                : null;

            BankStatusCategory = installedHandlers.ContainsKey(MessageCategory.BankStatus)
                ? installedHandlers[MessageCategory.BankStatus] as IBankStatusCategory
                : null;
        }

        #endregion

        #region IThemeApiCallbacks Members

        /// <inheritdoc/>
        public void ProcessThemeApiStart(string themeIdentifier, IEnumerable<Extension> extensions)
        {
            if(string.IsNullOrEmpty(themeIdentifier))
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            AscribedGameEntity = new AscribedGameEntity(AscribedGameType.Theme, themeIdentifier);
            linkedAscribedGameExtensions = extensions.ToList();
        }

        /// <inheritdoc/>
        public void ProcessThemeApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
        }

        #endregion

        #region IAscribedGameApiCallbacks Members

        /// <inheritdoc/>
        public void ProcessAscribedGameApiStart(AscribedGame ascribedGame, IEnumerable<Extension> extensions)
        {
            if(ascribedGame == null)
            {
                throw new ArgumentNullException(nameof(ascribedGame));
            }

            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            // AscribedGame could be either a theme id or shell id.
            if(ascribedGame.Item is ThemeIdentifier themeId)
            {
                AscribedGameEntity = new AscribedGameEntity(AscribedGameType.Theme, themeId.Value);
            }
            else
            {
                if(ascribedGame.Item is ShellIdentifier shellId)
                {
                    AscribedGameEntity = new AscribedGameEntity(AscribedGameType.Shell, shellId.Value);
                }
                else
                {
                    throw new ArgumentException("Not supported type of identifier in ascribedGame!");
                }
            }
            linkedAscribedGameExtensions = extensions.ToList();
        }

        /// <inheritdoc/>
        public void ProcessAscribedGameApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
        }

        #endregion

        #region ITsmApiCallbacks Members

        /// <inheritdoc/>
        public void ProcessTsmApiStart(string tsmIdentifier, IEnumerable<Extension> extensions)
        {
            if(string.IsNullOrEmpty(tsmIdentifier))
            {
                throw new ArgumentNullException(nameof(tsmIdentifier));
            }

            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            TsmId = tsmIdentifier;
            linkedTsmExtensions = extensions.ToList();
        }

        /// <inheritdoc/>
        public void ProcessTsmApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
        }

        #endregion

        #region IAppApiCallbacks Members

        /// <inheritdoc/>
        public void ProcessAppApiStart(IEnumerable<Extension> extensions)
        {
            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            // Make sure that the number of activated app extensions is one.
            var extensionList = extensions.ToList();
            if(extensionList.Count != 1)
            {
                throw new InvalidOperationException(
                    $"The number of activated app extensions must be one, it is actually: {extensionList.Count}.");
            }

            AppExtensionContext = new AppExtensionContext(new Guid(extensionList.First().ExtensionIdentifier));
        }

        /// <inheritdoc/>
        public void ProcessAppApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
        }

        #endregion
    }
}
