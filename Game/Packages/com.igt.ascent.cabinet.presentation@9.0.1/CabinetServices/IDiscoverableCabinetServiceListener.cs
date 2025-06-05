// -----------------------------------------------------------------------
// <copyright file = "IDiscoverableCabinetServiceListener.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;

    /// <summary>
    /// Interface for a listener callback object used by <see cref="CabinetServiceLocator"/> to report changes
    /// in discovered services.
    /// </summary>
    public interface IDiscoverableCabinetServiceListener : ICabinetServiceListener
    {
        /// <summary>
        /// Called when a discovered service of the appropriate type becomes available.
        /// </summary>
        /// <param name="service">Type of the newly available discovered service.</param>
        void OnServiceAvailable(Type service);

        /// <summary>
        /// Called when a service of the appropriate type gets removed.
        /// </summary>
        /// <param name="service">Type of the discovered service getting removed.</param>
        void OnServiceRemoved(Type service);
    }
}