// -----------------------------------------------------------------------
// <copyright file = "InterfaceExtensionManager.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// Manages interface extension requests and handles their category
    /// installation in a thread-safe manner.
    /// </summary>
    internal sealed class InterfaceExtensionManager : IDisposable
    {
        #region Private Fields

        /// <summary>
        ///The negotiation level this manager works for.
        /// </summary>
        private readonly CategoryNegotiationLevel negotiationLevel;

        /// <summary>
        /// The common dependencies required to create interface extensions.
        /// </summary>
        private readonly IInterfaceExtensionDependencies baseExtensionDependencies;

        /// <summary>
        /// Dictionary of list of extension categories that will be requested during API negotiation by the negotiation level.
        /// </summary>
        private readonly List<CategoryRequest> requestedCategories = new List<CategoryRequest>();

        /// <summary>
        /// Dictionary of requested extension configurations keyed by the category.
        /// </summary>
        private readonly ConcurrentDictionary<MessageCategory, IF2XInterfaceExtensionConfiguration> requestedExtensions =
            new ConcurrentDictionary<MessageCategory, IF2XInterfaceExtensionConfiguration>();

        /// <summary>
        /// Dictionary of interface extensions keyed by their interface type
        /// that holds the valid/available extensions for the current round of theme API version negotiation.
        /// </summary>
        private readonly ConcurrentDictionary<Type, IInterfaceExtension> currentInterfaceExtensions =
            new ConcurrentDictionary<Type, IInterfaceExtension>();

        /// <summary>
        /// A dictionary of interface extensions that keyed by their interface type
        /// that caches all the interface extensions created.
        /// </summary>
        private readonly ConcurrentDictionary<Type, IInterfaceExtension> allInterfaceExtensions =
            new ConcurrentDictionary<Type, IInterfaceExtension>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="InterfaceExtensionManager"/>.
        /// </summary>
        /// <param name="negotiationLevel">
        ///The negotiation level this manager works for.
        /// </param>
        /// <param name="baseExtensionDependencies">
        /// The common dependencies required to create interface extensions.
        /// </param>
        internal InterfaceExtensionManager(CategoryNegotiationLevel negotiationLevel, IInterfaceExtensionDependencies baseExtensionDependencies)
        {
            this.negotiationLevel = negotiationLevel;
            this.baseExtensionDependencies = baseExtensionDependencies;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves category requests from the F2X Interface Extension Configurations. Also saves the
        /// configurations so they can be later used to create the interface extension objects when
        /// the handler is installed.
        /// </summary>
        /// <param name="foundationTarget">
        /// Foundation version to target.
        /// </param>
        /// <param name="configurations">
        /// List of interface extension configurations to request.
        /// </param>
        /// <returns>
        /// A collection of category requests.
        /// </returns>
        public void RequestInterfaces(FoundationTarget foundationTarget, IEnumerable<IInterfaceExtensionConfiguration> configurations)
        {
            if(configurations == null)
            {
                return;
            }

            requestedCategories.Clear();

            var eligibleConfigurations = configurations.OfType<IF2XInterfaceExtensionConfiguration>()
                                                       .Where(configuration => configuration.NegotiationLevel.HasFlag(negotiationLevel));

            foreach(var interfaceConfiguration in eligibleConfigurations)
            {
                // Check if there is a category request for current foundation target.
                var eligibleCategories = interfaceConfiguration.GetCategoryRequests()
                                                               .Where(request => request.FoundationTarget.HasFlag(foundationTarget))
                                                               .ToList();

                requestedCategories.AddRange(eligibleCategories);

                foreach(var categoryRequest in eligibleCategories)
                {
                    // Save the extension configuration to create the interface if installed in InstalApiCategory.
                    requestedExtensions[(MessageCategory)categoryRequest.CategoryVersionInformation.Category]
                        = interfaceConfiguration;
                }
            }
        }

        /// <summary>
        /// Gets a collection of category requests for requested interface extensions.
        /// </summary>
        /// <returns>
        /// The collection of requested categories.
        /// </returns>
        public IReadOnlyList<CategoryRequest> GetRequestedCategories()
        {
            return requestedCategories;
        }

        /// <summary>
        /// Remove installed interface extensions.
        /// </summary>
        /// <remarks>
        /// This should be called before a new round of API version negotiation of the corresponding negotiation level.
        /// </remarks>
        public void UninstallInterfaceExtensions()
        {
            currentInterfaceExtensions.Clear();
        }

        /// <summary>
        /// Creates and stores an <see cref="IInterfaceExtension"/> if the specified
        /// <paramref name="apiCategoryHandler"/> belongs to an extension interface.
        /// </summary>
        /// <param name="apiCategoryHandler">
        /// Installed <see cref="IApiCategory"/>.
        /// </param>
        /// <remarks>
        /// This method uses a cache of interface extensions for multiple rounds of API version negotiation.
        /// If the specified category belongs to extension interface created previously, the existing extension
        /// will be used instead of instantiating a new instance.
        ///
        /// API version negotiation can happen multiple rounds during an application's life time.  For example:
        /// the Theme API version negotiation (on F2L link level) happens not only at game start,
        /// but also during theme switching for a stacked(multiple)-theme game;
        /// The AscribedGame API version negotiation happens whenever a new linked AscribedGame is activated;
        /// The App API version negotiation happens when a different App Extension in the extension bin is to be activated.
        /// </remarks>
        public void HandleInstalledCategory(IApiCategory apiCategoryHandler)
        {
            if(!requestedExtensions.TryGetValue(apiCategoryHandler.Category, out var requestedExtension))
            {
                return;
            }

            // This caching is based on the following assumptions:
            // 1. For a requested category, the Foundation will always give us the same version of the category.
            // 2. The Foundation might not always set the same set of categories for interface extensions,
            //    thus a category that is available for one theme can become unavailable for another.
            if(allInterfaceExtensions.ContainsKey(requestedExtension.InterfaceType))
            {
                var extension = allInterfaceExtensions[requestedExtension.InterfaceType];
                currentInterfaceExtensions[requestedExtension.InterfaceType] = extension;

                // Attempt to get inquiry interface
                if(extension is IInterfaceExtensionInquiry inquiry && allInterfaceExtensions.ContainsKey(inquiry.InterfaceType))
                {
                    currentInterfaceExtensions[inquiry.InterfaceType] = allInterfaceExtensions[inquiry.InterfaceType];
                }
            }
            else
            {
                var dependencies = new F2XInterfaceExtensionDependencies(apiCategoryHandler,
                                                                         baseExtensionDependencies);
                var extension = requestedExtension.CreateInterfaceExtension(dependencies);

                allInterfaceExtensions[requestedExtension.InterfaceType] = extension;
                currentInterfaceExtensions[requestedExtension.InterfaceType] = extension;

                // Attempt to get inquiry interface.
                if(extension is IInterfaceExtensionInquiry inquiry)
                {
                    allInterfaceExtensions[inquiry.InterfaceType] = extension;
                    currentInterfaceExtensions[inquiry.InterfaceType] = extension;
                }
            }
        }

        /// <summary>
        /// Gets an extended interface if it was requested and installed.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. <see langword="null"/> if none is found.
        /// </returns>
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            TExtendedInterface extension = null;

            if(currentInterfaceExtensions.TryGetValue(typeof(TExtendedInterface), out var interfaceExtension))
            {
                extension = interfaceExtension as TExtendedInterface;
            }

            return extension;
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            currentInterfaceExtensions.Clear();

            // Dispose all interface extensions.
            foreach(var extensionEntry in allInterfaceExtensions)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                var disposableInterfaceExtension = extensionEntry.Value as IDisposable;
                disposableInterfaceExtension?.Dispose();
            }

            allInterfaceExtensions.Clear();
        }

        #endregion
    }
}