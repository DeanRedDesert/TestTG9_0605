//-----------------------------------------------------------------------
// <copyright file = "InvalidReelException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates an attempt was made to control an invalid reel.
    /// </summary>
    [Serializable]
    public class InvalidReelException : Exception
    {
        /// <summary>
        /// The reel number which was invalid.
        /// </summary>
        public byte ReelNumber { private set; get; }

        /// <summary>
        /// The feature ID of the device the reel was in.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageFormat = "Reel: {0} not valid for device: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="reelNumber">The number of the reel which was invalid.</param>
        public InvalidReelException(string featureId, byte reelNumber)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, reelNumber, featureId))
        {
            ReelNumber = reelNumber;
            FeatureId = featureId;
        }
    }
}
