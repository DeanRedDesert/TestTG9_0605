//-----------------------------------------------------------------------
// <copyright file = "WinOutcomeItemException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates any error associated with the WinOutcomeItem.
    /// </summary>
    [Serializable]
    public class WinOutcomeItemException : Exception
    {
        private const string MessageFormat = "{0} : {1}";

        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="parameter">The parameter with the exception.</param>
        /// <param name="messageDetails">Message associated with the error.</param>
        public WinOutcomeItemException(string parameter, string messageDetails)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, parameter, messageDetails))
        {
        }
    }
}
