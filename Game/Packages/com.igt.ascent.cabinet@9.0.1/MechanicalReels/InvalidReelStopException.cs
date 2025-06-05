//-----------------------------------------------------------------------
// <copyright file = "InvalidReelStopException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates a request for an invalid reel stop.
    /// </summary>
    [Serializable]
    public class InvalidReelStopException : Exception
    {
        /// <summary>
        /// The reel number associated with the stop.
        /// </summary>
        public byte ReelNumber { private set; get; }

        /// <summary>
        /// The invalid reel stop.
        /// </summary>
        public byte ReelStop { private set; get; }

        /// <summary>
        /// The feature ID of the device the reel was in.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageFormat = "Invalid reel stop: {0} for reel: {1} in device: {2}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="reelNumber">The number of the reel.</param>
        /// <param name="reelStop">The stop which was invalid for the reel.</param>
        public InvalidReelStopException(string featureId, byte reelNumber, byte reelStop)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, reelStop, reelNumber, featureId))
        {
            ReelNumber = reelNumber;
            ReelStop = reelStop;
            FeatureId = featureId;
        }
    }
}
