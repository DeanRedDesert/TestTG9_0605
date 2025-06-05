// -----------------------------------------------------------------------
// <copyright file = "IDiscoverableCabinetServiceLocatorRestricted.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;

    /// <summary>
    /// Interface for the cabinet service locator, used to add and remove MEF discoverable services.
    /// </summary>
    public interface IDiscoverableCabinetServiceLocatorRestricted
    {
        /// <summary>
        /// Add a discovered service to the locator and notify listeners. If the service already exists,
        /// the previous service will first be removed (and listeners notified).
        /// </summary>        
        /// <param name="service">
        /// A unique type that implements <see cref="IMefDiscoverableService"/>. The actual run-time type
        /// to be added is found inspecting the 'ExportedType' property in this interface.
        /// </param>
        void AddDiscoveredService(IMefDiscoverableService service);

        /// <summary>
        /// Remove a discovered service from the locator. If the service exists, listeners will be notified.
        /// </summary>
        /// <param name="runTimeDiscoveredService">
        /// Type of the previously added discoverable service to remove.
        /// </param>
        void RemoveDiscoveredService(Type runTimeDiscoveredService);
    }
}