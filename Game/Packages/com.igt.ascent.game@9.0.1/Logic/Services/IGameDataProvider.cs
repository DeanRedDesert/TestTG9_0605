//-----------------------------------------------------------------------
// <copyright file = "IGameDataProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// Interface that contains required behavior for game data providers.
    /// These providers have services that are driven by game managed data rather than Foundation data.
    /// </summary>
    /// <remarks>
    /// See <see cref="GameDataProvider{T}"/> and <see cref="SerializableGameDataProvider"/> for
    /// examples of how to implement this interface. These two abstract classes were designed to be 
    /// conveniently derived from as necessary.
    /// </remarks>
    public interface IGameDataProvider : INamedProvider
    {
        /// <summary>
        /// Writes provider data to critical data.
        /// </summary>
        /// <param name="gameCriticalData">
        /// The critical data access point.
        /// </param>
        void Save(IGameCriticalData gameCriticalData);

        /// <summary>
        /// Reads critical data and restores provider data.
        /// </summary>
        /// <param name="gameCriticalData">
        /// The critical data access point.
        /// </param>
        void Load(IGameCriticalData gameCriticalData);
    }
}