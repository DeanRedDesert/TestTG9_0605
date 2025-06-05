//-----------------------------------------------------------------------
// <copyright file = "WinOutcomePackage.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using Schemas;

    /// <summary>
    /// Class to encompass all the values related to a win outcome.
    /// Currently used to combine all the values related to a win outcome to create feature entries.
    /// </summary>
    [Serializable]
    public class WinOutcomePackage
    {
        /// <summary>
        /// Win outcome.
        /// </summary>
        public WinOutcome WinOutcome { get; set; }

        /// <summary>
        /// The feature index corresponding to <see cref="WinOutcome"/>.
        /// </summary>
        public uint FeatureIndex { get; set; }

        /// <summary>
        /// The random numbers associated with the <see cref="WinOutcome"/>.
        /// </summary>
        public IList<int> RandomNumbers { get; set; }

        /// <summary>
        /// Progressive levels corresponding to the <see cref="WinOutcome"/>.
        /// </summary>
        public ProgressiveLevels ProgressiveLevels { get; set; }

        /// <summary>
        /// Constructor to create a win outcome package.
        /// </summary>
        /// <param name="winOutcome">The win outcome this package is based on.</param>
        /// <param name="featureIndex">The feature index corresponding to <paramref name="winOutcome"/>.</param>
        /// <param name="randomNumbers">The random numbers associated with the <paramref name="winOutcome"/>.</param>
        /// <param name="progressiveLevels">Progressive levels corresponding to <paramref name="winOutcome"/>.</param>
        public WinOutcomePackage(WinOutcome winOutcome, uint featureIndex, IList<int> randomNumbers, ProgressiveLevels progressiveLevels)
        {
            WinOutcome = winOutcome;
            FeatureIndex = featureIndex;
            RandomNumbers = randomNumbers;
            ProgressiveLevels = progressiveLevels;
        }
    }
}
