// -----------------------------------------------------------------------
// <copyright file = "IStandaloneRegistryInformationDependency.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interface that provides methods for retrieving specific registry values
    /// for standalone interface extension implementations.
    /// </summary>
    /// <remarks>
    /// Expand with registry methods as needed.
    /// </remarks>
    public interface IStandaloneRegistryInformationDependency
    {
        /// <summary>
        /// Retrieves all of the <see cref="IPayvarInfo"/> items for the current active
        /// payvar if it is a payvar group template.
        /// </summary>
        /// <returns>
        /// Returns a collection of payvars for the current active payvar if it is a group template.
        /// If current active payvar is not a group template, returns an empty collection.
        /// </returns>
        ReadOnlyCollection<IPayvarInfo> GetPayvarsInCurrentGroup();
    }
}
