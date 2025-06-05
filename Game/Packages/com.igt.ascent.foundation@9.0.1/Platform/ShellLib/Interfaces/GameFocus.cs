// -----------------------------------------------------------------------
// <copyright file = "GameFocus.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// The class defines a game focus.
    /// A game (or a game cycle) is in focus when it reaches the top of an internal work queue
    /// maintained by the Foundation, and is being actively worked on by the Foundation, such
    /// as serializing the game cycle etc.
    /// </summary>
    /// <remarks>
    /// For now, GameFocus does not concern games yet.
    /// </remarks>
    [Serializable]
    public class GameFocus
    {
        #region Properties

        /// <summary>
        /// Gets the ID of the coplayer in focus.
        /// </summary>
        public int CoplayerId { get; private set; }

        /// <summary>
        /// Gets the denomination of the game in focus.
        /// </summary>
        public long Denomination { get; private set; }

        /// <summary>
        /// Gets the theme identifier of the game in focus.
        /// </summary>
        public IdentifierToken ThemeIdentifier { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="GameFocus"/>
        /// </summary>
        /// <param name="coplayerId">The ID of the coplayer in focus.</param>
        /// <param name="themeIdentifier">The theme identifier of the game in focus .</param>
        /// <param name="denomination">The denomination of the game in focus.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="denomination"/> is less than 1.
        /// </exception>
        public GameFocus(int coplayerId, IdentifierToken themeIdentifier, long denomination)
        {
            if(themeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            if(denomination < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination can not be less than 1.");
            }

            CoplayerId = coplayerId;
            ThemeIdentifier = themeIdentifier;
            Denomination = denomination;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("GameFocus: CoplayerId({0}) / ThemeIdentifier Hash({1}) / Denomination ({2})",
                                 CoplayerId, ThemeIdentifier.GetHashCode(), Denomination);
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion
    }
}