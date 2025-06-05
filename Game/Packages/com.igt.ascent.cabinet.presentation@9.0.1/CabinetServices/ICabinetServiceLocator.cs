// -----------------------------------------------------------------------
// <copyright file = "ICabinetServiceLocator.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    /// <summary>
    /// Interface for the device service locator, used to get access to services and to register service listeners.
    /// </summary>
    public interface ICabinetServiceLocator
    {
        /// <summary>
        /// Get a service instance by its type.
        /// </summary>
        /// <typeparam name="TService">Type of the service to get. Must be an interface.</typeparam>
        /// <returns>The current service instance, or null if no such service exists.</returns>
        /// <exception cref="CabinetServiceTypeException">Thrown if <typeparamref name="TService"/> is not an interface.</exception>
        TService GetService<TService>() where TService : class, ICabinetService;

        /// <summary>
        /// Register a listener for a specific service, so that it gets called when matching services are added or removed.
        /// </summary>
        /// <typeparam name="TService">Type of the service to register for. Must be an interface.</typeparam>
        /// <param name="listener">Instance of the listener to register.</param>
        /// <exception cref="CabinetServiceTypeException">Thrown if <typeparamref name="TService"/> is not an interface.</exception>
        void RegisterListener<TService>(ICabinetServiceListener<TService> listener)
            where TService : class, ICabinetService;

        /// <summary>
        /// Unregister a listener for a specific service, so that it is no longer called when matching services are added or removed.
        /// </summary>
        /// <typeparam name="TService">Type of the service to unregister from. Must be an interface.</typeparam>
        /// <param name="listener">Instance of the listener to unregister.</param>
        /// <exception cref="CabinetServiceTypeException">Thrown if <typeparamref name="TService"/> is not an interface.</exception>
        void UnregisterListener<TService>(ICabinetServiceListener<TService> listener)
            where TService : class, ICabinetService;
    }
}