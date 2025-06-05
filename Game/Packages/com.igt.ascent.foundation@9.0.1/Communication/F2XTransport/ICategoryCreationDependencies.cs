// -----------------------------------------------------------------------
// <copyright file = "ICategoryCreationDependencies.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// This interface defines the dependencies required during construction of an F2X category.
    /// </summary>
    public interface ICategoryCreationDependencies
    {
        /// <summary>
        /// Gets the transport object on which all categories are installed.
        /// </summary>
        IF2XTransport Transport { get; }

        /// <summary>
        /// Gets the object for posting transactional events.
        /// </summary>
        IEventCallbacks EventCallbacks { get; }

        /// <summary>
        /// Gets the object for posting non-transactional events.
        /// </summary>
        INonTransactionalEventCallbacks NonTransactionalEventCallbacks { get; }

        /// <summary>
        /// Gets a dependency from the available dependencies.
        /// </summary>
        /// <typeparam name="T">The type of the dependency to retrieve.</typeparam>
        /// <param name="required">
        /// A flag which, if false, allows this method to return null for dependencies that are not available. 
        /// Otherwise this method will throw if no dependency is available.
        /// </param>
        /// <returns>The required dependency, or null if it is not available and the dependency is not required.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the dependency is not available for a required request.
        /// </exception>
        T GetDependency<T>(bool required = true) where T : class;
    }
}