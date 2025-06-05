//-----------------------------------------------------------------------
// <copyright file = "GetPortalIdByNameErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Getting portal id by name errors.
    /// </summary>
    public enum GetPortalIdByNameError
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
        /// Error to notify client that the request has an invalid portal name.
        /// </summary>
        InvalidPortalName,

        /// <summary>
        /// Error to notify client that an invalid portal class was supplied.
        /// </summary>
        InvalidPortalClass
    }

    /// <summary>
    /// Specific exception for getting a portal id by name.
    /// </summary>
    public class GetPortalIdByNameErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly GetPortalIdByNameError Error;

        /// <summary>
        /// Create a new GetPortalIdByNameErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public GetPortalIdByNameErrorException(GetPortalIdByNameError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
