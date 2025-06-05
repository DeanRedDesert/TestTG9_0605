// -----------------------------------------------------------------------
// <copyright file = "ApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using F2XCallbacks;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// Base class of all API Managers that manages the negotiation and installation of
    /// categories on a specific negotiation level.
    /// </summary>
    /// <devdoc>
    /// The construction of an API Manager is a collaboration between the link class and <see cref="LinkController"/>.
    /// The former provides the subscription and API Status Callbacks via the manager's constructor;
    /// while the latter provides the transport etc. that are needed by the negotiation and installation of categories
    /// via <see cref="IApiManager.Initialize"/> method.
    /// </devdoc>
    internal abstract class ApiManager : IApiManager, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// The list of categories the user of this manager would like to subscribe.
        /// </summary>
        private readonly List<CategorySubscription> subscriptions;

        /// <summary>
        /// List of all category handlers that have been created, each for a specific category and version.
        /// </summary>
        private readonly ConcurrentDictionary<CategoryVersionInformation, IApiCategory> allCategoryHandlers =
            new ConcurrentDictionary<CategoryVersionInformation, IApiCategory>();

        /// <summary>
        /// The Foundation version to target.
        /// </summary>
        private FoundationTarget foundationTarget;

        /// <summary>
        /// The dependencies required to create categories.
        /// </summary>
        private ICategoryNegotiationDependencies negotiationDependencies;

        /// <summary>
        /// The interface extension manager.
        /// </summary>
        private InterfaceExtensionManager interfaceExtensionManager;

        /// <summary>
        /// A list of all the categories that are supported on the specific negotiation level.
        /// </summary>
        private List<CategoryRequest> supportedCategories;

        /// <summary>
        /// A list of the categories requested by this manager in the API negotiation.
        /// </summary>
        private IList<CategoryRequest> requestedCategories;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ApiManager"/>.
        /// </summary>
        /// <param name="negotiationLevel">
        /// The negotiation level this manager is responsible for.
        /// </param>
        /// <param name="subscriptions">
        /// The list of categories the caller would like to subscribe.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        protected ApiManager(CategoryNegotiationLevel negotiationLevel,
                             IList<CategorySubscription> subscriptions)
        {
            if(subscriptions == null)
            {
                throw new ArgumentNullException(nameof(subscriptions));
            }

            NegotiationLevel = negotiationLevel;
            this.subscriptions = subscriptions.ToList();
        }

        #endregion

        #region IApiManager Implementation

        /// <inheritdoc/>
        public CategoryNegotiationLevel NegotiationLevel { get; }

        /// <inheritdoc/>
        public virtual void Initialize(FoundationTarget target,
                                       ICategoryNegotiationDependencies categoryDependencies,
                                       IInterfaceExtensionDependencies baseExtensionDependencies = null,
                                       IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations = null)
        {
            DoInitialize(target, categoryDependencies, baseExtensionDependencies, interfaceExtensionConfigurations, false);
        }

        /// <inheritdoc/>
        public void AddSubscriptions(IList<CategorySubscription> newSubscriptions)
        {
            if(newSubscriptions == null)
            {
                throw new ArgumentNullException(nameof(newSubscriptions));
            }

            if(requestedCategories != null)
            {
                throw new InvalidOperationException("Cannot add new subscriptions after the negotiation has started.");
            }

            subscriptions.AddRange(newSubscriptions);
        }

        /// <inheritdoc />
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            return interfaceExtensionManager.GetInterface<TExtendedInterface>();
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            // Dispose all category handlers.
            foreach(var categoryEntry in allCategoryHandlers)
            {
                var disposableHandler = categoryEntry.Value as IDisposable;
                disposableHandler?.Dispose();
            }

            interfaceExtensionManager.Dispose();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Do the initialization work.
        /// </summary>
        /// <param name="target">
        /// The version of Foundation to target.
        /// </param>
        /// <param name="categoryDependencies">
        /// The dependencies required to create categories.
        /// </param>
        /// <param name="baseExtensionDependencies">
        /// The common dependencies required to create interface extensions.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// List of interface extension configurations to request.
        /// </param>
        /// <param name="isConnectionLevel">
        /// The flag indicating if this is to initialize a ConnectApiManager.
        /// There is no Connect defined in the Negotiation Level enum, so we use this flag instead.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="categoryDependencies"/> is null.
        /// </exception>
        protected void DoInitialize(FoundationTarget target,
                                    ICategoryNegotiationDependencies categoryDependencies,
                                    IInterfaceExtensionDependencies baseExtensionDependencies,
                                    IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations,
                                    bool isConnectionLevel)
        {
            foundationTarget = target;
            negotiationDependencies = categoryDependencies ?? throw new ArgumentNullException(nameof(categoryDependencies));

            interfaceExtensionManager = new InterfaceExtensionManager(NegotiationLevel, baseExtensionDependencies);
            interfaceExtensionManager.RequestInterfaces(foundationTarget, interfaceExtensionConfigurations);

            if(isConnectionLevel)
            {
                // Connect API Manager only supports the LinkControl category,
                // and does not modify subscriptions for interface extensions.
                supportedCategories = CategorySupportRegistry.GetSupportedLinkControlList()
                                                             .Where(categoryRequest => categoryRequest.FoundationTarget.HasFlag(foundationTarget))
                                                             .ToList();
            }
            else
            {
                var extensionRequests = interfaceExtensionManager.GetRequestedCategories();

                // Populate the list of supported categories which are:
                // 1. On the negotiation level; 2. Targeting the given Foundation target;
                // It also includes the categories requested by the interface extension configurations.
                supportedCategories = CategorySupportRegistry.GetSupportedApiList(NegotiationLevel)
                                                             .Where(categoryRequest => categoryRequest.FoundationTarget.HasFlag(foundationTarget))
                                                             .Concat(extensionRequests)
                                                             .ToList();

                // Requested interface extensions are always subscribed
                var extensionSubscriptions = extensionRequests.Select(
                    request => new CategorySubscription((MessageCategory)request.CategoryVersionInformation.Category,
                                                        request.Required));

                // Add the subscriptions by interface extensions to the list.
                subscriptions.AddRange(extensionSubscriptions);
            }
        }

        /// <summary>
        /// Gets a list of the categories requested by this manager in the API negotiation.
        /// </summary>
        /// <devdoc>
        /// This method is first called when responding to a GetApiVersions message,
        /// So requestedCategories not being null can be used as an indicator that
        /// the negotiation has started.
        /// </devdoc>
        protected IList<CategoryRequest> GetRequestedCategories()
        {
            return requestedCategories ?? (requestedCategories = ParseSubscriptions());
        }

        /// <summary>
        /// Attempts to install all of the selected category handlers if all required categories are present.
        /// </summary>
        /// <param name="selectedCategories">
        /// All of the categories and versions selected by the Foundation.
        /// </param>
        /// <param name="installedHandlers">
        /// [Out] If true is returned, contains all of the installed category handlers.
        /// </param>
        /// <returns>
        /// Whether all the required categories were present and successfully installed.
        /// </returns>
        protected bool TryInstallApiCategories(IList<CategoryVersionInformation> selectedCategories,
                                               out IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            if(NegotiationLevel == CategoryNegotiationLevel.Link)
            {
                negotiationDependencies.Transport.UninstallControlLevelCategoryHandlers();
            }
            else
            {
                negotiationDependencies.Transport.UninstallControlLevelCategoryHandlers(
                    supportedCategories.Select(
                        categoryRequest => (MessageCategory)categoryRequest.CategoryVersionInformation.Category));
            }

            interfaceExtensionManager.UninstallInterfaceExtensions();

            var success = false;
            installedHandlers = null;

            // Make sure no must-have category is missing from the selected list.
            // If any is missing, return false.
            if(!AreAnyRequiredCategoriesMissing(selectedCategories))
            {
                installedHandlers = new Dictionary<MessageCategory, IApiCategory>(selectedCategories.Count);

                foreach(var categoryVersion in selectedCategories)
                {
                    var handler = InstallApiCategory(categoryVersion,
                                                     GetCategoryCreator(categoryVersion));
                    installedHandlers.Add((MessageCategory)categoryVersion.Category, handler);
                }

                success = true;
            }

            return success;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parses all the category subscriptions to obtain a list of categories to request from the Foundation.
        /// </summary>
        /// <returns>
        /// The list of categories to request.
        /// </returns>
        private IList<CategoryRequest> ParseSubscriptions()
        {
            var subscribedCategories = subscriptions.Select(subscription => (int)subscription.Category)
                                                    .ToList();
            var result = supportedCategories
                .Where(request => subscribedCategories.Contains(request.CategoryVersionInformation.Category))
                .ToList();

            // Get the required category.
            var requiredCategories = subscriptions
                .Where(categorySubscription => categorySubscription.Required)
                .Select(categorySubscription => (int)categorySubscription.Category);

            // Check if any of the required category subscription is not supported.
            var missingCategories = requiredCategories
                .Except(result.Select(request => request.CategoryVersionInformation.Category)).ToList();

            // Thrown an exception with information on all missing required category subscription(s)
            // for the current foundation target.
            if(missingCategories.Any())
            {
                var missingCategoriesMessageString =
                    missingCategories.Aggregate(string.Empty,
                                                (current, missingCategory) => current + " " + (MessageCategory)missingCategory);

                throw new InvalidOperationException($"Categories subscribed: {missingCategoriesMessageString} are not supported by " +
                                                    $"the current foundation target {foundationTarget}.");
            }

            foreach(var categoryRequest in result)
            {
                categoryRequest.Required = subscriptions
                    .First(subscription => (int)subscription.Category ==
                                           categoryRequest.CategoryVersionInformation.Category)
                    .Required;
            }

            // We don't check for duplicate categories here.
            // It will be checked by linkTransport when installing the categories.

            return result;
        }

        /// <summary>
        /// Creates a handler for the specified category and version, installs it with the link transport,
        /// and assigns the related public properties.
        /// </summary>
        /// <param name="categoryVersion">
        /// The category and version to install.
        /// </param>
        /// <param name="categoryCreator">
        /// The delegate used to instantiate the category.
        /// </param>
        /// <remarks>
        /// This method maintains a list of handlers created so far.
        /// If the handler already exists, it will use the existing handler instead of instantiating a new one.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="categoryVersion"/> is not supported by the application.
        /// </exception>
        private IApiCategory InstallApiCategory(CategoryVersionInformation categoryVersion,
                                                CreateCategoryDelegate categoryCreator)
        {
            // Check if the category and version is supported.
            if(categoryCreator == null)
            {
                throw new ArgumentException($"Category and version {categoryVersion} is not supported by the application.");
            }

            // Check if a handler already exists for this category and version.
            if(!allCategoryHandlers.TryGetValue(categoryVersion, out var handler))
            {
                // If not, create the handler.
                handler = categoryCreator(negotiationDependencies);

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
                allCategoryHandlers.TryAdd(categoryVersion, handler);
            }

            // Install the message category.
            negotiationDependencies.Transport.InstallCategoryHandler(handler);

            // If the category belongs to an extension, create its interface
            interfaceExtensionManager.HandleInstalledCategory(handler);

            return handler;
        }

        /// <summary>
        /// Verifies if any of the required category subscriptions are missing from the Foundation-selected
        /// category list.
        /// </summary>
        /// <param name="selectedCategories">The categories selected by the Foundation during the
        /// API negotiation.</param>
        /// <returns>Whether any of the required category subscriptions were missing.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="selectedCategories"/> is null.
        /// </exception>
        private bool AreAnyRequiredCategoriesMissing(IList<CategoryVersionInformation> selectedCategories)
        {
            if(selectedCategories == null)
            {
                throw new ArgumentNullException(nameof(selectedCategories));
            }

            return GetRequestedCategories().Any(requestedCategory =>
                                                    requestedCategory.Required &&
                                                    !selectedCategories.Contains(requestedCategory.CategoryVersionInformation));
        }

        /// <summary>
        /// Gets the delegate the creates the category implementation.
        /// </summary>
        /// <param name="categoryVersion">
        /// The version information for a category selected by the Foundation during the API negotiation.
        /// </param>
        /// <returns>
        /// The delegate to create the specified category implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="categoryVersion"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the category specified by <paramref name="categoryVersion"/> is not supported or
        /// was not requested with during negotiation.
        /// </exception>
        private CreateCategoryDelegate GetCategoryCreator(CategoryVersionInformation categoryVersion)
        {
            if(categoryVersion == null)
            {
                throw new ArgumentNullException(nameof(categoryVersion));
            }

            var creator = GetRequestedCategories().Where(categoryRequest =>
                                                             Equals(categoryRequest.CategoryVersionInformation, categoryVersion))
                                                  .Select(categoryRequest => categoryRequest.CreateCategory)
                                                  .FirstOrDefault();

            if(creator == null)
            {
                throw new InvalidOperationException($"Category and version {categoryVersion} is not supported or was not subscribed.");
            }

            return creator;
        }

        #endregion
    }
}