// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryStepKey.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// A key for a shell history step.
    /// </summary>
    internal readonly struct ShellHistoryStepKey
    {
        /// <summary>
        /// Gets the critical data name that should be used for this step key.
        /// </summary>
        public CriticalDataName CriticalDataName { get; }

        /// <summary>
        /// Initializes a new shell history step key.
        /// </summary>
        /// <param name="stepNumber">The step number for the history step.</param>
        internal ShellHistoryStepKey(int stepNumber)
        {
            if(stepNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepNumber), "Step number must be a positive integer.");
            }
            CriticalDataName = $"Shell/Step{stepNumber}";
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
