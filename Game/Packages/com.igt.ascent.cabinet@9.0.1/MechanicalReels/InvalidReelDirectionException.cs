//-----------------------------------------------------------------------
// <copyright file = "InvalidReelDirectionException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates a reel direction used in a command
    /// is not allowed for that command.
    /// </summary>
    [Serializable]
    public class InvalidReelDirectionException : Exception
    {
        /// <summary>
        /// The direction incorrectly used in the command.
        /// </summary>
        public ReelDirection Direction { private set; get; }

        /// <summary>
        /// The reel number that has an invalid direction set.
        /// </summary>
        public byte Number { private set; get; }

        /// <summary>
        /// The feature ID of the device being controlled.
        /// </summary>
        public string FeatureId { private set; get; }

        /// <summary>
        /// Format to use for the invalid direction parms. exception string.
        /// </summary>
        private const string MessageFormatInvalidParms = "The reel direction of '{0}' is not allowed for this request (reel number: {1}).";

        /// <summary>
        /// Format to use for the null or empty exception string.
        /// </summary>
        private const string MessageNullOrEmpty = "The specified collection of reel directions is null or empty.";

        /// <summary>
        /// Initialize an instance of <see cref="InvalidReelDirectionException"/>.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        public InvalidReelDirectionException(string featureId) : base(MessageNullOrEmpty)
        {
            FeatureId = featureId;
        }

        /// <summary>
        /// Initialize an instance of <see cref="InvalidReelDirectionException"/>.
        /// </summary>
        /// <param name="featureId">The feature ID of the reel device.</param>
        /// <param name="number">The reel number that has specified an invalid reel direction.</param>
        /// <param name="direction">The direction that is incorrectly used.</param>
        public InvalidReelDirectionException(string featureId, byte number, ReelDirection direction)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormatInvalidParms, direction, number))
        {
            FeatureId = featureId;
            Number = number;
            Direction = direction;
        }
    }
}