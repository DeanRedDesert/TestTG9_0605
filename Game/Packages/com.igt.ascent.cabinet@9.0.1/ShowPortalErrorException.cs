//-----------------------------------------------------------------------
// <copyright file = "ShowPortalErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Show portal errors.
    /// </summary>
    public enum ShowPortalError
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
        /// Error to notify client that the portal can't be shown.
        /// </summary>
        CantShowPortal,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exceptions for showing a portal.
    /// </summary>
    public class ShowPortalErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly ShowPortalError Error;

        /// <summary>
        /// Create a new ShowPortalErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public ShowPortalErrorException(ShowPortalError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
