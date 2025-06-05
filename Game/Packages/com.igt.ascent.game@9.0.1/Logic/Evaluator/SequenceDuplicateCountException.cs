//-----------------------------------------------------------------------
// <copyright file = "SequenceDuplicateCountException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which is used to indicate that the number of duplicates for a request has been exceeded when
    /// drawing numbers from a sequence.
    /// </summary>
    public class SequenceDuplicateCountException : Exception
    {
        /// <summary>
        /// Maximum number of duplicates allowed.
        /// </summary>
        public uint MaxDuplicates { get; private set; }

        /// <summary>
        /// Value pulled from the sequence.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Format for the exceptions message.
        /// </summary>
        private const string MessageFormat =
            "Values in sequence exceed duplicate count specified by request. MaxDuplicates: {0} Value: {1}";

        /// <summary>
        /// Create a SequenceDuplicateCountException.
        /// </summary>

        public SequenceDuplicateCountException(uint maxDuplicates, int value)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, maxDuplicates, value))
        {
            MaxDuplicates = maxDuplicates;
            Value = value;
        }
    }
}
