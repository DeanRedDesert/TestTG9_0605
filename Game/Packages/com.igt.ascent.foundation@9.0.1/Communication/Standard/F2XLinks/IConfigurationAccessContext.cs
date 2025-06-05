// -----------------------------------------------------------------------
// <copyright file = "IConfigurationAccessContext.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface provides information on and validation for a <see cref="ConfigurationScope"/>.
    /// </summary>
    internal interface IConfigurationAccessContext
    {
        /// <summary>
        /// Gets a boolean flag that indicates if the implementation of this interface requires a configuration scope
        /// identifier from the <see cref="ConfigurationItemKey"/>.
        /// </summary>
        bool IsConfigurationScopeIdentifierRequired { get; }

        /// <summary>
        /// Checks if the access to the configuration scope is allowed.
        /// </summary>
        /// <param name="configurationScope">The type of the configuration scope to validate.</param>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown if the access to the given configuration scope is denied.
        /// </exception>
        void ValidateConfigurationAccess(ConfigurationScope configurationScope);
    }
}
