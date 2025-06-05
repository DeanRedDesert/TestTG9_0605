//-----------------------------------------------------------------------
// <copyright file = "PortalInfoErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Portal info errors.
    /// </summary>
    public enum PortalInfoError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None,

        /// <summary>
        /// Some undefined error occured.
        /// </summary>
        OtherError,

        /// <summary>
        /// Error to notify client that an invalid portal id was sent.
        /// </summary>
        InvalidPortalId
    }

    /// <summary>
    /// Specific exception for portal info errors.
    /// </summary>
    public class PortalInfoErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly PortalInfoError Error;

        /// <summary>
        /// Create a new PortalInfoErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public PortalInfoErrorException(PortalInfoError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
