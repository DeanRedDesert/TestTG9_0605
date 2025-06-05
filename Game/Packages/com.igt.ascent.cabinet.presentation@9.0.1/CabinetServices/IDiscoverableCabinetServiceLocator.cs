// -----------------------------------------------------------------------
// <copyright file = "IDiscoverableCabinetServiceLocator.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;

    /// <summary>
    /// Interface for the discoverable cabinet service locator, used to get access to services and to register service listeners.
    /// </summary>
    public interface IDiscoverableCabinetServiceLocator
    {
        /// <summary>
        /// Register a listener for a specific discoverable service, so that it gets called when matching services are added or removed.
        /// </summary>
        /// <param name="mefDiscoverableService">The discoverable service of type <see cref="IMefDiscoverableService"/>.</param>
        /// <param name="listener">The listener of type <see cref="IDiscoverableCabinetServiceListener"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="mefDiscoverableService"/> or
        /// <paramref name="mefDiscoverableService"/> are null. </exception>
        void RegisterDiscoverableListener(IMefDiscoverableService mefDiscoverableService, IDiscoverableCabinetServiceListener listener);

        /// <summary>
        /// Unregister a listener for a specific service, so that it is no longer called when matching services are added or removed.
        /// </summary>
        /// <param name="mefDiscoverableService">The discoverable service of type <see cref="IMefDiscoverableService"/>.</param>
        /// <param name="listener">The listener of type <see cref="IDiscoverableCabinetServiceListener"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="mefDiscoverableService"/> or
        /// <paramref name="mefDiscoverableService"/> are null. </exception>
        void UnregisterDiscoverableListener(IMefDiscoverableService mefDiscoverableService, IDiscoverableCabinetServiceListener listener);
    }
}