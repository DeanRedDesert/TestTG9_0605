// -----------------------------------------------------------------------
// <copyright file = "ICabinetServiceLocatorRestricted.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    /// <summary>
    /// Interface for the device service locator, used to add and remove services.
    /// </summary>
    public interface ICabinetServiceLocatorRestricted
    {
        /// <summary>
        /// Add a service to the locator and notify listeners. If the service already exists,
        /// the previous service will first be removed (and listeners notified).
        /// </summary>
        /// <typeparam name="TService">Type of the service to add. Must be an interface.</typeparam>
        /// <param name="service">Service to add.</param>
        /// <exception cref="CabinetServiceTypeException">Thrown if <typeparamref name="TService"/> is not an interface.</exception>
        void AddService<TService>(TService service) where TService : class, ICabinetService;

        /// <summary>
        /// Remove a service from the locator. If the service exists, listeners will be notified.
        /// </summary>
        /// <typeparam name="TService">Type of the service to remove. Must be an interface.</typeparam>
        /// <exception cref="CabinetServiceTypeException">Thrown if <typeparamref name="TService"/> is not an interface.</exception>
        void RemoveService<TService>() where TService : class, ICabinetService;
    }
}