//-----------------------------------------------------------------------
// <copyright file = "PortalListErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Portal list errors.
    /// </summary>
    public enum PortalListError
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
        /// Error to notify client that an invalid portal class was sent.
        /// </summary>
        InvalidPortalClass
    }

    /// <summary>
    /// Specific exception for getting a list of portals.
    /// </summary>
    public class PortalListErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly PortalListError Error;

        /// <summary>
        /// Create a new PortalListErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public PortalListErrorException(PortalListError error,
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
