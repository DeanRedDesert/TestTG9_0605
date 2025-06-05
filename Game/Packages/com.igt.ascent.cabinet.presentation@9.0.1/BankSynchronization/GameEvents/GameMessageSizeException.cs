// -----------------------------------------------------------------------
// <copyright file = "GameMessageSizeException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;

    /// <summary>
    /// Exception thrown when sending a game message that exceeds the maximum allowed size.
    /// </summary>
    [Serializable]
    public class GameMessageSizeException : Exception
    {
        private const string MessageFormat = "Serialized Game Message size of {0} exceeds maximum size of {1} bytes.";

        /// <summary>
        /// Create a new <see cref="GameMessageSizeException"/>.
        /// </summary>
        /// <param name="gameMessageSize">The size of the message in bytes.</param>
        /// <param name="maximumSize">The maximum allowed size in bytes.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="gameMessageSize"/> is less than <paramref name="maximumSize"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="maximumSize"/> is zero.</exception>
        public GameMessageSizeException(int gameMessageSize, int maximumSize)
            : base(string.Format(MessageFormat, gameMessageSize, maximumSize))
        {
            if(maximumSize == 0)
            {
                throw new ArgumentException("Maximum size cannot be 0.");
            }
            if(gameMessageSize <= maximumSize)
            {
                throw new ArgumentException(
                    $"Message size of {gameMessageSize} does not exceed specified max size of {maximumSize}.");
            }
        }
    }
}