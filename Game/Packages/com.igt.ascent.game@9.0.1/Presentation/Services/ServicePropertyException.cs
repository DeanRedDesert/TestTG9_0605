//-----------------------------------------------------------------------
// <copyright file = "ServicePropertyException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Services
{
    using System.Reflection;
    using System.Globalization;

    /// <summary>
    /// Exception to be thrown when an exception is caught while updating service properties.
    /// </summary>
    public class ServicePropertyException : System.Exception
    {
        /// <summary>
        /// Initialize an instance of ServicePropertyException.
        /// </summary>
        /// <param name="newErrorPropertyInfo">PropertyInfo that caused the Exception</param>
        /// <param name="value">Object that caused the Exception</param>
        /// <param name="message">Reason for throwing this exception.</param>
        public ServicePropertyException(PropertyInfo newErrorPropertyInfo, object value, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                newErrorPropertyInfo.Name, value.ToString(), message))
        {
            ErrorPropertyInfo = newErrorPropertyInfo;
            ErrorObject = value;
        }


        /// <summary>
        /// Initialize an instance of ServicePropertyException.
        /// </summary>
        /// <param name="newErrorPropertyInfo">PropertyInfo that caused the Exception</param>
        /// <param name="value">Object that caused the Exception</param>
        /// <param name="message">Reason for throwing this exception.</param>
        /// <param name="innerException">Exception that caused this exception.</param>
        public ServicePropertyException(PropertyInfo newErrorPropertyInfo, object value, string message,
            System.Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                newErrorPropertyInfo.Name, value.ToString(), message), innerException)
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
