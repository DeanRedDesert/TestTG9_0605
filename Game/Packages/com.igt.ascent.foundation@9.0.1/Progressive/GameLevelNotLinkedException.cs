//-----------------------------------------------------------------------
// <copyright file = "GameLevelNotLinkedException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This exception indicates that a game level is not linked
    /// to any of the controller levels supported by a progressive
    /// controller.
    /// </summary>
    [Serializable]
    public class GameLevelNotLinkedException : Exception
    {
        /// <summary>
        /// The game level in question.
        /// </summary>
        public int GameLevel { get; private set; }

        private const string MessageFormat = "Game level {0} is not linked to any of the controller levels.";

        /// <summary>
        /// Construct a GameLevelNotLinkedException with a general message.
        /// </summary>
        /// <param name="gameLevel">The game level in question.</param>
        public GameLevelNotLinkedException(int gameLevel)
            : this(gameLevel, string.Format(CultureInfo.InvariantCulture, MessageFormat, gameLevel))
        {
        }

        /// <summary>
        /// Construct a GameLevelNotLinkedException with a general message and an innerException exception.
        /// </summary>
        /// <param name="gameLevel">The game level in question.</param>
        /// <param name="innerException">The inner exception.</param>
        public GameLevelNotLinkedException(int gameLevel, Exception innerException)
            : this(gameLevel, string.Format(CultureInfo.InvariantCulture, MessageFormat, gameLevel), innerException)
        {
        }

        /// <summary>
        /// Construct a GameLevelNotLinkedException with a specific message.
        /// </summary>
        /// <param name="gameLevel">The game level in question.</param>
        /// <param name="message">The message for the exception.</param>
        public GameLevelNotLinkedException(int gameLevel, string message)
            :base(message)
        {
            GameLevel = gameLevel;
        }

        /// <summary>
        /// Construct a GameLevelNotLinkedException with a specific message and an innerException exception.
        /// </summary>
        /// <param name="gameLevel">The game level in question.</param>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public GameLevelNotLinkedException(int gameLevel, string message, Exception innerException)
            : base(message, innerException)
        {
            GameLevel = gameLevel;
        }
    }
}
