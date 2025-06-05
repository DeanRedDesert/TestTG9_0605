//-----------------------------------------------------------------------
// <copyright file = "InvalidChangeSpeedProfileException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates an invalid change speed profile was used.
    /// </summary>
    [Serializable]
    public class InvalidChangeSpeedProfileException : Exception
    {
        /// <summary>
        /// The feature ID of the device the reel was in.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// The reel number which was invalid.
        /// </summary>
        public byte ReelNumber { private set; get; }

        /// <summary>
        /// The direction specified.
        /// </summary>
        public ReelDirection ReelDirection { private set; get; }

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageFormat = "Reel #: {0} on feature: {1} can not have its direction set to: {2}.";

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageInvalidOrEmpty = "The collection of specified reels is null or empty.";

        /// <summary>
        /// Construct an instance of the exception if the collection of specified reels is null or empty.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        public InvalidChangeSpeedProfileException(string featureId) : base(MessageInvalidOrEmpty)
        {
            FeatureId = featureId;
        }

        /// <summary>
        /// Construct an overloaded instance of the exception containing invalid direction and reel information.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="reelNumber">The number of the reel which was invalid.</param>
        /// <param name="reelDirection">The invalid <see cref="ReelDirection"/> specified.</param>
        public InvalidChangeSpeedProfileException(string featureId, byte reelNumber, ReelDirection reelDirection)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, reelNumber, featureId, reelDirection))
        {
            ReelNumber = reelNumber;
            FeatureId = featureId;
            ReelDirection = reelDirection;
        }
    }
}
