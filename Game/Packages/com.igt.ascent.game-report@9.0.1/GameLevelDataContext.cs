// -----------------------------------------------------------------------
// <copyright file = "GameLevelDataContext.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;

    /// <summary>
    /// Class indicating the theme-paytables-denominations information.
    /// </summary>
    public class GameLevelDataContext : IEquatable<GameLevelDataContext>
    {
        /// <summary>
        /// Gets the identifier of a game theme.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Gets the paytable identifier of a game theme identified by <see cref="ThemeIdentifier"/>.
        /// </summary>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the denomination of a paytable identified by <see cref="PaytableIdentifier"/>.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GameLevelDataContext"/>.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <param name="paytableIdentifier">The paytable identifier.</param>
        /// <param name="denomination">The denomination.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> or <paramref name="paytableIdentifier"/>
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="denomination"/> is less than or equals to zero.
        /// </exception>
        public GameLevelDataContext(string themeIdentifier, string paytableIdentifier, long denomination)
        {
            if(denomination <= 0)
            {
                throw new ArgumentException("The denomination must be greater than zero", nameof(denomination));
            }

            ThemeIdentifier = themeIdentifier ?? throw new ArgumentNullException(nameof(themeIdentifier));
            PaytableIdentifier = paytableIdentifier ?? throw new ArgumentNullException(nameof(paytableIdentifier));
            Denomination = denomination;
        }

        #region IEquatable<GameLevelDataContext> Members

        /// <inheritdoc />
        public bool Equals(GameLevelDataContext other)
        {
            return this == other;
        }

        #endregion

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check</param>
        /// <returns>
        /// True if the right hand object equals to this object.
        /// False otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as GameLevelDataContext);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = hash * 37 + ThemeIdentifier.GetHashCode();
            hash = hash * 37 + PaytableIdentifier.GetHashCode();
            hash = hash * 37 + Denomination.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>
        /// True if two operands are considered equal.
        /// False otherwise.
        /// </returns>
        public static bool operator ==(GameLevelDataContext left, GameLevelDataContext right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if(ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.ThemeIdentifier == right.ThemeIdentifier &&
                   left.PaytableIdentifier == right.PaytableIdentifier &&
                   left.Denomination == right.Denomination;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>
        /// True if two operands are not considered equal.
        /// False otherwise.
        /// </returns>
        public static bool operator !=(GameLevelDataContext left, GameLevelDataContext right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Convert <see cref="GameLevelDataContext"/> to a string.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $"Theme Identifier({ThemeIdentifier})/Paytable Identifier({PaytableIdentifier})/Denomination({Denomination})";
        }
    }
}