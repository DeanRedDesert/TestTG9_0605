//-----------------------------------------------------------------------
// <copyright file = "HidePortalErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Hide portal errors.
    /// </summary>
    public enum HidePortalError
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
        /// Error to notify client that the portal could not be hidden.
        /// </summary>
        CannotHidePortal,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exception for hiding portals.
    /// </summary>
    public class HidePortalErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly HidePortalError Error;

        /// <summary>
        /// Create a new HidePortalErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public HidePortalErrorException(HidePortalError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
