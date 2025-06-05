//-----------------------------------------------------------------------
// <copyright file = "FunctionCallNotAllowedInModeOrStateException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Globalization;
    using Platform.Interfaces;

    /// <summary>
    /// This exception indicates that a method was invoked in a Game Mode or
    /// a Game Cycle State in which it is not supposed to be invoked.
    /// </summary>
    [Serializable]
    public class FunctionCallNotAllowedInModeOrStateException : Exception
    {
        /// <summary>
        /// Name of the game mode when the exception is thrown.
        /// </summary>
        public string ModeName { get; private set; }

        /// <summary>
        /// Name of the game cycle state when the exception is thrown.
        /// </summary>
        public string StateName { get; private set; }

        private const string MessageFormat = "{0} It is being called in {1} mode and {2} state.";

        /// <summary>
        /// Construct an InvalidFunctionCallInStateException with a mode and a state.
        /// </summary>
        /// <param name="mode">The game mode where the method is wrongly invoked.</param>
        /// <param name="state">The game cycle state where the method is wrongly invoked.</param>
        public FunctionCallNotAllowedInModeOrStateException(GameMode mode, GameCycleState state)
            : this("The function call is not allowed in the game mode or the game cycle state.", mode, state)
        {
        }

        /// <summary>
        /// Construct an InvalidFunctionCallInStateException with a mode, a state and an inner exception.
        /// </summary>
        /// <param name="mode">The game mode where the method is wrongly invoked.</param>
        /// <param name="state">The game cycle state where the method is wrongly invoked.</param>
        /// <param name="ex">The inner exception.</param>
        public FunctionCallNotAllowedInModeOrStateException(GameMode mode, GameCycleState state, Exception ex)
            : this("The function call is not allowed in the game mode or the game cycle state.", mode, state, ex)
        {
        }

        /// <summary>
        /// Construct an InvalidFunctionCallInStateException with a message, a mode and a state.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="mode">The game mode where the method is wrongly invoked.</param>
        /// <param name="state">The game cycle state where the method is wrongly invoked.</param>
        public FunctionCallNotAllowedInModeOrStateException(string message, GameMode mode, GameCycleState state)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, mode, state))
        {
            ModeName = mode.ToString();
            StateName = state.ToString(); 
        }

        /// <summary>
        /// Construct an InvalidFunctionCallInStateException with a message,
        /// a mode, a state and an inner exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="mode">The game mode where the method is wrongly invoked.</param>
        /// <param name="state">The game cycle state where the method is wrongly invoked.</param>
        /// <param name="ex">The inner exception.</param>
        public FunctionCallNotAllowedInModeOrStateException(string message, GameMode mode, GameCycleState state, Exception ex)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, mode, state), ex)
        {
            ModeName = mode.ToString();
            StateName = state.ToString();
        }
    }
}
