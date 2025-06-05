//-----------------------------------------------------------------------
// <copyright file = "InvalidSpeedIndexException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates an invalid speed index was used.
    /// </summary>
    [Serializable]
    public class InvalidSpeedIndexException : Exception
    {
        /// <summary>
        /// The invalid speed index that was used.
        /// </summary>
        public ushort SpeedIndex { private set; get; }

        /// <summary>
        /// The feature ID of the device being controlled.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageFormat = "Speed index: {0} invalid for device: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="speedIndex">The speed index which was invalid.</param>
        public InvalidSpeedIndexException(string featureId, ushort speedIndex)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, speedIndex, featureId))
        {
            SpeedIndex = speedIndex;
            FeatureId = featureId;
        }
    }
}
