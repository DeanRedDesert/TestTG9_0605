// -----------------------------------------------------------------------
// <copyright file = "INamedProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    /// <summary>
    /// Interface that specifies the behavior of a provider with a name.
    /// Used by <see cref="ServiceController"/> to register a provider by name.
    /// </summary>
    public interface INamedProvider
    {
        /// <summary>
        /// Gets the provider name.
        /// </summary>
        string Name { get; }
    }
}