// -----------------------------------------------------------------------
// <copyright file = "IGameConfigurator.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System.Collections.Generic;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;

    /// <summary>
    /// This interface defines information and functionalities that
    /// must be provided by the game specific implementations.
    /// </summary>
    public interface IGameConfigurator
    {
        /// <summary>
        /// Gets the interface extensions requested by the game/cotheme.
        /// If granted, the interface extension can be accessed by
        /// <see cref="ICoplayerLib.GetInterface{T}()"/>.
        /// </summary>
        /// <remarks>
        /// Note that some interface extensions are only available to shell,
        /// not cothemes.  A typical example is the IThemeToExtParcelComm interface.
        /// It is an error for a cotheme to request shell only interface extensions.
        /// </remarks>
        /// <returns>
        /// List of interface extension configurations the game wishes to have.
        /// Null if none is needed.
        /// </returns>
        IReadOnlyList<IInterfaceExtensionConfiguration> GetInterfaceExtensionRequests();

        /// <summary>
        /// Creates the game state machine.
        /// </summary>
        /// <remarks>
        /// Note that history is taken care of by the state machine framework,
        /// hence transparent to individual game/cotheme implementation.
        /// Therefore, this API will only be called for
        /// <see cref="Communication.Platform.Interfaces.GameMode.Play"/> mode and
        /// <see cref="Communication.Platform.Interfaces.GameMode.Utility"/> mode,
        /// but never for <see cref="Communication.Platform.Interfaces.GameMode.History"/> mode.
        /// </remarks>
        /// <param name="coplayerContext">
        /// The context for which the game state machine is created.
        /// </param>
        /// <returns>
        /// The game state machine created.
        /// </returns>
        IGameStateMachine CreateGameStateMachine(ICoplayerContext coplayerContext);
    }
}