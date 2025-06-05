// -----------------------------------------------------------------------
// <copyright file = "CoplayerHistoryStepKey.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// A key for a coplayer history step.
    /// </summary>
    internal readonly struct CoplayerHistoryStepKey
    {
        /// <summary>
        /// Gets the critical data name that should be used for this step key.
        /// </summary>
        public CriticalDataName CriticalDataName { get; }

        /// <summary>
        /// Initializes a new coplayer history step key.
        /// </summary>
        /// <param name="stepNumber">The step number for the history step.</param>
        internal CoplayerHistoryStepKey(int stepNumber)
        {
            if(stepNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepNumber), "Step number must be a positive integer.");
            }
            CriticalDataName = $"Step{stepNumber}";
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return CriticalDataName;
        }

        #endregion
    }
}
