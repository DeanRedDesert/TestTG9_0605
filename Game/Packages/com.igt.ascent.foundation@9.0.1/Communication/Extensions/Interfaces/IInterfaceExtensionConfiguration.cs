//-----------------------------------------------------------------------
// <copyright file = "IInterfaceExtensionConfiguration.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;

    /// <summary>
    /// Interface to use for extension configurations. This interface contains base level functionality common to all
    /// extension configurations. Additional interfaces may be needed for extra platform information.
    /// </summary>
    public interface IInterfaceExtensionConfiguration
    {
        /// <summary>
        /// Instantiate the extended interface implementation.
        /// </summary>
        /// <exception cref="InterfaceExtensionDependencyException">
        /// Thrown when there is an issue creating the interface with the given dependency information.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dependencies"/> is null.</exception>
        IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies);

        /// <summary>
        /// Gets the type of the interface. The value must derive from <see cref="IInterfaceExtension"/>.
        /// </summary>
        Type InterfaceType { get; }
    }
}
