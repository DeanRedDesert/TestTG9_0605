//-----------------------------------------------------------------------
// <copyright file = "DuplicateConsumerPropertyException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Services
{
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Exception thrown when there is a duplicate property exception.
    /// </summary>
    public class DuplicateConsumerPropertyException : System.Exception
    {
        /// <summary>
        /// Initialize an instance of DuplicateConsumerPropertyException.
        /// </summary>
        /// <param name="newErrorPropertyInfo">PropertyInfo that caused the exception.</param>
        /// <param name="value">Object that caused the exception</param>
        /// <param name="message">Reason for throwing this exception.</param>
        public DuplicateConsumerPropertyException(PropertyInfo newErrorPropertyInfo, object value, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                newErrorPropertyInfo.Name, value, message))
        {
            ErrorPropertyInfo = newErrorPropertyInfo;
            ErrorObject = value;
        }

        /// <summary>
        /// Initialize an instance of DuplicateConsumerPropertyException.
        /// </summary>
        /// <param name="newErrorPropertyInfo">PropertyInfo that caused the exception.</param>
        /// <param name="value">Object that caused the exception</param>
        /// <param name="message">Reason for throwing this exception.</param>
        /// <param name="innerException">Exception that caused this exception.</param>
        public DuplicateConsumerPropertyException(PropertyInfo newErrorPropertyInfo, object value,
            string message, System.Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                newErrorPropertyInfo.Name, value, message), innerException)
        {
            ErrorPropertyInfo = newErrorPropertyInfo;
            ErrorObject = value;
        }


        /// <summary>
        /// Object being accessed at point of exception
        /// </summary>
        public object ErrorObject { get; }

        /// <summary>
        /// Property Info being accessed at point of exception
        /// </summary>
        public PropertyInfo ErrorPropertyInfo { get; }

        private const string MessageFormat = "ErrorPropertyInfo: \"{0}\" ErrorObject: \"{1}\" Reason: {2}";
    }
}
