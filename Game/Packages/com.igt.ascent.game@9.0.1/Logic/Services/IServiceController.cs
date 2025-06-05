// -----------------------------------------------------------------------
// <copyright file = "IServiceController.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;

    /// <summary>
    /// This interface allows a state machine and its states to register service providers.
    /// </summary>
    public interface IServiceController
    {
        /// <summary>
        /// Adds a service provider object with a specific name to the Service Controller.
        /// Once a provider is added, all of that provider's game services will be exposed
        /// to the Presentation.
        /// </summary>
        /// <param name="provider">
        /// Object containing game services to expose.
        /// </param>
        /// <param name="providerName">
        /// The name that this provider instance is to be exposed as.
        /// The Presentation will use this name to locate the provider
        /// and reference the game services within.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="provider"/> or <paramref name="providerName"/> is null.
        /// </exception>
        /// <exception cref="DuplicateServiceProviderException">
        /// Thrown when <paramref name="providerName"/> is a duplicate of any existing provider names.
        /// </exception>
        /// <remarks>
        /// Adding providers in the service controller by multiple threads is not guaranteed thread-safe;
        /// Updating the services by multiple threads is not thread-safe either.
        /// It's the caller's obligation to implement the thread synchronization in multi-thread situations.
        /// </remarks>
        void AddProvider(object provider, string providerName);

        /// <summary>
        /// Adds a service provider that implements <see cref="INamedProvider"/> to the Service Controller.
        /// Once a provider is added, all of that provider's game services will be exposed to the Presentation.
        /// </summary>
        /// <param name="provider">
        /// Object containing game services to expose.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="provider"/> is null.
        /// </exception>
        /// <exception cref="DuplicateServiceProviderException">
        /// Thrown when <paramref name="provider"/> has a name that is a duplicate of any existing provider names.
        /// </exception>
        void AddProvider(INamedProvider provider);
    }
}