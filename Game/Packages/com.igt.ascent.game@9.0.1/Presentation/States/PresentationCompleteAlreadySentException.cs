//-----------------------------------------------------------------------
// <copyright file = "PresentationCompleteAlreadySentException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception that is thrown when a presentation state tries to send presentation complete
    /// after one has already been sent.
    /// </summary>
    [Serializable]
    public class PresentationCompleteAlreadySentException : Exception
    {
        private const string MessageFormat = "State Name: \"{0}\" Reason: {1}";

        /// <summary>
        /// Initialize a new instance of PresentationCompleteAlreadySentException with a state name and message.
        /// </summary>
        /// <param name="stateName">Name of the state that caused the problem.</param>
        /// <param name="message">Message giving detail about the cause of this exception.</param>
        public PresentationCompleteAlreadySentException(string stateName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, stateName, message))
        {
            StateName = stateName;
        }

        /// <summary>
        /// Initialize a new instance of PresentationCompleteAlreadySentException with a state name, message,
        /// and exception that caused this one to be thrown.
        /// </summary>
        /// <param name="stateName">Name of the state that caused the problem.</param>
        /// <param name="message">Message giving detail about the problem.</param>
        /// <param name="innerException">The exception that caused this exception to be thrown.</param>
        public PresentationCompleteAlreadySentException(string stateName, string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, stateName, message), innerException)
        {
            StateName = stateName;
        }

        #region Properties
        /// <summary>
        /// Get the name of the state that caused this exception.
        /// </summary>
        public string StateName
        {
            get;
            private set;
        }
        #endregion
    }
}
