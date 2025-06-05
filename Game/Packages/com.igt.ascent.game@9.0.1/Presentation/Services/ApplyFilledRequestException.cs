//-----------------------------------------------------------------------
// <copyright file = "ApplyFilledRequestException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Services
{
    using System.Globalization;
    /// <summary>
    /// Exception to be thrown when an exception is caught while applying a filled request.
    /// </summary>
    public class ApplyFilledRequestException : System.Exception
    {
        /// <summary>
        /// Initialize an instance of ServicePropertyException.
        /// </summary>
        /// <param name="newErrorProvider">Service Provider name that caused the error.</param>
        /// <param name="message">Reason for throwing this exception.</param>
        public ApplyFilledRequestException(string newErrorProvider, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                                 newErrorProvider, message))
        {
            ErrorProvider = newErrorProvider;
        }

        /// <summary>
        /// Initialize an instance of ServicePropertyException.
        /// </summary>
        /// <param name="newErrorProvider">Service Provider name that caused the error.</param>
        /// <param name="message">Reason for throwing this exception.</param>
        /// <param name="innerException">Exception that caused this exception.</param>
        public ApplyFilledRequestException(string newErrorProvider, string message, System.Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                                 newErrorProvider, message), innerException)
        {
            ErrorProvider = newErrorProvider;
        }

        /// <summary>
        /// serviceProvider name that caused the error.
        /// </summary>
        public string ErrorProvider { get; }

        private const string MessageFormat = "{1}  Provider Causing the Error: \"{0}\" ";
    }
}