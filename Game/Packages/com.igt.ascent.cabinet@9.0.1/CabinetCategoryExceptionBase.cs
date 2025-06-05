//-----------------------------------------------------------------------
// <copyright file = "CabinetCategoryExceptionBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Primary exception of error status for cabinet categories.
    /// </summary>
    [Serializable]
    public abstract class CabinetCategoryExceptionBase : Exception
    {
        /// <summary>
        /// Format for the exception message.
        /// </summary>
        protected const string MessageFormat = "Status Code: {0} Description: {1}";

        /// <summary>
        /// The error code received from the CSI Manager.
        /// </summary>
        public string ErrorCode { private set; get; }

        /// <summary>
        /// The error description received from the CSI Manager.
        /// </summary>
        public string ErrorDescription { private set; get; }

        /// <summary>
        /// Initialize a new instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code received.</param>
        /// <param name="description">A description of the error.</param>
        protected CabinetCategoryExceptionBase(string errorCode, string description)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, errorCode, description))
        {
            ErrorCode = errorCode;
            ErrorDescription = description;
        }
    }
}