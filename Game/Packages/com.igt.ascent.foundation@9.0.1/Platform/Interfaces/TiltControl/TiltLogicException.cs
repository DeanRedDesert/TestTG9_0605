//-----------------------------------------------------------------------
// <copyright file = "TiltLogicException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces.TiltControl
{
    using System;
    using System.Globalization;

    ///<summary>
    /// Used to signal an error with tilt logic such as posting a tilt that has already been posted.
    ///</summary>
    [Serializable]
    public class TiltLogicException : Exception
    {
        private const string ExceptionFormat = "Tilt logic error.  Tilt key: '{0}' Reason: {1}";

        /// <summary>
        /// Create a TiltLogicException with relevant information.
        /// </summary>
        /// <param name="tiltKey">The tilt key that was being operated on.</param>
        /// <param name="reason">The reason for the logic error.</param>
        public TiltLogicException(string tiltKey, string reason)
            : base(string.Format(CultureInfo.InvariantCulture, ExceptionFormat, tiltKey, reason))
        {
        }
    }
}