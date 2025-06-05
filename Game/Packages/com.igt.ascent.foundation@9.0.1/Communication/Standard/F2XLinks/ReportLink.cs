//-----------------------------------------------------------------------
// <copyright file = "ReportLink.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2XCallbacks;
    using F2XTransport;
    using F2X.Schemas.Internal.DiscoveryContextTypes;
    using F2X.Schemas.Internal.LinkControl;
    using GameReport.Interfaces;

    /// <summary>
    /// This class provides a link to the Foundation for game reporting.
    /// </summary>
    internal class ReportLink : LinkBase
    {
        #region Public Properties

        /// <summary>
        /// Gets the theme identifier currently being processed for report.
        /// </summary>
        public string ThemeIdentifier { get; private set; }

        /// <summary>
        /// Gets the category for querying game information.
        /// </summary>
        public IGameInformationCategory GameInformation { get; private set; }

        /// <summary>
        /// Gets the category for sending progressive data queries.
        /// </summary>
        public IProgressiveDataCategory ProgressiveData { get; private set; }

        /// <summary>
        /// Gets the category for requesting localization information.
        /// </summary>
        public ILocalizationCategory Localization { get; private set; }

        /// <summary>
        /// Gets the category for requesting EGM-wide config data.
        /// </summary>
        public IEgmConfigDataCategory EgmConfigData { get; private set; }

        /// <summary>
        /// Gets the category for requesting information about custom configuration items.
        /// </summary>
        public ICustomConfigurationReadCategory CustomConfigurationRead { get; private set; }

        /// <summary>
        /// Gets the category for non-transactional reading critical data.
        /// </summary>
        public INonTransactionalCritDataReadCategory NonTransactionalCritDataRead { get; private set; }

        /// <summary>
        /// Gets the category for transactional reading critical data.
        /// </summary>
        public ITransactionalCritDataReadCategory TransactionalCritDataRead { get; private set; }

        /// <summary>
        /// Gets the category for querying game group information.
        /// </summary>
        public IGameGroupInformationCategory GameGroupInformation { get; private set; }

        /// <summary>
        /// Gets the category for transactional writing critical data.
        /// </summary>
        public ITransactionalCritDataWriteCategory TransactionalCritDataWrite { get; private set; }

        #endregion

        #region Private Fields

        /// <summary>
        /// The link level API Manager.
        /// </summary>
        private readonly LinkApiManager linkApiManager =
            new LinkApiManager
            (
                new List<CategorySubscription>
                    {
                        // Set the category required being true only when all currently supported foundation
                        // targets support the category.
                        new CategorySubscription(MessageCategory.Activation, true),
                        new CategorySubscription(MessageCategory.GameInformation, true),
                        new CategorySubscription(MessageCategory.ProgressiveData, true),
                        new CategorySubscription(MessageCategory.Localization, true),
                        new CategorySubscription(MessageCategory.EgmConfigData, true),
                        new CategorySubscription(MessageCategory.CustomConfigurationRead, true),
                        new CategorySubscription(MessageCategory.NonTransactionalCritDataRead, true),
                        new CategorySubscription(MessageCategory.TransactionalCritDataRead, true),
                        new CategorySubscription(MessageCategory.GameGroupInformation, false),
                        new CategorySubscription(MessageCategory.TransactionalCritDataWrite, false),
                    }
            );

        /// <summary>
        /// Lookup table from reporting service type to their related category subscriptions.
        /// </summary>
        private readonly Dictionary<ReportingServiceType, List<CategorySubscription>> serviceSpecificSubscriptions =
            new Dictionary<ReportingServiceType, List<CategorySubscription>>
            {
                {
                    ReportingServiceType.GameDataInspection,
                    new List<CategorySubscription>
                    {
                        new CategorySubscription(MessageCategory.ReportGameDataInspection, true)
                    }
                },
                {
                    ReportingServiceType.GameDataHtmlInspection,
                    new List<CategorySubscription>
                    {
                        new CategorySubscription(MessageCategory.ReportGameDataInspection, true)
                    }
                },
                {
                    ReportingServiceType.GameLevelAward,
                    new List<CategorySubscription>
                    {
                        new CategorySubscription(MessageCategory.GameLevelAward, true)
                    }
                },
                {
                    ReportingServiceType.SetupValidation,
                    new List<CategorySubscription>
                    {
                        new CategorySubscription(MessageCategory.SetupValidation, true)
                    }
                },
                {
                    ReportingServiceType.GamePerformance,
                    new List<CategorySubscription>
                    {
                        new CategorySubscription(MessageCategory.ReportGamePerformance, true)
                    }
                },
            };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ReportLink"/>.
        /// </summary>
        /// <param name="address">Foundation address to connect to.</param>
        /// <param name="port">Foundation port to connect to.</param>
        /// <param name="foundationTarget">Foundation version to target.</param>
        /// <param name="nonTransactionalEventCallbacks">The callback interface for handling non-transactional events.</param>
        public ReportLink(string address, ushort port, FoundationTarget foundationTarget,
                          INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            var categoryDependencies = new CategoryNegotiationDependencies
                                           {
                                               EventCallbacks = this,
                                               NonTransactionalEventCallbacks = nonTransactionalEventCallbacks,
                                               TransactionCallbacks = this,
                                               LinkControlCategoryCallbacks = linkApiManager,
                                           };

            LinkController = new LinkController(address, port, foundationTarget,
                                                linkStatusCallbacks: this,
                                                categoryManagers: new List<IApiManager> { linkApiManager },
                                                categoryDependencies: categoryDependencies);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Establishes a connect to the Foundation.
        /// </summary>
        /// <param name="reportingServiceTypes">
        /// The flags of reporting services that will be supported.
        /// </param>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reportingServiceTypes"/> is null.
        /// </exception>
        public bool Connect(IEnumerable<ReportingServiceType> reportingServiceTypes)
        {
            if(reportingServiceTypes == null)
            {
                throw new ArgumentNullException("reportingServiceTypes");
            }

            linkApiManager.AddSubscriptions(GetReportingSubscriptions(reportingServiceTypes));

            return Connect();
        }

        #endregion

        #region LinkBase Overrides

        /// <inheritdoc/>
        protected override DiscoveryType MountPointDiscoveryType
        {
            get { return DiscoveryType.Bin; }
        }

        /// <inheritdoc/>
        public override void ProcessLinkStart(string jurisdiction, string connectToken,
                                              ICollection<DiscoveryContext> discoveryContexts,
                                              ICollection<ExtensionImport> extensionImports)
        {
            base.ProcessLinkStart(jurisdiction, connectToken, discoveryContexts, extensionImports);

            // There should be only one theme at a time.
            var themeContext = discoveryContexts.FirstOrDefault(
                discoveryContext => discoveryContext.DiscoveryType == DiscoveryType.Theme);

            ThemeIdentifier = themeContext != null ? themeContext.Identifier : null;
        }

        /// <inheritdoc />
        public override void ProcessLinkNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            base.ProcessLinkNegotiation(installedHandlers);

            GameInformation = installedHandlers.ContainsKey(MessageCategory.GameInformation)
                ? installedHandlers[MessageCategory.GameInformation] as IGameInformationCategory
                : null;

            ProgressiveData = installedHandlers.ContainsKey(MessageCategory.ProgressiveData)
                ? installedHandlers[MessageCategory.ProgressiveData] as IProgressiveDataCategory
                : null;

            Localization = installedHandlers.ContainsKey(MessageCategory.Localization)
                ? installedHandlers[MessageCategory.Localization] as ILocalizationCategory
                : null;

            EgmConfigData = installedHandlers.ContainsKey(MessageCategory.EgmConfigData)
                ? installedHandlers[MessageCategory.EgmConfigData] as IEgmConfigDataCategory
                : null;

            CustomConfigurationRead = installedHandlers.ContainsKey(MessageCategory.CustomConfigurationRead)
                ? installedHandlers[MessageCategory.CustomConfigurationRead] as ICustomConfigurationReadCategory
                : null;

            NonTransactionalCritDataRead = installedHandlers.ContainsKey(MessageCategory.NonTransactionalCritDataRead)
                ? installedHandlers[MessageCategory.NonTransactionalCritDataRead] as INonTransactionalCritDataReadCategory
                : null;

            TransactionalCritDataRead = installedHandlers.ContainsKey(MessageCategory.TransactionalCritDataRead)
                ? installedHandlers[MessageCategory.TransactionalCritDataRead] as ITransactionalCritDataReadCategory
                : null;

            GameGroupInformation = installedHandlers.ContainsKey(MessageCategory.GameGroupInformation)
                ? installedHandlers[MessageCategory.GameGroupInformation] as IGameGroupInformationCategory
                : null;

            TransactionalCritDataWrite = installedHandlers.ContainsKey(MessageCategory.TransactionalCritDataWrite)
                ? installedHandlers[MessageCategory.TransactionalCritDataWrite] as ITransactionalCritDataWriteCategory
                : null;
        }

        /// <inheritdoc />
        public override bool ActionRequest(byte[] payload)
        {
            // F2R does not support ActionRequest category.
            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the service specific category subscriptions.
        /// </summary>
        /// <param name="reportingServiceTypes">
        /// The supported reporting service types.
        /// </param>
        /// <returns>
        /// The service specific category subscriptions.
        /// </returns>
        private IList<CategorySubscription> GetReportingSubscriptions(IEnumerable<ReportingServiceType> reportingServiceTypes)
        {
            var subscriptions = new List<CategorySubscription>();

            foreach(var reportingServiceType in reportingServiceTypes)
            {
                subscriptions.AddRange(serviceSpecificSubscriptions[reportingServiceType]);
            }

            return subscriptions;
        }

        #endregion
    }
}