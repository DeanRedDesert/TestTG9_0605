// -----------------------------------------------------------------------
// <copyright file = "IYaGameDataProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;

    /// <summary>
    /// Yet another interface that contains required behavior for game data providers.
    /// These providers have services that are driven by game managed data rather than Foundation data.
    /// </summary>
    public interface IYaGameDataProvider : INamedProvider
    {
        /// <summary>
        /// Gets the flag indicating whether the provider has been previously loaded from/saved to critical data.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Writes provider data to critical data.
        /// </summary>
        void Save();

        /// <summary>
        /// Reads critical data and restores provider data.
        /// </summary>
        /// <param name="allowsOverwrite">
        /// The flag indicating whether it is okay to overwrite the existing provider data that has been loaded
        /// from the critical data or has been saved to the critical data.
        /// </param>
        /// <remarks>
        /// Generally, the recommendation is to avoid loading the provider data from the critical data repeatedly.
        /// Best practice is to load it once at the time of start up, such as in the ReadConfiguration method of
        /// the state machine, then use <see cref="Save"/> method to sync the data into the safe storage.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provider data has been loaded before, and <paramref name="allowsOverwrite"/> is false,
        /// </exception>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        void Load(bool allowsOverwrite = false);
    }
}