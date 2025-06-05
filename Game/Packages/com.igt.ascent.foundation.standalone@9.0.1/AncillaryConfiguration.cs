//-----------------------------------------------------------------------
// <copyright file = "AncillaryConfiguration.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    /// <summary>
    /// Class that provides system configuration for ancillary games
    /// </summary>
    public class AncillaryConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AncillaryConfiguration(bool supported, long cycleLimit, long monetaryLimit)
        {
            AncillarySupported = supported;
            AncillaryCycleLimit = cycleLimit;
            AncillaryMonetaryLimit = monetaryLimit;
        }

        /// <summary>
        /// Indicate if Ancillary game is allowed machine wide.
        /// </summary>
        public bool AncillarySupported { get; }

        /// <summary>
        /// Ancillary game Cycle permitted in a single ancillary play
        /// </summary>
        public long AncillaryCycleLimit { get; }

        /// <summary>
        /// Ancillary game max win amount allowed in a single ancillary play
        /// </summary>
        public long AncillaryMonetaryLimit { get; }
    }
}