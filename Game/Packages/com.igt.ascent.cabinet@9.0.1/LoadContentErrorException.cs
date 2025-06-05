//-----------------------------------------------------------------------
// <copyright file = "LoadContentErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Load content errors.
    /// </summary>
    public enum LoadContentError
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
        /// Error to notify client that a malformed URL was sent.
        /// </summary>
        MalformedUrl,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exception for loading content.
    /// </summary>
    public class LoadContentErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly LoadContentError Error;

        /// <summary>
        /// Create a new LoadContentErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public LoadContentErrorException(LoadContentError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
