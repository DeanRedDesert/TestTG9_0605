// -----------------------------------------------------------------------
// <copyright file = "ICoplayerLib.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines functionality of a coplayer communication lib
    /// to be used by the coplayer clients.
    /// </summary>
    public interface ICoplayerLib : IAppLib
    {
        /// <summary>
        /// Gets the current context of the coplayer, including the information
        /// on the mount point, game mode, theme, payvar and denomination at play etc.
        /// </summary>
        ICoplayerContext Context { get; }

        /// <summary>
        /// Gets the interface for game cycle play.
        /// </summary>
        IGameCyclePlay GameCyclePlay { get; }

        /// <summary>
        /// Gets the interface for game cycle betting.
        /// </summary>
        IGameCycleBetting GameCycleBetting { get; }

        /// <summary>
        /// Gets the interface for access to the critical data for the coplayer theme.
        /// </summary>
        ICriticalDataStore ThemeStore { get; }

        /// <summary>
        /// Gets the interface for access to the critical data for the coplayer payvar.
        /// </summary>
        ICriticalDataStore PayvarStore { get; }

        /// <summary>
        /// Gets the interface for access to the game-play scope of critical data that is used for
        /// concurrent game cycle per coplayer.
        /// </summary>
        /// <remarks>
        /// It is different from legacy game cycle critical data scope that the GamePlayStore is NOT
        /// constrained by game cycle state, and it will be cleared by the Foundation when:
        /// <list>
        ///     <item>The game cycle successfully enters the <see cref="GameCycleState.Committed"/> state.</item>
        ///     <item>A new coplayer context is activated.</item>
        ///     <item>A new shell is selected for play.</item>
        ///     <item>On entering and exiting <see cref="GameMode.Utility"/> mode.</item>
        /// </list>
        /// </remarks>
        ICriticalDataStore GamePlayStore { get; }

        /// <summary>
        /// Gets the interface for access to the critical data for the coplayer history.
        /// </summary>
        ICriticalDataStore HistoryStore { get; }

        /// <summary>
        /// Gets an extended interface if it was requested and installed.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of. 
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed, then <see langword="null"/> will
        /// be returned.
        /// </returns>
        TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class;
    }
}