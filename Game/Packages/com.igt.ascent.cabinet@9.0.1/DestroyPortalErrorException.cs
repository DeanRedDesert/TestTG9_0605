//-----------------------------------------------------------------------
// <copyright file = "DestroyPortalErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Destroy portal errors.
    /// </summary>
    public enum DestroyPortalError
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
        InvalidPortalId,

        /// <summary>
        /// Error to notify client that the portal could not be destroyed.
        /// </summary>
        CannotDestroyPortal,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exception for destroying portals.
    /// </summary>
    public class DestroyPortalErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly DestroyPortalError Error;

        /// <summary>
        /// Create a new DestroyPortalErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public DestroyPortalErrorException(DestroyPortalError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
