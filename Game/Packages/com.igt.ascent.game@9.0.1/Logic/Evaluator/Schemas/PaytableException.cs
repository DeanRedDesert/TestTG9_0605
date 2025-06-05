//-----------------------------------------------------------------------
// <copyright file = "PaytableException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that there is an error in the paytable data.
    /// </summary>
    public class PaytableException : Exception
    {
        /// <summary>
        /// Paytable element which is the source of the error.
        /// </summary>
        public string PaytableElement { private set; get; }

        /// <summary>
        /// The problem with the paytable element.
        /// </summary>
        public string Error { private set; get; }

        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "Error in paytable element: {0} Error: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="element">
        /// A <see cref="System.String"/> containing the name of the offending paytable element.
        /// </param>
        /// <param name="error">
        /// A <see cref="System.String"/> containing a description of the problem with the element.
        /// </param>
        public PaytableException(string element, string error) : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, element, error))
        {
            PaytableElement = element;
            Error = error;
        }
    }
}

