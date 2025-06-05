//-----------------------------------------------------------------------
// <copyright file = "InvalidLightStateSizeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Thrown when packed light data does not match the expected size.
    /// </summary>
    [Serializable]
    public class InvalidLightStateSizeException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat =
            "Packed light control data for {0}/{1} did not match the expected size. Expected: {2} Actual: {3}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the device.</param>
        /// <param name="lightGroup">The group which was not valid for the device.</param>
        /// <param name="expectedSize">
        /// The expected size of the packed data based on the bits per light and the number of lights being controlled.
        /// </param>
        /// <param name="actualSize">The size of the packed data given to the bitwise control.</param>
        public InvalidLightStateSizeException(string featureId, byte lightGroup, int expectedSize, int actualSize)
            : base(
                string.Format(CultureInfo.InvariantCulture, MessageFormat, featureId, lightGroup, expectedSize,
                              actualSize))
        {
        }
    }
}
