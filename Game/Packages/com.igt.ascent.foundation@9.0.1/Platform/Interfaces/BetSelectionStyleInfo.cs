//-----------------------------------------------------------------------
// <copyright file = "BetSelectionStyleInfo.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This class represents data structure of bet selection style.
    /// </summary>
    public class BetSelectionStyleInfo
    {
        /// <summary>
        /// Gets the selection behavior of number of subsets.
        /// </summary>
        public BetSelectionBehavior NumberOfSubsets { get; }

        /// <summary>
        /// Gets the selection behavior of bet per subset.
        /// </summary>
        public BetSelectionBehavior BetPerSubset { get; }

        /// <summary>
        /// Gets the selection behavior of side bet.
        /// </summary>
        public SideBetSelectionBehavior SideBet { get; }

        /// <summary>
        /// Default constructor to initialize all the properties being undefined.
        /// </summary>
        public BetSelectionStyleInfo()
        {
            NumberOfSubsets = BetSelectionBehavior.Undefined;
            BetPerSubset = BetSelectionBehavior.Undefined;
            SideBet = SideBetSelectionBehavior.Undefined;
        }

        /// <summary>
        /// Initializes an instance of <see cref="BetSelectionStyleInfo"/>.
        /// </summary>
        /// <param name="numberOfSubsets">
        /// To initialize the selection behavior of number of subsets.
        /// </param>
        /// <param name="betPerSubset">
        /// To initialize the selection behavior of bet per subset.
        /// </param>
        /// <param name="sideBet">
        /// To initialize the selection behavior of side bet.
        /// </param>
        public BetSelectionStyleInfo(BetSelectionBehavior numberOfSubsets,
                                     BetSelectionBehavior betPerSubset,
                                     SideBetSelectionBehavior sideBet)
        {
            NumberOfSubsets = numberOfSubsets;
            BetPerSubset = betPerSubset;
            SideBet = sideBet;
        }
    }
}
