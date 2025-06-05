//-----------------------------------------------------------------------
// <copyright file = "FoundationLink.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using F2L;
    using F2L.Schemas.Internal;
    using F2X;
    using F2XCallbacks;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;
    using Threading;
    using Transport;
    using ConnectCategory = F2L.ConnectCategory;
    using ILinkControlCategoryCallbacks = F2L.ILinkControlCategoryCallbacks;
    using LinkControlCategory = F2L.LinkControlCategory;

    /// <summary>
    /// Class which provides a link to the foundation.
    /// </summary>
    internal sealed class FoundationLink : ILinkControlCategoryCallbacks, IConnectCallbacks,
                                           ITransactionEventLink,
                                           ITransactionCallbacks, IEventCallbacks,
                                           IDisposable
    {
        #region Mappings Between F2L.Schema and Communication.Foundation

        /// <summary>
        /// Mapping from <see cref="EnvironmentAttributeType"/> to <see cref="EnvironmentAttribute"/>.
        /// </summary>
        private static readonly Dictionary<EnvironmentAttributeType, EnvironmentAttribute> EnvironmentAttributeMapping =
            new Dictionary<EnvironmentAttributeType, EnvironmentAttribute>
            {
                {EnvironmentAttributeType.bankedcredit, EnvironmentAttribute.BankedCredits},
                {EnvironmentAttributeType.CDS, EnvironmentAttribute.Cds},
                {EnvironmentAttributeType.showdemo, EnvironmentAttribute.ShowDemo}
            };

        #endregion Mappings Between F2L.Schema and Communication.Foundation

        #region Universal Controller

        // Categories supported: GameControlCategory 4.1, LinkControlCategory 2.0, ConnectCategory 2.0, ShowCategory 2.0,
        // AutoPlayCategory 1.0, (Extended)GameTiltCategory 1.0,  (Extended)IdentificationCategory 1.0, GameStopReportCategory 2.0,
        // (Extended)WapSignControlCategory.
        internal static readonly CategoryVersionInformation GameControl4P1 =
            new CategoryVersionInformation((int)MessageCategory.GameControl, 4, 1);

        #endregion Universal Controller

        #region Supported Message Categories

        /// <summary>
        /// Link Control Category, version 2.0.
        /// </summary>
        private static readonly CategoryVersionInformation LinkControlCategory2P0 =
            new CategoryVersionInformation((int)MessageCategory.F2LLinkControl, 2, 0);

        /// <summary>
        /// Link Control Category, version 2.3.
        /// </summary>
        private static readonly CategoryVersionInformation LinkControlCategory2P3 =
            new CategoryVersionInformation((int)MessageCategory.F2LLinkControl, 2, 3);

        /// <summary>
        /// Link Control Category, version 2.4.
        /// </summary>
        private static readonly CategoryVersionInformation LinkControlCategory2P4 =
            new CategoryVersionInformation((int)MessageCategory.F2LLinkControl, 2, 4);

        /// <summary>
        /// Auto Play Category, version 1.0.
        /// </summary>
        internal static readonly CategoryVersionInformation AutoPlay1P0 =
            new CategoryVersionInformation((int)MessageCategory.AutoPlay, 1, 0);

        /// <summary>
        /// Auto Play Category, version 1.1.
        /// </summary>
        internal static readonly CategoryVersionInformation AutoPlay1P1 =
            new CategoryVersionInformation((int)MessageCategory.AutoPlay, 1, 1);

        /// <summary>
        /// Game Control Category, version 4.13.
        /// </summary>
        internal static readonly CategoryVersionInformation GameControl4P13 =
            new CategoryVersionInformation((int)MessageCategory.GameControl, 4, 13);

        /// <summary>
        /// Game Control Category, version 4.14.
        /// </summary>
        internal static readonly CategoryVersionInformation GameControl4P14 =
            new CategoryVersionInformation((int)MessageCategory.GameControl, 4, 14);

        /// <summary>
        /// Game Control Category, version 4.17.
        /// </summary>
        private static readonly CategoryVersionInformation GameControl4P17 =
            new CategoryVersionInformation((int)MessageCategory.GameControl, 4, 17);

        /// <summary>
        /// Show Category, version 2.0.
        /// </summary>
        internal static readonly CategoryVersionInformation Show2P0 =
            new CategoryVersionInformation((int)MessageCategory.Show, 2, 0);

        /// <summary>
        /// Show Category, version 2.1.
        /// </summary>
        internal static readonly CategoryVersionInformation Show2P1 =
            new CategoryVersionInformation((int)MessageCategory.Show, 2, 1);

        /// <summary>
        /// Localization Category, version 1.1.
        /// </summary>
        internal static readonly CategoryVersionInformation Localization1P1 =
            new CategoryVersionInformation((int)MessageCategory.Localization, 1, 1);

        /// <summary>
        /// Localization Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation Localization1P2 =
            new CategoryVersionInformation((int)MessageCategory.Localization, 1, 2);

        /// <summary>
        /// Game Stop Report Category, version 2.0.
        /// </summary>
        internal static readonly CategoryVersionInformation GameStopReport2P0 =
            new CategoryVersionInformation((int)MessageCategory.GameStopReport, 2, 0);

        /// <summary>
        /// Voucher Print Category, version 1.0
        /// </summary>
        internal static readonly CategoryVersionInformation VoucherPrint1P0 =
            new CategoryVersionInformation((int)MessageCategory.VoucherPrint, 1, 0);

        /// <summary>
        /// EGM Config Data Category, version 1.3.
        /// </summary>
        internal static readonly CategoryVersionInformation EgmConfigData1P3 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 3);

        /// <summary>
        /// EGM Config Data Category, version 1.4.
        /// </summary>
        internal static readonly CategoryVersionInformation EgmConfigData1P4 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 4);

        /// <summary>
        /// EGM Config Data Category, version 1.5.
        /// </summary>
        private static readonly CategoryVersionInformation EgmConfigData1P5 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 5);

        /// <summary>
        /// EGM Config Data Category, version 1.6.
        /// </summary>
        private static readonly CategoryVersionInformation EgmConfigData1P6 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 6);

        /// <summary>
        /// Custom Configuration Read Category, version 1.1.
        /// </summary>
        internal static readonly CategoryVersionInformation CustomConfigurationRead1P1 =
            new CategoryVersionInformation((int)MessageCategory.CustomConfigurationRead, 1, 1);

        #endregion Supported Message Categories

        #region Fields

        /// <summary>
        /// Transport used by this link.
        /// </summary>
        private readonly IF2XTransport transport;

        /// <summary>
        /// The Foundation version to target.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        /// <summary>
        /// Dependencies required for creating categories.
        /// </summary>
        private readonly CategoryNegotiationDependencies categoryDependencies;

        /// <summary>
        /// Event used to block until connecting is finished.
        /// </summary>
        private readonly AutoResetEvent connectComplete = new AutoResetEvent(false);

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// A list of the categories requested by the game in the theme level negotiation.
        /// </summary>
        private readonly List<CategoryRequest> requestedCategories;

        /// <summary>
        /// List of all category handlers that have been created,
        /// each for a specific category and version.
        /// </summary>
        private readonly Dictionary<CategoryVersionInformation, IApiCategory> allCategoryHandlers;

        /// <summary>
        /// Manages the requested interface extensions.
        /// </summary>
        private readonly InterfaceExtensionManager extensionManager;

        /// <summary>
        /// Handles callbacks from the game lib.
        /// </summary>
        private readonly IGameLibCallback gameLibCallback;

        /// <summary>
        /// Underlying socket transport used by the F2L.
        /// </summary>
        private readonly SocketTransport socketTransport;

        /// <summary>
        /// The information on the theme resource received during
        /// the theme level negotiation, and to be passed on to the
        /// game at the end of the negotiation.
        /// </summary>
        private ThemeResource themeResource;

        /// <summary>
        /// Flag to indicate if a connection has been established on the foundation link.
        /// </summary>
        private volatile bool isConnected;

        #endregion Fields

        #region Public Properties

        /// <summary>
        /// Auto play category for the foundation link.
        /// </summary>
        public IAutoPlayCategory AutoPlay { private set; get; }

        /// <summary>
        /// Game control category for the foundation link.
        /// </summary>
        /// <DevDoc>
        /// Made this internal set for testing only.
        /// </DevDoc>
        // ReSharper disable once MemberCanBePrivate.Global
        public IGameControlCategory GameControl { internal set; get; }

        /// <summary>
        /// Show demo category for the foundation link.
        /// </summary>
        public IShowCategory ShowControl { private set; get; }

        /// <summary>
        /// Localization category for the foundation link.
        /// </summary>
        public ILocalizationCategory LocalizationControl { private set; get; }

        /// <summary>
        /// Game stop report category. If the category is unsupported, or not requested, then the
        /// property will be null.
        /// </summary>
        public IGameStopReportCategory GameStopReport { private set; get; }

        /// <summary>
        /// Voucher print category for the foundation link.
        /// </summary>
        public IApiCategory VoucherPrint { private set; get; }

        /// <summary>
        /// EGM Config Data category for the foundation link.
        /// </summary>
        public IEgmConfigDataCategory EgmConfigData { private set; get; }

        /// <summary>
        /// Custom Configuration Read Category for the foundation link.
        /// </summary>
        public ICustomConfigurationReadCategory CustomConfigurationRead { private set; get; }

        /// <summary>
        ///  Flag indicating if the game is in show mode.
        /// </summary>
        public bool ShowMode => ShowControl != null;

        /// <summary>
        /// Token assigned by the Foundation and used to identify this game instance.
        /// </summary>
        public string Token { private set; get; }

        /// <summary>
        /// The mount point of the game executable.
        /// </summary>
        public string GameMountPoint { private set; get; }

        /// <summary>
        /// A monitor to monitor the exceptions on the transport callback threads.
        /// </summary>
        public IExceptionMonitor TransportExceptionMonitor => socketTransport;

        /// <summary>
        /// Gets the information on imported extensions linked to the application.
        /// </summary>
        public IExtensionImportCollection ExtensionImportCollection { get; private set; }

        #endregion Public Properties

        #region Constructor

        /// <summary>
        /// Construct an instance of the foundation link.
        /// </summary>
        /// <param name="owner">GameLib which owns the link.</param>
        /// <param name="baseExtensionDependencies">The common dependencies for extensions.</param>
        /// <param name="address">Foundation address to connect to.</param>
        /// <param name="port">Foundation port to connect to.</param>
        /// <param name="foundationTarget">Foundation version to target.</param>
        /// <param name="nonTransactionalEventCallbacks">The callback interface for handling non-transactional events.</param>
        public FoundationLink(IGameLibCallback owner, IInterfaceExtensionDependencies baseExtensionDependencies,
                              string address, ushort port, FoundationTarget foundationTarget,
                              INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            socketTransport = new SocketTransport(address, port);
            transport = new F2XTransport(socketTransport);

            this.foundationTarget = foundationTarget;

            categoryDependencies = new CategoryNegotiationDependencies
                                       {
                                           Transport = transport,
                                           EventCallbacks = this,
                                           NonTransactionalEventCallbacks = nonTransactionalEventCallbacks,
                                           TransactionCallbacks = this,
                                       };

            allCategoryHandlers = new Dictionary<CategoryVersionInformation, IApiCategory>();

            // Populate the table of available link control categories.
            // The CategoryNegotiationDependencies.LinkControlCategoryCallbacks is for F2X LinkControl category,
            // while this link uses F2L LinkControl category.  Therefore, it still has to use "this" here.
            var availableLinkControlCategoryHandlers = new List<CategoryRequest>
                                                           {
                                                               new CategoryRequest(LinkControlCategory2P0, true,
                                                                                   FoundationTarget.UniversalController.UpTo(FoundationTarget.UniversalController2),
                                                                                   dependencies => new LinkControlCategory(dependencies.Transport, this)),

                                                               new CategoryRequest(LinkControlCategory2P3, true, FoundationTarget.AscentHSeriesCds.UpTo(FoundationTarget.AscentJSeriesMps),
                                                                                   dependencies => new LinkControlCategory(dependencies.Transport, this)),

                                                               new CategoryRequest(LinkControlCategory2P4, true, FoundationTarget.AscentKSeriesCds.AndHigher(),
                                                                                   dependencies => new LinkControlCategory(dependencies.Transport, this)),
                                                           };

            // Select the link control category for the defined FoundationTarget.
            var selected = availableLinkControlCategoryHandlers.First(request => request.FoundationTarget.HasFlag(foundationTarget));

            // Instantiate the link control category.
            var linkControlCategory = (LinkControlCategory)selected.CreateCategory(categoryDependencies);

            // Set the effective version for the link control category.
            linkControlCategory.SetVersion(selected.CategoryVersionInformation.MajorVersion,
                                           selected.CategoryVersionInformation.MinorVersion);

            // Instantiate the connect category.
            var connectCategory = new ConnectCategory(transport, this, linkControlCategory);

            // Install link control and connect categories.
            transport.InstallCategoryHandler(linkControlCategory);
            transport.InstallCategoryHandler(connectCategory);

            // Add them to allCategoryHandlers table so that they can be disposed later.
            allCategoryHandlers.Add(selected.CategoryVersionInformation, linkControlCategory);

            allCategoryHandlers.Add(new CategoryVersionInformation((int)connectCategory.Category,
                                                                   connectCategory.MajorVersion,
                                                                   connectCategory.MinorVersion),
                                    connectCategory);

            extensionManager = new InterfaceExtensionManager(CategoryNegotiationLevel.Link,
                                                             baseExtensionDependencies);

            gameLibCallback = owner;

            //Populate the table of available categories.
            var availableCategories =
                new List<CategoryRequest>
                    {
                        new CategoryRequest(AutoPlay1P0, true,
                                            FoundationTarget.UniversalController.UpTo(FoundationTarget.AscentHSeriesCds),
                                            dependencies => new AutoPlayCategory(
                                                dependencies.Transport,
                                                new AutoPlayCallbackHandler(dependencies.EventCallbacks))),

                        new CategoryRequest(AutoPlay1P1, true, FoundationTarget.AscentISeriesCds.AndHigher(),
                                            dependencies => new AutoPlayCategory(
                                                dependencies.Transport,
                                                new AutoPlayCallbackHandler(dependencies.EventCallbacks))),

                        new CategoryRequest(Show2P0, false, FoundationTarget.UniversalController.UpTo(FoundationTarget.UniversalController2),
                                            dependencies => new ShowCategory(dependencies.Transport)),

                        new CategoryRequest(Show2P1, false, FoundationTarget.AllAscent,
                                            dependencies => new ShowCategory(dependencies.Transport)),

                        new CategoryRequest(GameStopReport2P0, false, FoundationTarget.All,
                                            dependencies => new GameStopReportCategory(dependencies.Transport)),

                        new CategoryRequest(GameControl4P13, true, FoundationTarget.AscentHSeriesCds,
                                            dependencies => new GameControlCategory(
                                                dependencies.Transport,
                                                new GameControlCallbackHandler(((ICategoryNegotiationDependencies)dependencies).TransactionCallbacks,
                                                                               dependencies.EventCallbacks))),

                        new CategoryRequest(GameControl4P14, true, FoundationTarget.AscentISeriesCds.UpTo(FoundationTarget.AscentP1Dynasty),
                                            dependencies => new GameControlCategory(
                                                dependencies.Transport,
                                                new GameControlCallbackHandler(((ICategoryNegotiationDependencies)dependencies).TransactionCallbacks,
                                                                               dependencies.EventCallbacks))),

                        new CategoryRequest(GameControl4P17, true, FoundationTarget.AscentQ1Series.AndHigher(),
                            dependencies => new GameControlCategory(
                                dependencies.Transport,
                                new GameControlCallbackHandler(((ICategoryNegotiationDependencies)dependencies).TransactionCallbacks,
                                    dependencies.EventCallbacks))),

                        new CategoryRequest(GameControl4P1, true, FoundationTarget.UniversalController.UpTo(FoundationTarget.UniversalController2),
                                            dependencies => new UcGameControlCategory(
                                                dependencies.Transport,
                                                new GameControlCallbackHandler(((ICategoryNegotiationDependencies)dependencies).TransactionCallbacks,
                                                                               dependencies.EventCallbacks))),

                        new CategoryRequest(VoucherPrint1P0, true, FoundationTarget.AllAscent,
                                            dependencies => new VoucherPrintCategory(
                                                dependencies.Transport,
                                                new VoucherPrintCallbackHandler(dependencies.EventCallbacks))),

                        new CategoryRequest(Localization1P1, false, FoundationTarget.AscentHSeriesCds.UpTo(FoundationTarget.AscentKSeriesCds),
                                            dependencies => new LocalizationCategory(dependencies.Transport)),

                        new CategoryRequest(Localization1P2, false, FoundationTarget.AscentMSeries.AndHigher(),
                            dependencies => new LocalizationCategory(dependencies.Transport)),

                        new CategoryRequest(EgmConfigData1P3, false, FoundationTarget.AscentHSeriesCds,
                                            dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                        new CategoryRequest(EgmConfigData1P4, true, FoundationTarget.AscentISeriesCds.UpTo(FoundationTarget.AscentQ2Series),
                                            dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                        new CategoryRequest(EgmConfigData1P5, true, FoundationTarget.AscentQ3Series.UpTo(FoundationTarget.AscentR1Series),
                                            dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                        new CategoryRequest(EgmConfigData1P6, true, FoundationTarget.AscentR2Series.AndHigher(),
                                            dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                        new CategoryRequest(CustomConfigurationRead1P1, FoundationTarget.AscentISeriesCds.AndHigher(),
                                            dependencies => new CustomConfigurationReadCategory(dependencies.Transport)),
                    };

            requestedCategories =
                availableCategories.Where(request => request.FoundationTarget.HasFlag(foundationTarget))
                                   .ToList();
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Get an extended interface. This allows for extension of platform features beyond <see cref="IGameLib"/>.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed, then <see langword="null"/> will
        /// be returned.
        /// </returns>
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            return extensionManager.GetInterface<TExtendedInterface>();
        }

        /// <summary>
        /// Establish a connect to the foundation.
        /// </summary>
        /// <param name="additionalInterfaces">List of additional interface configurations to request.</param>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        public bool Connect(IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaces)
        {
            RequestAdditionalInterfaces(additionalInterfaces);

            transport.Connect();

            connectComplete.WaitOne(socketTransport);

            return isConnected;
        }

        /// <summary>
        /// Disconnect from the foundation.
        /// </summary>
        public void Disconnect()
        {
            transport.Disconnect();

            // Clean up just to make sure.
            isConnected = false;
            connectComplete.Reset();
        }

        #endregion Public Methods

        #region ITransactionEventLink Members

        /// <inheritdoc />
        public bool ActionRequest(byte[] payload)
        {
            return GameControl.ActionRequest(payload);
        }

        /// <inheritdoc />
        public event EventHandler PostingEvent;

        /// <inheritdoc />
        public event EventHandler ActionResponseEvent;

        #endregion ITransactionEventLink Members

        #region ITransactionCallbacks Members

        /// <inheritdoc />
        public void ProcessActionResponse(byte[] payload)
        {
            ActionResponseEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion ITransactionCallbacks Members

        #region IEventCallbacks Members

        /// <inheritdoc />
        /// <devdoc>
        /// Do not implement IEventCallbacks explicitly so that
        /// this type's own methods can call PostEvent conveniently.
        /// </devdoc>
        public void PostEvent(EventArgs foundationEvent)
        {
            PostingEvent?.Invoke(this, foundationEvent);
        }

        #endregion IEventCallbacks Members

        #region ILinkControlCategoryCallbacks Members

        /// <inheritdoc />
        public string ProcessGetThemeApiVersions(string binTag, string binConnectToken, string binTagDataFile, string themeTag,
                                                 string themeTagDataFile,
                                                 LinkControlGetThemeApiVersionsSendResourcePaths resourcePaths,
                                                 LinkControlGetThemeApiVersionsSendEnvironmentAttributes environmentAttributes,
                                                 string jurisdiction, IEnumerable<ExtensionListExtension> extensions,
                                                 out LinkControlGetThemeApiVersionsReplyCategoryVersions callbackResult)
        {
            // Token and game mount point should be the same for all themes in the same game package.
            Token = binConnectToken;

            GameMountPoint = resourcePaths.ResourcePath.Where(path => path.InclusionType == GameDiscovPathTypeInclusionType.Bin)
                                                       .Select(path => path.Path)
                                                       .FirstOrDefault();

            themeResource = new ThemeResource
            {
                GameMountPoint = GameMountPoint,
                Tag = themeTag,
                TagDataFile = themeTagDataFile
            };

            // Check if all of the environment attributes received are supported.
            var attributes = environmentAttributes.EnvironmentAttribute;
            if(attributes.Any(attribute => !EnvironmentAttributeMapping.ContainsKey(attribute)))
            {
                var attributeStrings = attributes.Aggregate(string.Empty,
                    (current, attribute) => current + attribute.ToString());

                throw new InvalidOperationException($"One or more environment attributes are not supported.  Attributes received: {attributeStrings}");
            }

            // Save environment attributes.
            var attributeList = attributes.Select(attributeType => EnvironmentAttributeMapping[attributeType])
                .ToList();

            gameLibCallback.SetEnvironmentAttributes(attributeList);

            gameLibCallback.SetJurisdiction(jurisdiction);

            var categoriesRequested =
                requestedCategories.Select(categoryRequest => categoryRequest.CategoryVersionInformation);

            ExtensionImportCollection = new ExtensionImportCollection(
                extensions.Select(extensionImport => new ExtensionImport(
                    extensionImport.ExtensionIdentifier,
                    new Version((int)extensionImport.ExtensionVersion.MajorVersion,
                        (int)extensionImport.ExtensionVersion.MinorVersion,
                        (int)extensionImport.ExtensionVersion.PatchVersion),
                    extensionImport.ResourceDirectoryBase)));

            // Convert the generic type to schema type.
            var convertedTypes = from categoryVersion in categoriesRequested
                                 select new CategoryVersionType
                                            {
                                                Category = categoryVersion.Category,
                                                Version = new VersionType(categoryVersion.MajorVersion,
                                                                          categoryVersion.MinorVersion)
                                            };

            callbackResult = new LinkControlGetThemeApiVersionsReplyCategoryVersions
                                                {
                                                    CategoryVersion = convertedTypes.ToList()
                                                };

            return null;
        }

        /// <inheritdoc />
        string ILinkControlCategoryCallbacks.ProcessPark()
        {
            PostEvent(new ParkEventArgs());
            return null;
        }

        /// <inheritdoc />
        public string ProcessSetThemeApiVersions(LinkControlSetThemeApiVersionsSendCategoryVersions categoryVersions)
        {
            string errorMessage = null;

            List<CategoryVersionInformation> selectedCategories;

            if(categoryVersions?.CategoryVersion == null)
            {
                selectedCategories = new List<CategoryVersionInformation>();
            }
            else
            {
                // Convert the schema type to generic type.
                 selectedCategories = (from cvType in categoryVersions.CategoryVersion
                                       select new CategoryVersionInformation(cvType.Category,
                                                                             cvType.Version.MajorVersion,
                                                                             cvType.Version.MinorVersion)).ToList();
            }

            // Remove all installed category handlers first.
            transport.UninstallControlLevelCategoryHandlers();

            // Reset related properties.
            AutoPlay = null;
            GameControl = null;
            ShowControl = null;
            GameStopReport = null;
            VoucherPrint = null;
            LocalizationControl = null;
            EgmConfigData = null;
            CustomConfigurationRead = null;

            extensionManager.UninstallInterfaceExtensions();

            // Check if any must-have category is missing from the selected list.
            var anyMissing = requestedCategories.Any(requestedCategory => requestedCategory.Required && !selectedCategories.Contains(requestedCategory.CategoryVersionInformation));
            if(anyMissing)
            {
                errorMessage = "Not all required theme category versions are found.";
            }
            else
            {
                foreach(var categoryVersion in selectedCategories)
                {
                    InstallApiCategory(categoryVersion);
                }

                isConnected = true;

                // Signal the connect complete event.
                // In case of theme level re-negotiations, signaling this event is redundant but OK.
                connectComplete.Set();

                // 1. The theme resource could also be updated by
                // SwitchThemeContext event (from GameControlCategory),
                // so we only cache themeResource momentarily in this class
                // to correctly fill in the NewThemeSelectionEvent payload.
                // 2. Post the event only after the connection has been complete.
                // Otherwise, the game won't be able to process it.
                if(themeResource != null)
                {
                    PostEvent(new NewThemeSelectionEventArgs(themeResource));
                    themeResource = null;
                }
            }

            return errorMessage;
        }

        #endregion ILinkControlCategoryCallbacks Members

        #region IDisposable Implementation

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// </summary>
        /// <param name="disposing">
        /// Flag indicating if the object is being disposed. If true Dispose was called, if false the finalizer called
        /// this function. If the finalizer called the function, then only unmanaged resources should be released.
        /// </param>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                // The auto reset event implements IDisposable, so there is no need to check if it converted.
                (connectComplete as IDisposable).Dispose();

                // The socket transport implements IDisposable.
                socketTransport.Dispose();

                // The F2L transport may or may not implement IDisposable.
                var disposableTransport = transport as IDisposable;
                disposableTransport?.Dispose();

                // Dispose all category handlers.
                foreach(var categoryEntry in allCategoryHandlers)
                {
                    var disposableHandler = categoryEntry.Value as IDisposable;
                    disposableHandler?.Dispose();
                }

                // Dispose all interface extensions.
                extensionManager.Dispose();

                disposed = true;
            }
        }

        #endregion IDisposable Implementation

        #region IConnectCallbacks Members

        /// <inheritdoc />
        public void ProcessShutDown()
        {
            // If the theme level negotiation in LinkControlCategory fails,
            // Foundation would send a Shut Down message, while the game is
            // still being blocked in the ConnectToFoundation method.
            // Unblock connectComplete so that the game can properly shut down.
            connectComplete.Set();

            gameLibCallback.ShutDownProcess();
        }

        #endregion IConnectCallbacks Members

        #region Private Methods

        /// <summary>
        /// Request additional interface configurations.
        /// </summary>
        /// <param name="additionalInterfaces">List of additional interface configurations to request.</param>
        private void RequestAdditionalInterfaces(IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaces)
        {
            extensionManager.RequestInterfaces(foundationTarget, additionalInterfaces);
            requestedCategories.AddRange(extensionManager.GetRequestedCategories());
        }

        /// <summary>
        /// Create a handler for the specified category and version,
        /// install it with the F2L transport, and assign the related
        /// public properties.
        /// </summary>
        /// <remarks>
        /// This method maintains a list of handlers created so far.
        /// If the handler already exists, it will use the existing
        /// handler instead of instantiating a new one.
        /// </remarks>
        /// <param name="categoryVersion">
        /// The category and version to install.
        /// </param>
        private void InstallApiCategory(CategoryVersionInformation categoryVersion)
        {
            IApiCategory handler;

            var categoryCreator = requestedCategories.Where(
                categoryRequest => Equals(categoryRequest.CategoryVersionInformation, categoryVersion))
                .Select(categoryRequest => categoryRequest.CreateCategory)
                .FirstOrDefault();

            // Check if the category and version is supported.
            if(categoryCreator == null)
            {
                throw new ArgumentException($"Category and version {categoryVersion} is not supported by the game");
            }

            lock(allCategoryHandlers)
            {
                // Check if a handler already exists for this category and version.
                if(allCategoryHandlers.ContainsKey(categoryVersion))
                {
                    handler = allCategoryHandlers[categoryVersion];
                }
                else
                {
                    // If not, create the handler.
                    handler = categoryCreator(categoryDependencies);

                    // If the category handler supports multiple versions,
                    // update its version with the one accepted by Foundation.
                    // This is the version to use in future communications.
                    if(handler is IMultiVersionSupport multiVersionHandler)
                    {
                        multiVersionHandler.SetVersion(categoryVersion.MajorVersion, categoryVersion.MinorVersion);
                    }
                    // If the category handler supports only one version,
                    // double check if the version accepted by Foundation
                    // matches the one implemented by the handler.
                    else if(handler.MajorVersion != categoryVersion.MajorVersion ||
                            handler.MinorVersion != categoryVersion.MinorVersion)
                    {
                        throw new ApplicationException($"Requested category version {categoryVersion} does not match the category implementation.");
                    }

                    // Add it to the list of category handlers created.
                    allCategoryHandlers.Add(categoryVersion, handler);
                }
            }

            // Install the message category.
            transport.InstallCategoryHandler(handler);
            extensionManager.HandleInstalledCategory(handler);

            // Set related properties.
            switch(handler.Category)
            {
                case MessageCategory.AutoPlay:
                    AutoPlay = handler as IAutoPlayCategory;
                    break;

                case MessageCategory.GameControl:
                    GameControl = handler as IGameControlCategory;
                    break;

                case MessageCategory.Show:
                    ShowControl = handler as IShowCategory;
                    break;

                case MessageCategory.GameStopReport:
                    GameStopReport = handler as IGameStopReportCategory;
                    break;

                case MessageCategory.VoucherPrint:
                    VoucherPrint = handler;
                    break;

                case MessageCategory.Localization:
                    LocalizationControl = handler as ILocalizationCategory;
                    break;

                case MessageCategory.EgmConfigData:
                    EgmConfigData = handler as IEgmConfigDataCategory;
                    break;

                case MessageCategory.CustomConfigurationRead:
                    CustomConfigurationRead = handler as ICustomConfigurationReadCategory;
                    break;
            }
        }

        #endregion Private Methods
    }
}