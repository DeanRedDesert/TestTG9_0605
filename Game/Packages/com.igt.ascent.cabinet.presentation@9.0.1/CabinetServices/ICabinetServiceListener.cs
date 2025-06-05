// -----------------------------------------------------------------------
// <copyright file = "ICabinetServiceListener.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    /// <summary>
    /// Common parent listener interface for storage.
    /// </summary>
    public interface ICabinetServiceListener { }

    /// <summary>
    /// Interface for a listener callback object used by <see cref="CabinetServiceLocator"/> to report changes in services.
    /// </summary>
    /// <typeparam name="TService">Type of service to listen for changes on.</typeparam>
    public interface ICabinetServiceListener<in TService> : ICabinetServiceListener where TService : class, ICabinetService
    {
        /// <summary>
        /// Called when a service of the appropriate type becomes available.
        /// </summary>
        /// <param name="service">Instance of the newly available service.</param>
        void OnServiceAvailable(TService service);

        /// <summary>
        /// Called when a service of the appropriate type gets removed.
        /// </summary>
        /// <param name="service">Instance of the service getting removed.</param>
        void OnServiceRemoved(TService service);
    }
}