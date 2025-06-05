// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceReportContext.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;

    /// <summary>
    /// This class contains the context information for game performance report.
    /// </summary>
    public class GamePerformanceReportContext : IEquatable<GamePerformanceReportContext>
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
        /// Initializes a new instance of <see cref="GamePerformanceReportContext"/>.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <param name="paytableIdentifier">The paytable identifier.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> or <paramref name="paytableIdentifier"/>
        /// is null.
        /// </exception>
        public GamePerformanceReportContext(string themeIdentifier, string paytableIdentifier)
        {
            ThemeIdentifier = themeIdentifier ?? throw new ArgumentNullException(nameof(themeIdentifier));
            PaytableIdentifier = paytableIdentifier ?? throw new ArgumentNullException(nameof(paytableIdentifier));
        }

        #region IEquatable<GamePerformanceReportContext> Members

        /// <inheritdoc />
        public bool Equals(GamePerformanceReportContext other)
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
            return Equals(obj as GamePerformanceReportContext);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = hash * 37 + ThemeIdentifier.GetHashCode();
            hash = hash * 37 + PaytableIdentifier.GetHashCode();

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
        public static bool operator ==(GamePerformanceReportContext left, GamePerformanceReportContext right)
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
                   left.PaytableIdentifier == right.PaytableIdentifier;
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
        public static bool operator !=(GamePerformanceReportContext left, GamePerformanceReportContext right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Convert <see cref="GamePerformanceReportContext"/> to a string.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Theme Identifier({ThemeIdentifier})/Paytable Identifier({PaytableIdentifier})";
        }
    }
}
