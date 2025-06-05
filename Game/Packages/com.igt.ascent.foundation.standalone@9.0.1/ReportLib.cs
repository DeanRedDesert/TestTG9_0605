//-----------------------------------------------------------------------
// <copyright file = "ReportLib.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Xml.Serialization;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Communication.Standalone.Schemas;
    using GameReport.Interfaces;
    using Registries;
    using TopScreenGameAdvertisementType = Ascent.Communication.Platform.Interfaces.TopScreenGameAdvertisementType;

    /// <summary>
    /// Standalone implementation of <see cref="IReportLib"/> that
    /// imitates the Foundation for game reporting.
    /// </summary>
    /// <remarks>
    /// This class relies on the SystemConfig file but does not
    /// perform any validation against the registries as done by GameLib.
    /// </remarks>
    public class ReportLib : IReportLib, IReportLibRestricted, IEgmConfigData, IDisposable,
                             ICriticalDataAccessValidation
    {
        #region Constant Fields

        /// <summary>
        /// Number of events to be processed per each ProcessEvents call
        /// for the Standalone version.
        /// </summary>
        private const int EventProcessPace = 1;

        /// <summary>
        /// Separator string used in the reports.
        /// </summary>
        private readonly string reportSeparator = new string('*', 70);

        /// <summary>
        /// Format string for writing report items which left-aligns the labels and values.
        /// </summary>
        private const string ReportItemFormatString = "{0,-35}{1}";

        /// <summary>
        /// Format string for the game report files that includes the culture.
        /// </summary>
        private const string GameReportFileNameFormat = "GameReport_{0}.txt";

        /// <summary>
        /// Format string for the Html game report files that includes the culture.
        /// </summary>
        private const string GameReportHtmlFileNameFormat = "GameReport_{0}.html";

        /// <summary>
        /// Name for the min playable credit balance report file.
        /// </summary>
        private const string MinPlayableCreditBalanceReportFileName = "MinPlayableCreditBalanceReport.txt";

        /// <summary>
        /// Name for the game level award report file.
        /// </summary>
        private const string GameLevelAwardReportFileNameFormat = "GameLevelAwardReport_{0}.txt";

        /// <summary>
        /// Name for the setup validation report file.
        /// </summary>
        private const string SetupValidationReportFileName = "SetupValidationReport.txt";

        /// <summary>
        /// The game performance data file name.
        /// </summary>
        private const string GamePerformanceReportFileName = "GamePerformanceReport.txt";

        /// <summary>
        /// Wild card search pattern for the game report files.
        /// </summary>
        private const string GameReportFileNameSearchPattern = "GameReport*.txt";

        /// <summary>
        /// Wild card search pattern for the game html report files.
        /// </summary>
        private const string GameHtmlReportFileNameSearchPattern = "GameReport*.html";

        /// <summary>
        /// Wild card search pattern for the game level award report files.
        /// </summary>
        private const string GameLevelAwardReportFileNameSearchPattern = "GameLevelAwardReport*.txt";

        /// <summary>
        /// The file name of the SystemConfig file.
        /// </summary>
        private const string SystemConfigFileName = "SystemConfig.xml";

        /// <summary>
        /// The file name of the Modifier file.
        /// </summary>
        private const string ModifierPath = "Mod.safestorage";

        /// <summary>
        /// The file name of the Committed file.
        /// </summary>
        private const string CommittedPath = "Com.safestorage";

        /// <summary>
        /// The maximum number of cultures to use for game reports for each paytable.
        /// </summary>
        private const int MaxCultures = 3;

        #endregion

        #region Private Fields

        /// <summary>
        /// Locker object guarding the event related fields, including
        /// foundationTransactionOpen and foundationEvents.
        /// </summary>
        private readonly object eventLocker = new object();

        /// <summary>
        /// Event used for blocking process event calls.
        /// </summary>
        private readonly AutoResetEvent eventResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable;

        /// <summary>
        /// Event queue to hold the incoming events posted by the Foundation.
        /// </summary>
        /// <remarks>Guarded by eventLocker.</remarks>
        /// <devdoc>
        /// There may not be a way for there to be multiple events
        /// but for now the lib will assume support for it.
        /// </devdoc>
        private readonly List<EventArgs> foundationEvents = new List<EventArgs>();

        /// <summary>
        /// Dictionary of the game's loaded registries.
        /// </summary>
        private readonly IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries;

        /// <summary>
        /// Dictionary of PaytableBindings used to generate reports.
        /// </summary>
        /// <remarks>
        /// Key of the IDictionary is the paytable identifier which represents a paytable
        /// configuration.  The key of the value IDictionary is the denomination.
        /// </remarks>
        private readonly IDictionary<string, IDictionary<long, PaytableBinding>> paytableBindings;

        /// <summary>
        /// The system configurations loaded from the SystemConfig.xml file.
        /// </summary>
        private readonly SystemConfigurations systemConfigurations;

        /// <summary>
        /// Implementation that provides theme and payvar registry configuration items
        /// through the game's registry files.
        /// </summary>
        private readonly ConfigurationRead configurationRead;

        /// <summary>
        /// Implementation that provides accessing game information to support game report.
        /// </summary>
        private readonly GameInformation gameInformation;

        /// <summary>
        /// Provider of localization information.
        /// </summary>
        private readonly LocalizationInformation localization = new LocalizationInformation();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// This flag indicates if a Foundation initiated transaction is currently open.
        /// The Standalone Game Lib simulates to open a Foundation initiated transaction
        /// when processing events.
        /// </summary>
        /// <remarks>Guarded by eventLocker.</remarks>
        private bool foundationTransactionOpen;

        /// <summary>
        /// Transaction id is used for debugging to indicate
        /// the number of transactions which have been performed.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int transactionId;

        /// <summary>
        /// Lookup table of the Game Report functionality targets to their implementations.
        /// </summary>
        private readonly Dictionary<ReportingServiceType, Action> reportingServiceInitiators;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ReportLib"/>.
        /// </summary>
        public ReportLib()
        {
            // Please keep the keys in alphabetical order.
            eventTable = new Dictionary<Type, EventHandler>
                             {
                                 {
                                     typeof(ActivateContextEventArgs),
                                     (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, ActivateContextEvent)
                                 },
                                 {
                                     typeof(EventArgs),
                                     ExecuteShutDownEvent
                                 },
                                 {
                                     typeof(GenerateGamePerformanceReportEventArgs),
                                     ExecuteGenerateGamePerformanceReportEvent
                                 },
                                 {
                                     typeof(GenerateHtmlInspectionReportEventArgs),
                                     ExecuteGenerateHtmlInspectionReportEvent
                                 },
                                 {
                                     typeof(GenerateInspectionReportEventArgs),
                                     ExecuteGenerateInspectionReportEvent
                                 },
                                 {
                                     typeof(GetGameLevelValuesEventArgs),
                                     ExecuteGetGameLevelValuesEvent
                                 },
                                 {
                                     typeof(GetMinPlayableCreditBalanceEventArgs),
                                     ExecuteGetMinPlayableCreditBalanceEvent
                                 },
                                 {
                                     typeof(InactivateContextEventArgs),
                                     (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, InactivateContextEvent)
                                 },
                                 {
                                     typeof(InitializeGameLevelDataEventArgs),
                                     (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, InitializeGameLevelDataEvent)
                                 },
                                 {
                                     typeof(ValidateThemeSetupEventArgs),
                                     ExecuteSetupValidation
                                 }
                             };

            DeleteExistingReportFiles();

            // Load registries.
            registries = RegistryLoader.Load(MountPoint);

            // Load system configurations.
            systemConfigurations = LoadSystemConfigurations(MountPoint);

            // Get paytable bindings based on registries and system config file.
            paytableBindings = RetrievePaytableBindings(systemConfigurations.PaytableList, registries);

            // Instantiate ConfigurationRead.
            var paytableIdentifierList = new List<string>();
            paytableIdentifierList.AddRange(paytableBindings.Keys);

            configurationRead = new ConfigurationRead(registries, paytableIdentifierList);

            // Instantiate GameInformation.
            gameInformation = new GameInformation(registries, paytableBindings, MountPoint);

            // Instantiate CriticalDataAccessor.
            var registryIndexer = new RegistryIndexer(registries);
            var diskStoreSectionIndexer = new DiskStoreSectionIndexer(registryIndexer);

            var diskStoreManager = new CompressedBinaryDiskStoreManager(Path.Combine(Directory.GetCurrentDirectory(), ModifierPath),
                                                                        Path.Combine(Directory.GetCurrentDirectory(), CommittedPath));

            CriticalDataAccessor = new CriticalDataAccessor(diskStoreManager, true, diskStoreSectionIndexer, this);

            // "Entry points" for each reporting service type.
            reportingServiceInitiators = new Dictionary<ReportingServiceType, Action>
                                             {
                                                 { ReportingServiceType.GameDataHtmlInspection, InitiateGameDataHtmlInspection },
                                                 { ReportingServiceType.GameDataInspection, InitiateGameDataInspection },
                                                 { ReportingServiceType.GameLevelAward, InitiateGameLevelAward },
                                                 { ReportingServiceType.GamePerformance, InitiateGamePerformance },
                                                 { ReportingServiceType.SetupValidation, InitiateSetupValidation }
                                             };
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
        public string MountPoint => Directory.GetCurrentDirectory();

        /// <inheritdoc/>
        public string Jurisdiction => "USDM";

        /// <inheritdoc/>
        public ILocalizationInformation LocalizationInformation => localization;

        /// <inheritdoc/>
        public IEgmConfigData EgmConfigData => this;

        /// <inheritdoc/>
        public IConfigurationRead ConfigurationRead => configurationRead;

        /// <inheritdoc/>
        public ICriticalDataAccessor CriticalDataAccessor { get; }

        /// <inheritdoc/>
        public IGameInformation GameInformation => gameInformation;

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="paytableIdentifier"/> is not found in paytableBindings.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when paytable binding for <paramref name="paytableIdentifier"/> and <paramref name="denomination"/> is not found.
        /// </exception>
        public IDictionary<int, ProgressiveSettings> GetLinkedProgressiveSettings(string paytableIdentifier,
                                                                                  long denomination)
        {
            if(!paytableBindings.ContainsKey(paytableIdentifier))
            {
                throw new ArgumentException(
                    $"Paytable identifier {paytableIdentifier} is not found.", nameof(paytableIdentifier));
            }

            var returnDictionary = new Dictionary<int, ProgressiveSettings>();

            if(systemConfigurations.SystemControlledProgressives != null)
            {
                var paytable = paytableBindings[paytableIdentifier][denomination];
                if(paytable == null)
                {
                    throw new ArgumentException(
                        $"Paytable {paytableIdentifier} and denomination {denomination} are not found in the paytableBindings.", nameof(paytableIdentifier));
                }

                var progressiveLinks = (from setup in systemConfigurations.SystemControlledProgressives
                                                                          .ProgressiveSetups
                                        let config = setup.PaytableConfiguration
                                        let paytableFileName = Utility.UniformSlashes(config.PaytableFileName)
                                        where config.ThemeIdentifier == paytable.ThemeIdentifier &&
                                              paytableFileName == paytable.PaytableFileName &&
                                              config.PaytableName == paytable.PaytableName &&
                                              config.Denomination == denomination
                                        select setup)
                    .SelectMany(setup => setup.ProgressiveLink);

                foreach(var progressiveLink in progressiveLinks)
                {
                    var targetController = systemConfigurations.SystemControlledProgressives.ProgressiveControllers
                                               .FirstOrDefault(controller => controller.Name == progressiveLink.ControllerName);

                    var targetControllerLevel = targetController?.ControllerLevel
                        .FirstOrDefault(level => level.Id == progressiveLink.ControllerLevel);

                    // Should never be null.
                    if(targetControllerLevel == null)
                    {
                        break;
                    }

                    returnDictionary.Add(
                        progressiveLink.GameLevel,
                        new ProgressiveSettings
                        {
                            StartAmount = targetControllerLevel.StartingAmount,
                            MaxAmount = targetControllerLevel.MaximumAmount,
                            // SystemConfig progressive settings actually store the contribution as a rate.
                            // Converting it to percentage here to match with the Foundation.
                            ContributionPercentage = (decimal)targetControllerLevel.ContributionPercentage * 100
                        });
                }
            }

            return returnDictionary;
        }

        #endregion

        #region IReportLibRestricted Members

        /// <inheritdoc/>
        /// <devdocs>
        /// Posts inactivate context event and generate report events for every paytable/denomination
        /// in SystemConfig.xml  and for the first three languages in the theme registry.
        /// Finally, a shutdown event is posted.  The events will not be processed until
        /// <see cref="ProcessEvents"/> is called.
        /// </devdocs>
        public bool ConnectToFoundation(IEnumerable<ReportingServiceType> reportingServiceTypes)
        {
            if(reportingServiceTypes == null)
            {
                throw new ArgumentNullException(nameof(reportingServiceTypes));
            }

            foreach(var reportingServiceType in reportingServiceTypes)
            {
                reportingServiceInitiators[reportingServiceType]();
            }

            // Post shutdown event
            PostEvent(EventArgs.Empty);

            return true;
        }

        /// <inheritdoc/>
        public bool DisconnectFromFoundation()
        {
            return true;
        }

        /// <inheritdoc/>
        public WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles)
        {
            // Simulate a Foundation initiated event.
            CreateFoundationTransaction();

            bool pendingEvents;
            lock(eventLocker)
            {
                pendingEvents = foundationEvents.Count > 0;
            }

            // Process any event that is already in the queue.
            // Check and reset the reset event for foundation events. There could be multiple events because of
            // pace control, so the count must also be checked.
            if(eventResetEvent.WaitOne(0) || pendingEvents)
            {
                ExecuteEvents(EventProcessPace);
            }

            // Close the Foundation initiated event.
            CloseFoundationTransaction();

            return null;
        }

        #endregion

        #region IEgmConfigData Members

        /// <inheritdoc/>
        public long GetMinimumBet()
        {
            return systemConfigurations.FoundationOwnedSettings.GameMinBetSpecified
                       ? systemConfigurations.FoundationOwnedSettings.GameMinBet
                       : 0;
        }

        /// <inheritdoc/>
        public WinCapBehaviorInfo GetWinCapBehaviorInfo()
        {
            return new WinCapBehaviorInfo
                (
                    (WinCapBehavior)systemConfigurations.FoundationOwnedSettings.WinCapBehavior,
                    systemConfigurations.FoundationOwnedSettings.WinCapLimitSpecified
                        ? systemConfigurations.FoundationOwnedSettings.WinCapLimit
                        : 0,
                    systemConfigurations.FoundationOwnedSettings.WinCapMultiplierSpecified
                        ? systemConfigurations.FoundationOwnedSettings.WinCapMultiplier
                        : 0
                );
        }

        /// <inheritdoc/>
        public long GetAncillaryMonetaryLimit()
        {
            return systemConfigurations.FoundationOwnedSettings.AncillarySetting?.MonetaryLimit ?? 0;
        }

        /// <inheritdoc/>
        public MarketingBehavior GetMarketingBehavior()
        {
            return systemConfigurations.FoundationOwnedSettings.MarketingBehavior?.TopScreenGameAdvertisementSpecified == true
                ? new MarketingBehavior { TopScreenGameAdvertisement = 
                    (TopScreenGameAdvertisementType)systemConfigurations.FoundationOwnedSettings.MarketingBehavior.TopScreenGameAdvertisement }
                : new MarketingBehavior();
        }

        /// <inheritdoc/>
        public BetSelectionStyleInfo GetDefaultBetSelectionStyle()
        {
            return new BetSelectionStyleInfo(BetSelectionBehavior.Undefined,
                                             BetSelectionBehavior.Undefined,
                                             SideBetSelectionBehavior.Undefined);
        }

        /// <inheritdoc/>
        public long GetProgressiveWinCap()
        {
            return systemConfigurations.FoundationOwnedSettings.ProgressiveWinCapLimit;
        }

        /// <inheritdoc/>
        public long GetTotalWinCap()
        {
            return systemConfigurations.FoundationOwnedSettings.TotalWinCapLimit;
        }

        /// <inheritdoc />
        public bool GetDisplayVideoReelsForStepper()
        {
            return systemConfigurations.FoundationOwnedSettings.DisplayVideoReelsForStepper;
        }

        /// <inheritdoc />
        public BonusSoaaSettings GetBonusSoaaSettings()
        {
            var settings = systemConfigurations.FoundationOwnedSettings.BonusSoaaSettings;

            return settings?.Supported == true
                       ? settings.IsAllowed
                             ? new BonusSoaaSettings(true, (uint)settings.MinDelaySeconds)
                             : new BonusSoaaSettings(false)
                       : null;
        }

        /// <inheritdoc />
        public bool GetRtpOrderedByBetRequired()
        {
            return systemConfigurations.FoundationOwnedSettings.RtpOrderedByBetRequired;
        }

        #endregion

        #region ICriticalDataAccessValidation Members

        /// <inheritdoc />
        void ICriticalDataAccessValidation.ValidateCriticalDataAccess(
            IList<CriticalDataSelector> criticalDataSelectors, DataAccessing dataAccessing)
        {
            if(criticalDataSelectors == null)
            {
                throw new ArgumentNullException(nameof(criticalDataSelectors));
            }

            if(dataAccessing != DataAccessing.Read)
            {
                throw new CriticalDataAccessDeniedException(
                    $"The accessing {dataAccessing} is not supported in reports.");
            }

            var selectors =
                criticalDataSelectors.Where(selector =>
                                                !SupportedCriticalDataScopeTable.IsScopeAllowed(
                                                    CriticalDataScopeClientType.Report,
                                                    selector.Scope)).ToArray();
            if(selectors.Any())
            {
                throw new CriticalDataAccessDeniedException(
                    $"The critical data scope [{string.Join(",", selectors.Select(selector => selector.Scope.ToString()).ToArray())}] are not accessible to reports.");
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Deletes any existing GameReport files.
        /// </summary>
        /// <devdoc>
        /// Internal only for unit testing purpose.
        /// </devdoc>
        internal void DeleteExistingReportFiles()
        {
            foreach(var reportFileName in Directory.GetFiles(MountPoint, GameReportFileNameSearchPattern)
                                                   .Concat(Directory.GetFiles(MountPoint, GameHtmlReportFileNameSearchPattern))
                                                   .Concat(Directory.GetFiles(MountPoint, MinPlayableCreditBalanceReportFileName))
                                                   .Concat(Directory.GetFiles(MountPoint, GameLevelAwardReportFileNameSearchPattern))
                                                   .Concat(Directory.GetFiles(MountPoint, SetupValidationReportFileName))
                                                   .Concat(Directory.GetFiles(MountPoint, GamePerformanceReportFileName)))
            {
                File.Delete(reportFileName);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads standalone settings from SystemConfig file.
        /// </summary>
        /// <returns>The loaded <see cref="SystemConfigurations"/>.</returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown when SystemConfig.xml cannot be found.
        /// </exception>
        private static SystemConfigurations LoadSystemConfigurations(string mountPoint)
        {
            SystemConfigurations configurations;
            var systemConfigFile = Path.Combine(mountPoint, SystemConfigFileName);

            if(!File.Exists(systemConfigFile))
            {
                throw new FileNotFoundException($"Cannot find {SystemConfigFileName} at {mountPoint}");
            }

            var serializer = new XmlSerializer(typeof(SystemConfigurations));
            using(var reader = new FileStream(systemConfigFile, FileMode.Open, FileAccess.Read))
            {
                configurations = serializer.Deserialize(reader) as SystemConfigurations;
                if(configurations == null)
                {
                    throw new ApplicationException($"Failed to load {SystemConfigFileName}.");
                }

                reader.Close();
            }

            return configurations;
        }

        /// <summary>
        /// Retrieves paytable bindings based on registries and the paytable list in the system config file.
        /// </summary>
        /// <param name="paytableList">The paytable list in the system config file.</param>
        /// <param name="allRegistries">The registries loaded.</param>
        /// <returns>
        /// A dictionary keyed by paytable identifier. For each identifier, there are paytable bindings
        /// that are keyed by the denomination.
        /// </returns>
        private static IDictionary<string, IDictionary<long, PaytableBinding>> RetrievePaytableBindings(IList<PaytableListPaytableConfiguration> paytableList,
                                                                                                        IDictionary<IThemeRegistry, IList<IPayvarRegistry>> allRegistries)
        {
            var result = new Dictionary<string, IDictionary<long, PaytableBinding>>();

            if(paytableList != null)
            {
                foreach(var paytableConfig in paytableList)
                {
                    // Store path with uniform slashes for comparison in GetLinkedProgressiveSettings
                    paytableConfig.PaytableFileName = Utility.UniformSlashes(paytableConfig.PaytableFileName);

                    // Get the Payvar Name from the registry which is guaranteed to be unique
                    var payvarRegistry = allRegistries.First(registrySet => registrySet.Key.G2SThemeId == paytableConfig.ThemeIdentifier)
                                                      .Value
                                                      .First(payvar => payvar.PaytableTagName == paytableConfig.PaytableName &&
                                                                       payvar.PaytableTagFileName == paytableConfig.PaytableFileName);

                    // Add bindings for payvars in group if present to replace the group template binding.
                    if(payvarRegistry.PayvarGroupRegistry != null)
                    {
                        foreach(var payvar in payvarRegistry.PayvarGroupRegistry.Payvars)
                        {
                            var paytableIdentifier = payvar.PaytableIdentifier;

                            if(!result.ContainsKey(paytableIdentifier))
                            {
                                result.Add(paytableIdentifier,
                                           new Dictionary<long, PaytableBinding>());
                            }

                            // The original (group template) paytable binding is stored unmodified 
                            // because its values are used for progressive setups.
                            result[paytableIdentifier].Add(paytableConfig.Denomination, paytableConfig);
                        }
                    }
                    else
                    {
                        var paytableIdentifier = payvarRegistry.PaytableIdentifier;
                        if(!result.ContainsKey(paytableIdentifier))
                        {
                            result.Add(paytableIdentifier,
                                       new Dictionary<long, PaytableBinding>());
                        }

                        result[paytableIdentifier].Add(paytableConfig.Denomination, paytableConfig);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Initiates the functionality of game data inspection reporting service.
        /// </summary>
        private void InitiateGameDataInspection()
        {
            var minPlayableCreditBalanceRequests = new List<MinPlayableCreditBalanceRequest>();

            foreach(var paytablesPerPayvar in paytableBindings)
            {
                var themeRegistry = registries.First(registrySet =>
                                                         registrySet.Key.G2SThemeId == paytablesPerPayvar.Value.First().Value.ThemeIdentifier)
                                              .Key;

                foreach(var item in paytablesPerPayvar.Value)
                {
                    var paytableBinding = item.Value;
                    foreach(var culture in themeRegistry.GetSupportedCultures().Take(MaxCultures))
                    {
                        PostEvent(new GenerateInspectionReportEventArgs(culture,
                                                                        paytableBinding.ThemeIdentifier,
                                                                        paytablesPerPayvar.Key,
                                                                        paytableBinding.Denomination));
                    }

                    minPlayableCreditBalanceRequests.Add(new MinPlayableCreditBalanceRequest(paytableBinding.ThemeIdentifier,
                                                                                             paytablesPerPayvar.Key,
                                                                                             paytableBinding.Denomination));
                }
            }

            PostEvent(new GetMinPlayableCreditBalanceEventArgs(minPlayableCreditBalanceRequests));
        }

        /// <summary>
        /// Initiates the functionality of html game data inspection reporting service. In standalone
        /// game reporting mode, Html is not fully supported at this time.
        /// </summary>
        private void InitiateGameDataHtmlInspection()
        {
            var themeAndPayvarDictionary = new Dictionary<string, IList<PaytableDenominationInfo>>();
            var minPlayableCreditBalanceRequests = new List<MinPlayableCreditBalanceRequest>();

            foreach(var paytablesPerPayvar in paytableBindings)
            {
                foreach(var denomPaytableBindingPair in paytablesPerPayvar.Value)
                {
                    var paytableBinding = denomPaytableBindingPair.Value;
                    if(!themeAndPayvarDictionary.ContainsKey(paytableBinding.ThemeIdentifier))
                    {
                        themeAndPayvarDictionary[paytableBinding.ThemeIdentifier] = new List<PaytableDenominationInfo>();
                    }

                    themeAndPayvarDictionary[paytableBinding.ThemeIdentifier].Add(new PaytableDenominationInfo(paytableBinding.PaytableIdentifier,
                                                                                                               paytableBinding.Denomination));

                    minPlayableCreditBalanceRequests.Add(new MinPlayableCreditBalanceRequest(paytableBinding.ThemeIdentifier,
                                                                                             paytablesPerPayvar.Key,
                                                                                             paytableBinding.Denomination));
                }
            }

            // Use the first theme's cultures
            var cultures = registries.Keys.First().GetSupportedCultures().Take(MaxCultures);

            foreach(var culture in cultures)
            {
                PostEvent(new GenerateHtmlInspectionReportEventArgs(culture, themeAndPayvarDictionary));
            }

            PostEvent(new GetMinPlayableCreditBalanceEventArgs(minPlayableCreditBalanceRequests));
        }

        /// <summary>
        /// Initiates the functionality of game level award reporting service.
        /// </summary>
        private void InitiateGameLevelAward()
        {
            var themeIdentifierList = new Dictionary<string, Dictionary<string, KeyValuePair<string, PaytableBinding>>>();
            foreach(var item in paytableBindings)
            {
                foreach(var subItem in item.Value)
                {
                    if(!themeIdentifierList.ContainsKey(subItem.Value.ThemeIdentifier))
                    {
                        themeIdentifierList.Add(subItem.Value.ThemeIdentifier, new Dictionary<string, KeyValuePair<string, PaytableBinding>>());
                    }
                    themeIdentifierList[subItem.Value.ThemeIdentifier].Add($"{item.Key}_{subItem.Key}", new KeyValuePair<string, PaytableBinding>(item.Key, subItem.Value));
                }
            }

            foreach(var themeIdentifier in themeIdentifierList)
            {
                var paytableDenominationPairs =
                    themeIdentifier.Value.Select(
                        item =>
                            new PaytableDenominationInfo(item.Value.Value.PaytableIdentifier,
                                item.Value.Value.Denomination)).ToList();

                PostEvent(new InitializeGameLevelDataEventArgs(themeIdentifier.Key, paytableDenominationPairs));

                var gameLevelValues = new Dictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>>();
                foreach(var paytableBinding in themeIdentifier.Value)
                {
                    var gameLevelKey = new PaytableDenominationInfo(paytableBinding.Value.Value.PaytableIdentifier,
                                                                    paytableBinding.Value.Value.Denomination);

                    // Game Groups will cause duplicate paytable bindings keyed by differing group payvar names.
                    if(!gameLevelValues.ContainsKey(gameLevelKey))
                    {
                        var progressiveSettings = GetLinkedProgressiveSettings(paytableBinding.Value.Key,
                                                                               paytableBinding.Value.Value.Denomination);
                        var progressiveData = progressiveSettings
                            .Select(setting => new GameLevelLinkedData((uint)setting.Key,
                                                                       new GameLevelLinkUpValue(
                                                                           setting.Value.StartAmount)))
                            .ToList();

                        gameLevelValues.Add(gameLevelKey, progressiveData);
                    }
                }
                PostEvent(new GetGameLevelValuesEventArgs(themeIdentifier.Key, gameLevelValues));
                PostEvent(new InactivateContextEventArgs());
            }
        }

        /// <summary>
        /// Initiates the functionality of setup validation reporting service.
        /// </summary>
        private void InitiateSetupValidation()
        {
            foreach(var themePair in registries)
            {
                var themeIdentifier = themePair.Key.G2SThemeId;

                PostEvent(new ValidateThemeSetupEventArgs(themeIdentifier));
            }
        }

        /// <summary>
        /// Initiates the functionality of game performance reporting service.
        /// </summary>
        private void InitiateGamePerformance()
        {
            foreach(var paytablePair in paytableBindings)
            {
                PostEvent(new GenerateGamePerformanceReportEventArgs(paytablePair.Value.First().Value.ThemeIdentifier, paytablePair.Key));
            }
        }

        /// <summary>
        /// Standalone GameLib specific function for posting events.
        /// The function is thread safe and events may be posted from
        /// another thread. If the processEvents function is currently
        /// waiting for an event, then this function will notify it of
        /// a change in events.
        /// </summary>
        /// <param name="gameLibEvent">The event to post.</param>
        private void PostEvent(EventArgs gameLibEvent)
        {
            lock(eventLocker)
            {
                foundationEvents.Add(gameLibEvent);
                eventResetEvent.Set();
            }
        }

        /// <summary>
        /// Distribute any pending events.
        /// </summary>
        /// <param name="pace">Number of events to be processed per function call.</param>
        private void ExecuteEvents(int pace)
        {
            lock(eventLocker)
            {
                if(foundationEvents.Count > 0 && pace > 0)
                {
                    // Get the desired number of events to process.
                    var eventList = foundationEvents.GetRange(0, pace);

                    foreach(var foundationEvent in eventList)
                    {
                        if(eventTable.ContainsKey(foundationEvent.GetType()))
                        {
                            // Execute event delegate
                            var eventHandlerDelegate = eventTable[foundationEvent.GetType()];
                            eventHandlerDelegate(this, foundationEvent);
                        }
                    }

                    // Remove the processed events from the event queue.
                    foundationEvents.RemoveRange(0, pace);
                }
            }
        }

        /// <summary>
        /// Open a simulated Foundation initiated transaction.
        /// </summary>
        /// <returns>
        /// True if an open Foundation initiated transaction is available after the operation.
        /// False otherwise.
        /// </returns>
        private void CreateFoundationTransaction()
        {
            if(!foundationTransactionOpen)
            {
                foundationTransactionOpen = true;
                transactionId++;
            }
        }

        /// <summary>
        /// Close a simulated Foundation initiated transaction.
        /// </summary>
        private void CloseFoundationTransaction()
        {
            if(foundationTransactionOpen)
            {
                foundationTransactionOpen = false;
            }
        }

        /// <summary>
        /// Initializes <see cref="ConfigurationRead"/> and <see cref="LocalizationInformation"/>
        /// for the current report context, executes <see cref="GenerateInspectionReportEvent"/>,
        /// and writes the reports to files for each culture.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The <see cref="GenerateInspectionReportEventArgs"/> event data.</param>
        private void ExecuteGenerateInspectionReportEvent(object sender, EventArgs eventArgs)
        {
            if(eventArgs is GenerateInspectionReportEventArgs generateReportEventArgs)
            {
                localization.SetCulture(generateReportEventArgs.Culture);

                // Execute event handler
                GenerateInspectionReportEvent?.Invoke(sender, generateReportEventArgs);

                WriteOutGameInspectionReport(generateReportEventArgs);
            }
        }

        /// <summary>
        /// Executes <see cref="GenerateHtmlInspectionReportEvent"/>, and writes the reports to files for each culture.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The <see cref="GenerateHtmlInspectionReportEventArgs"/> event data.</param>
        private void ExecuteGenerateHtmlInspectionReportEvent(object sender, EventArgs eventArgs)
        {
            if(eventArgs is GenerateHtmlInspectionReportEventArgs generateHtmlReportEventArgs)
            {
                // Execute event handler
                GenerateHtmlInspectionReportEvent?.Invoke(sender, generateHtmlReportEventArgs);

                WriteOutHtmlGameInspectionReport(generateHtmlReportEventArgs);
            }
        }

        /// <summary>
        /// Executes <see cref="GetMinPlayableCreditBalanceEvent"/>, and writes the result to a file.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The <see cref="GetMinPlayableCreditBalanceEventArgs"/> event data.</param>
        private void ExecuteGetMinPlayableCreditBalanceEvent(object sender, EventArgs eventArgs)
        {
            if(eventArgs is GetMinPlayableCreditBalanceEventArgs getMinPlayableCreditBalanceEventArgs)
            {
                // Execute event handler
                GetMinPlayableCreditBalanceEvent?.Invoke(sender, getMinPlayableCreditBalanceEventArgs);

                WriteOutMinPlayableCreditBalanceReport(getMinPlayableCreditBalanceEventArgs);
            }
        }

        /// <summary>
        /// Executes the setup validation event by raising <see cref="ValidateThemeSetupEvent"/>.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The <see cref="ValidateThemeSetupEventArgs"/> event data.</param>
        private void ExecuteSetupValidation(object sender, EventArgs eventArgs)
        {
            if(eventArgs is ValidateThemeSetupEventArgs validateThemeSetupEventArgs)
            {
                ValidateThemeSetupEvent?.Invoke(sender, validateThemeSetupEventArgs);

                WriteOutSetupValidationReport(validateThemeSetupEventArgs);
            }
        }

        /// <summary>
        /// Executed when the foundation requests to generate game performance report.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The <see cref="GenerateGamePerformanceReportEventArgs"/> event data.</param>
        private void ExecuteGenerateGamePerformanceReportEvent(object sender, EventArgs eventArgs)
        {
            if(eventArgs is GenerateGamePerformanceReportEventArgs generateReportEventArgs)
            {
                // Execute event handler
                GenerateGamePerformanceReportEvent?.Invoke(sender, generateReportEventArgs);

                WriteOutGamePerformanceReport(generateReportEventArgs);
            }
        }

        /// <summary>
        /// Executed when the foundation requests to get game level award report.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The payload of the event.</param>
        private void ExecuteGetGameLevelValuesEvent(object sender, EventArgs eventArgs)
        {
            if(eventArgs is GetGameLevelValuesEventArgs getGameLevelValuesEventArgs)
            {
                GetGameLevelValuesEvent?.Invoke(sender, getGameLevelValuesEventArgs);
                WriteOutGameLevelAwardReport(getGameLevelValuesEventArgs);
            }
        }

        /// <summary>
        /// Executes the shut down event by raising <see cref="ShutDownEvent"/>.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Empty event arguments.</param>
        private void ExecuteShutDownEvent(object sender, EventArgs eventArgs)
        {
            ShutDownEvent?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the specified event.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event payload.</typeparam>
        /// <param name="sender">The sender of the event to raise.</param>
        /// <param name="eventArgs">The event payload.</param>
        /// <param name="handler">The event handler.</param>
        private static void ExecuteEventHandler<TEventArgs>(
            object sender,
            EventArgs eventArgs,
            EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            if(eventArgs is TEventArgs targetEventArgs)
            {
                handler?.Invoke(sender, targetEventArgs);
            }
        }

        /// <summary>
        /// Write out the content of <paramref name="generateReportEventArgs"/> to the Inspection Report file.
        /// </summary>
        /// <param name="generateReportEventArgs">The arguments to write out to the file.</param>
        private void WriteOutGameInspectionReport(GenerateInspectionReportEventArgs generateReportEventArgs)
        {
            if(generateReportEventArgs.GeneratedReport == null)
            {
                // Produce a verbose message because this is in standalone
                var message = new StringBuilder()
                    .AppendLine("There was an error trying to generate the Inspection report for")
                    .AppendFormat("Theme Identifier:    {0}", generateReportEventArgs.ThemeIdentifier).AppendLine()
                    .AppendFormat("Paytable Identifier: {0}", generateReportEventArgs.PaytableIdentifier).AppendLine()
                    .AppendFormat("Denomination:        {0}", generateReportEventArgs.Denomination).AppendLine()
                    .AppendFormat("Culture:             {0}", generateReportEventArgs.Culture).AppendLine()
                    .AppendLine("Error Message:")
                    .AppendLine(generateReportEventArgs.ErrorMessage);

                throw new ApplicationException(message.ToString());
            }

            var reportFileName = Path.Combine(MountPoint,
                                              string.Format(GameReportFileNameFormat, generateReportEventArgs.Culture));

            // Append to the existing file or create a new file
            using(var reportStream = new StreamWriter(reportFileName, true))
            {
                // Header
                reportStream.WriteLine(reportSeparator);
                reportStream.WriteLine(ReportItemFormatString, "Theme Identifier", generateReportEventArgs.ThemeIdentifier);
                reportStream.WriteLine(ReportItemFormatString, "Paytable Identifier", generateReportEventArgs.PaytableIdentifier);
                reportStream.WriteLine(ReportItemFormatString, "Denomination", generateReportEventArgs.Denomination);
                reportStream.WriteLine(ReportItemFormatString, "Culture", generateReportEventArgs.Culture);
                reportStream.WriteLine(reportSeparator);

                // Standard section
                foreach(var item in generateReportEventArgs.GeneratedReport.StandardReportPart.DefinedReportItems)
                {
                    reportStream.WriteLine(ReportItemFormatString, item.Label, item.Value);
                }

                foreach(var item in generateReportEventArgs.GeneratedReport.StandardReportPart.CustomReportItems)
                {
                    reportStream.WriteLine(ReportItemFormatString, item.Label, item.Value);
                }

                reportStream.WriteLine();

                // For each progressive game level
                foreach(var progressiveLevel in generateReportEventArgs.GeneratedReport.ProgressiveLevelParts)
                {
                    reportStream.WriteLine(ReportItemFormatString, "Progressive Game Level", progressiveLevel.ProgressiveLevel);
                    reportStream.WriteLine(reportSeparator);

                    foreach(var item in progressiveLevel.DefinedReportItems)
                    {
                        reportStream.WriteLine(ReportItemFormatString, item.Label, item.Value);
                    }
                    foreach(var item in progressiveLevel.CustomReportItems)
                    {
                        reportStream.WriteLine(ReportItemFormatString, item.Label, item.Value);
                    }
                }

                reportStream.WriteLine();
                reportStream.WriteLine();
                reportStream.WriteLine();
            }
        }

        /// <summary>
        /// Write out the content of <paramref name="generateHtmlReportEventArgs"/> to a local Html file.
        /// </summary>
        /// <param name="generateHtmlReportEventArgs">The arguments containing the Html to write out to the file.</param>
        private void WriteOutHtmlGameInspectionReport(GenerateHtmlInspectionReportEventArgs generateHtmlReportEventArgs)
        {
            if(string.IsNullOrEmpty(generateHtmlReportEventArgs?.GeneratedHtmlReport?.HtmlReport) ||
               !string.IsNullOrEmpty(generateHtmlReportEventArgs.ErrorMessage))
            {
                // Produce a verbose message because this is in standalone.
                var message = new StringBuilder()
                    .AppendFormat("There was an error trying to generate an Html Inspection report for all themes and paytables for culture {0}.",
                        generateHtmlReportEventArgs == null ? @"Unknown Culture" : generateHtmlReportEventArgs.Culture)
                    .AppendFormat("Error Message: {0}", generateHtmlReportEventArgs == null ? @"Unknown Error" : generateHtmlReportEventArgs.ErrorMessage);

                throw new ApplicationException(message.ToString());
            }

            var reportFileName = Path.Combine(MountPoint,
                string.Format(GameReportHtmlFileNameFormat, generateHtmlReportEventArgs.Culture));

            // Append to the existing file or create a new file
            File.AppendAllText(reportFileName, generateHtmlReportEventArgs.GeneratedHtmlReport.HtmlReport);
        }

        /// <summary>
        /// Write out the content of <paramref name="eventArgs"/> to the Min Playable Credit Balance file.
        /// </summary>
        /// <param name="eventArgs">The event arguments containing the information to write out.</param>
        private void WriteOutMinPlayableCreditBalanceReport(GetMinPlayableCreditBalanceEventArgs eventArgs)
        {
            // Check if any of the MinPlayableCreditBalance value is 0.
            var hasError = eventArgs.Requests.Any(request => request.MinPlayableCreditBalance == 0);

            // Append to the existing file or create a new file
            using(var reportStream = new StreamWriter(MinPlayableCreditBalanceReportFileName, true))
            {
                // Write a header with timestamp and warnings (if any).
                // The timestamp is used to distinguish the reports generated for GameInspection
                // and HtmlGameInspection services in case both services are enabled.
                reportStream.WriteLine(reportSeparator);
                reportStream.WriteLine($"Generated on {DateTime.Now}");

                // Write an error message if needed.
                if(hasError)
                {
                    reportStream.WriteLine("!!! ERROR !!!");
                    reportStream.WriteLine("One or more Min Playable Credit Balance values are 0.");
                    reportStream.WriteLine("Please double check the report implementation.");
                }

                reportStream.WriteLine(reportSeparator);
                reportStream.WriteLine();
                reportStream.WriteLine();

                foreach(var request in eventArgs.Requests)
                {
                    reportStream.WriteLine(reportSeparator);
                    reportStream.WriteLine(ReportItemFormatString, "Theme Identifier", request.ThemeIdentifier);
                    reportStream.WriteLine(ReportItemFormatString, "Paytable Identifier", request.PaytableIdentifier);
                    reportStream.WriteLine(ReportItemFormatString, "Denomination", request.Denomination);
                    reportStream.WriteLine(reportSeparator);
                    reportStream.WriteLine(ReportItemFormatString, "Min Playable Credit Balance", request.MinPlayableCreditBalance);
                    reportStream.WriteLine();
                }

                reportStream.WriteLine();
                reportStream.WriteLine();
            }

            // Throw an exception so an error message could be displayed.
            // Do this AFTER the report has been written so that the user can check the report file
            // to see which ones are 0.
            if(hasError)
            {
                var message = "One or more Min Playable Credit Balance values are 0.  "
                              + $"Please see {MinPlayableCreditBalanceReportFileName} for details.";

                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Write out the content of <paramref name="getGameLevelValuesEventArgs"/> to the Game Level Award Report file.
        /// </summary>
        /// <param name="getGameLevelValuesEventArgs">The arguments to write out to the file.</param>
        private void WriteOutGameLevelAwardReport(GetGameLevelValuesEventArgs getGameLevelValuesEventArgs)
        {
            if(getGameLevelValuesEventArgs.GameLevelValues == null)
            {
                // Produce a verbose message because this is in standalone
                var message = new StringBuilder()
                    .AppendLine("There was an error trying to generate the Game Level Award report for")
                    .AppendFormat("Theme Identifier: {0}", getGameLevelValuesEventArgs.ThemeIdentifier)
                    .AppendLine()
                    .AppendLine("Error Message:")
                    .AppendLine(getGameLevelValuesEventArgs.ErrorMessage);

                throw new ApplicationException(message.ToString());
            }

            var reportFileName = Path.Combine(MountPoint,
                string.Format(GameLevelAwardReportFileNameFormat, getGameLevelValuesEventArgs.ThemeIdentifier));

            // Create a new file for writing Game Level Award report as the file for current theme identifier is
            // delete when Standalone.ReportLib is instantiated.
            using(var reportStream = new StreamWriter(reportFileName))
            {
                // Header
                reportStream.WriteLine(reportSeparator);
                reportStream.WriteLine(ReportItemFormatString, "Theme Identifier",
                    getGameLevelValuesEventArgs.ThemeIdentifier);
                reportStream.WriteLine(reportSeparator);

                foreach(var gameLevelValue in getGameLevelValuesEventArgs.GameLevelValues)
                {
                    reportStream.WriteLine();
                    reportStream.WriteLine(gameLevelValue.Key);
                    foreach(var gameLevelLinkedData in gameLevelValue.Value)
                    {
                        reportStream.WriteLine(reportSeparator);
                        reportStream.WriteLine(ReportItemFormatString,
                                               "Game Level Linked Data",
                                               gameLevelLinkedData);
                    }
                }

                reportStream.WriteLine();
            }
        }

        /// <summary>
        /// Write out the content of <paramref name="generateReportEventArgs"/> to the Game Performance Report file.
        /// </summary>
        /// <param name="generateReportEventArgs">The arguments to write out to the file.</param>
        private void WriteOutGamePerformanceReport(GenerateGamePerformanceReportEventArgs generateReportEventArgs)
        {
            if(generateReportEventArgs.GeneratedReport == null)
            {
                // Produce a verbose message because this is in standalone
                var message = new StringBuilder()
                    .AppendLine("There was an error trying to generate the Game Performance report for")
                    .AppendFormat("Theme Identifier:  {0}", generateReportEventArgs.ThemeIdentifier).AppendLine()
                    .AppendFormat("Paytable Identifier: {0}", generateReportEventArgs.PaytableIdentifier).AppendLine()
                    .AppendLine("Error Message:")
                    .AppendLine(generateReportEventArgs.ErrorMessage);

                throw new ApplicationException(message.ToString());
            }

            var reportFileName = Path.Combine(MountPoint, GamePerformanceReportFileName);

            // Append to the existing file or create a new file
            using(var reportStream = new StreamWriter(reportFileName, true))
            {
                // Header
                reportStream.WriteLine(reportSeparator);
                reportStream.WriteLine(ReportItemFormatString, "Theme Identifier", generateReportEventArgs.ThemeIdentifier);
                reportStream.WriteLine(ReportItemFormatString, "Paytable Identifier", generateReportEventArgs.PaytableIdentifier);
                reportStream.WriteLine(reportSeparator);

                reportStream.WriteLine(generateReportEventArgs.GeneratedReport);

                reportStream.WriteLine();
            }
        }

        /// <summary>
        /// Write out the setup validation Report file.
        /// </summary>
        /// <param name="validateThemeSetupEventArgs">The arguments to write out to the file.</param>
        private void WriteOutSetupValidationReport(ValidateThemeSetupEventArgs validateThemeSetupEventArgs)
        {
            if(validateThemeSetupEventArgs.ErrorMessage != null)
            {
                // Produce a verbose message because this is in standalone
                var message = new StringBuilder()
                    .AppendLine("There was an error trying to generate the Theme Setup Validation Report for")
                    .AppendFormat("Theme Identifier: {0}", validateThemeSetupEventArgs.ThemeIdentifier).AppendLine()
                    .AppendLine("Error Message:")
                    .AppendLine(validateThemeSetupEventArgs.ErrorMessage);

                throw new ApplicationException(message.ToString());
            }

            var reportFileName = Path.Combine(MountPoint, SetupValidationReportFileName);

            // Create a new or append to the existing file.
            using(var reportStream = new StreamWriter(reportFileName, true))
            {
                // Header
                reportStream.WriteLine(reportSeparator);
                reportStream.WriteLine(ReportItemFormatString, "Theme Identifier", validateThemeSetupEventArgs.ThemeIdentifier);
                reportStream.WriteLine(reportSeparator);

                reportStream.WriteLine();

                // For each setup validation result
                foreach(var setupValidationResult in validateThemeSetupEventArgs.ValidationResults)
                {
                    reportStream.WriteLine(ReportItemFormatString, "Setup Validation Type", setupValidationResult.FaultType);
                    reportStream.WriteLine(ReportItemFormatString, "Setup Validation Area", setupValidationResult.FaultArea);
                    reportStream.WriteLine(ReportItemFormatString, "Setup Validation Messages :", string.Empty);
                    foreach(var faultLocalizationItem in setupValidationResult.FaultLocalizationItems)
                    {
                        reportStream.WriteLine(ReportItemFormatString, "Fault Messages Culture", faultLocalizationItem.Culture);
                        reportStream.WriteLine(ReportItemFormatString, "Fault Messages Title", faultLocalizationItem.Title);
                        reportStream.WriteLine(ReportItemFormatString, "Fault Messages Message", faultLocalizationItem.Message);
                    }
                    reportStream.WriteLine(reportSeparator);
                }

                reportStream.WriteLine();
            }
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
            if(!disposed && disposing)
            {
                (eventResetEvent as IDisposable).Dispose();
                disposed = true;
            }
        }

        #endregion
    }
}
