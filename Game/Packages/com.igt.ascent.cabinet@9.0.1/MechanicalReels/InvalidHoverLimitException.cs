//-----------------------------------------------------------------------
// <copyright file = "InvalidHoverLimitException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates a request with an invalid hover limit.
    /// </summary>
    [Serializable]
    public class InvalidHoverLimitException : Exception
    {
        /// <summary>
        /// The feature ID of the device the reel was in.
        /// </summary>
        public string FeatureId { get; private set; }

        /// <summary>
        /// The reel number associated with the error.
        /// </summary>
        public byte ReelNumber { get; private set; }

        /// <summary>
        /// The number of stops available on the reel.
        /// </summary>
        public byte NumberOfStops { get; private set; }

        /// <summary>
        /// The requested hover limits.
        /// </summary>
        public HoverLimits Limits { get; private set; }

        private const string MessageFormat = "Invalid hover ({0}) for reel: {1} with {2} stops, in device: {3}";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="featureId">The feature ID of the device the reel was in.</param>
        /// <param name="reelNumber">The reel number associated with the error.</param>
        /// <param name="numberOfStops">The number of stops available on the reel.</param>
        /// <param name="limits">The requested hover limit.</param>
        public InvalidHoverLimitException(string featureId, byte reelNumber, byte numberOfStops, HoverLimits limits)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, limits, reelNumber,
                numberOfStops, featureId))
        {
            FeatureId = featureId;
            ReelNumber = reelNumber;
            NumberOfStops = numberOfStops;
            Limits = limits;
        }
    }
}
