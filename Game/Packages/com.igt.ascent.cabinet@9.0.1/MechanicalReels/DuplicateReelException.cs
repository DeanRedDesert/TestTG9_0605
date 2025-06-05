//-----------------------------------------------------------------------
// <copyright file = "DuplicateReelException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates a command attempted to control a single reel multiple times.
    /// </summary>
    [Serializable]
    public class DuplicateReelException : Exception
    {
        /// <summary>
        /// The reel number which was a duplicate.
        /// </summary>
        public byte ReelNumber { private set; get; }

        /// <summary>
        /// The feature ID of the device the reel was in.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageFormat = "Duplicate commands for reel: {0} in device device: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="reelNumber">The number of the reel which was a duplicate.</param>
        public DuplicateReelException(string featureId, byte reelNumber)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, reelNumber, featureId))
        {
            ReelNumber = reelNumber;
            FeatureId = featureId;
        }
    }
}
