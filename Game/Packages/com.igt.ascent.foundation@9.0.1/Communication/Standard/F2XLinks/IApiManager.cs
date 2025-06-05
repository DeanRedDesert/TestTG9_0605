// -----------------------------------------------------------------------
// <copyright file = "IApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using F2XCallbacks;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// This interface represents an API Manager that manages the
    /// negotiation and installation of categories on a specific negotiation level.
    /// </summary>
    internal interface IApiManager
    {
        /// <summary>
        /// Gets the negotiation level this manager is responsible for.
        /// </summary>
        CategoryNegotiationLevel NegotiationLevel { get; }

        /// <summary>
        /// Initializes the API Manager with dependencies.
        /// </summary>
        /// <remarks>
        /// This is to be called by <see cref="LinkController"/>.
        /// </remarks>
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
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="categoryDependencies"/> is null.
        /// </exception>
        void Initialize(FoundationTarget target,
                        ICategoryNegotiationDependencies categoryDependencies,
                        IInterfaceExtensionDependencies baseExtensionDependencies = null,
                        IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations = null);

        /// <summary>
        /// Adds more category subscriptions after the construction of the API Manager.
        /// </summary>
        /// <param name="newSubscriptions">
        /// List of subscriptions to add.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this function is called after the category negotiation has started.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="newSubscriptions"/> is null.
        /// </exception>
        void AddSubscriptions(IList<CategorySubscription> newSubscriptions);

        /// <summary>
        /// Gets an extended interface if it was requested and installed.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. <see langword="null"/> if none was found.
        /// </returns>
        TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class;
    }
}