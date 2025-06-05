//-----------------------------------------------------------------------
// <copyright file = "InvalidDecelerationIndexException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates an invalid deceleration index was used.
    /// </summary>
    [Serializable]
    public class InvalidDecelerationIndexException : Exception
    {
        /// <summary>
        /// The invalid deceleration index that was used.
        /// </summary>
        public ushort DecelerationIndex { private set; get; }

        /// <summary>
        /// The feature ID of the device being controlled.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// Format to use for the exception string.
        /// </summary>
        private const string MessageFormat = "Deceleration index: {0} invalid for device: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="decelerationIndex">The deceleration index which was invalid.</param>
        public InvalidDecelerationIndexException(string featureId, ushort decelerationIndex)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, decelerationIndex, featureId))
        {
            DecelerationIndex = decelerationIndex;
            FeatureId = featureId;
        }
    }
}
