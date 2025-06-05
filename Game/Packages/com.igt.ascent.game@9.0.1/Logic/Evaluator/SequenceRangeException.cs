//-----------------------------------------------------------------------
// <copyright file = "SequenceRangeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which is used to indicate that a number in a sequence did not satisfy the range requirements of
    /// the request for which it was pulled.
    /// </summary>
    public class SequenceRangeException : Exception
    {
        /// <summary>
        /// Minimum range of the request.
        /// </summary>
        public int MinRange { get; private set; }

        /// <summary>
        /// Maximum range of the request.
        /// </summary>
        public int MaxRange { get; private set; }

        /// <summary>
        /// Value pulled from the sequence.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Format for the exceptions message.
        /// </summary>
        private const string MessageFormat =
            "Sequenced value does not meet range requirements for request. MinRange: {0} MaxRange: {1} Value: {2}";

        /// <summary>
        /// Create a SequenceRangeException.
        /// </summary>
        /// <param name="minRange">The minimum range of the request.</param>
        /// <param name="maxRange">The maximum range of the request. </param>
        /// <param name="value">The value pulled from the sequence.</param>
        public SequenceRangeException(int minRange, int maxRange, int value)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, minRange, maxRange, value))
        {
            MinRange = minRange;
            MaxRange = maxRange;
            Value = value;
        }
    }
}
