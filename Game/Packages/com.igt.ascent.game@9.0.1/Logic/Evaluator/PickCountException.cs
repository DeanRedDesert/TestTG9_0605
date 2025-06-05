//-----------------------------------------------------------------------
// <copyright file = "PickCountException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that a requested pick count could not be safisfied without duplicates.
    /// </summary>
    public class PickCountException : EvaluationException
    {
        /// <summary>
        /// String to use to format the exception message for the base class.
        /// </summary>
        private const string MessageFormat = "There are not enough picks to satisfy the request without duplicates. Requested picks: {0} Picks available: {1}";

        /// <summary>
        /// The requested number of picks that caused the exception.
        /// </summary>
        public uint RequestedCount { set; get; }

        /// <summary>
        /// The total number of picks which where available.
        /// </summary>
        public int TotalPicks { set; get; }

        /// <summary>
        /// Create a PickCountException.
        /// </summary>
        /// <param name="requestedCount">The requested number of picks.</param>
        /// <param name="totalPicks">The total picks available.</param>
        public PickCountException(uint requestedCount, int totalPicks)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, requestedCount, totalPicks))
        {
            RequestedCount = requestedCount;
            TotalPicks = totalPicks;
        }
    }
}
