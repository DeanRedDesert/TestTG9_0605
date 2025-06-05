//-----------------------------------------------------------------------
// <copyright file = "IStateMachineContext.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines the context information that are used to construct a state machine.
    /// </summary>
    public interface IStateMachineContext
    {
        /// <summary>
        /// The game mode.
        /// </summary>
        GameMode GameContextMode { get; }

        /// <summary>
        /// The game sub-mode.
        /// </summary>
        GameSubMode GameSubMode { get; }

        /// <summary>
        /// The paytable to use in the context.
        /// </summary>
        string PaytableName { get; }

        /// <summary>
        /// The paytable file to use in the context.
        /// </summary>
        string PaytableFileName { get; }

        /// <summary>
        /// The resource for the game theme.
        /// </summary>
        ThemeResource ThemeResource { get; }

        /// <summary>
        /// The flag indicating if the current context(play mode) is newly selected for play.
        /// </summary>
        bool IsNewlySelectedForPlay { get; }

        /// <summary>
        /// Reset the newly selected for play flag of the current context(play mode).
        /// </summary>
        /// <remarks>
        /// A <see cref="FunctionCallNotAllowedInModeOrStateException"/> may be thrown if this method is called
        /// in other game context mode rather than <seealso cref="GameMode.Play"/> depending on the implementation.
        /// An <see cref="InvalidTransactionException"/> may be thrown if this method is called without a
        /// valid transaction opened depending on the implementation.
        /// </remarks>
        void ResetNewlySelectedForPlay();
    }
}
