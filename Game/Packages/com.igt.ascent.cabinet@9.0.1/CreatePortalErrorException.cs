//-----------------------------------------------------------------------
// <copyright file = "CreatePortalErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Create portal errors.
    /// </summary>
    public enum CreatePortalError
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
        /// Error to notify client that the portal already exists.
        /// </summary>
        PortalExists,

        /// <summary>
        /// Error to notify client that the portal is out of bounds.
        /// </summary>
        PortalOutOfBounds,

        /// <summary>
        /// Error to notify client that the portal has an invalid priority.
        /// </summary>
        InvalidPriority,

        /// <summary>
        /// Error to notify client that the request has an invalid monitor.
        /// </summary>
        InvalidMonitor,

        /// <summary>
        /// Error to notify client that the request has an invalid portal name.
        /// </summary>
        InvalidPortalName,

        /// <summary>
        /// Error to notify client that an invalid portal class was supplied.
        /// </summary>
        InvalidPortalClass,

        /// <summary>
        /// Error to notify the client that an invalid default EMDI token was supplied.
        /// </summary>
        InvalidDefaultEMDIToken,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exception for creating portals.
    /// </summary>
    public class CreatePortalErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly CreatePortalError Error;

        /// <summary>
        /// Create a new CreatePortalErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public CreatePortalErrorException(CreatePortalError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
