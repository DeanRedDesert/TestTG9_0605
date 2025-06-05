//-----------------------------------------------------------------------
// <copyright file = "UnknownPresentationStateException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Globalization;

    /// <summary>
    ///    Exception that should be thrown when a consumer is trying to access a presentation state
    ///    that is not registered with the StateManager.
    /// </summary>
    public class UnknownPresentationStateException : Exception
    {
        private const string MessageFormat = "State Name: \"{0}\" Reason: {1}";

        /// <summary>
        ///    Initialize a new instance of UnknownPresentationStateException.
        /// </summary>
        /// <param name = "stateName">Name of the state that caused the problem.</param>
        /// <param name = "message">Message giving detail about the problem.</param>
        public UnknownPresentationStateException(string stateName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, stateName, message))
        {
            StateName = stateName;
        }

        /// <summary>
        ///    Initialize a new instance of DuplicatePresentationStatesException.
        /// </summary>
        /// <param name = "stateName">Name of the state that caused the problem.</param>
        /// <param name = "message">Message giving detail about the problem.</param>
        /// <param name = "innerException">The exception that caused this one to be thrown.</param>
        public UnknownPresentationStateException(string stateName, string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, stateName, message), innerException)
        {
            StateName = stateName;
        }

        #region Properties

        /// <summary>
        ///    Get the name of the state that caused this exception.
        /// </summary>
        public string StateName
        {
            get;
        }

        #endregion
    }
}
