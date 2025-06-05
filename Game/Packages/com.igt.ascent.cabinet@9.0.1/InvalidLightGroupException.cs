//-----------------------------------------------------------------------
// <copyright file = "InvalidLightGroupException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Thrown when a group specified for a feature is not valid.
    /// </summary>
    [Serializable]
    public class InvalidLightGroupException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "The Feature: {0} does not contain the specified Group: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the device.</param>
        /// <param name="lightGroup">The group which was not valid for the device.</param>
        public InvalidLightGroupException(string featureId, byte lightGroup)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, featureId, lightGroup))
        {
        }
    }
}
