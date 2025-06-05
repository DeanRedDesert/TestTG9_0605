//-----------------------------------------------------------------------
// <copyright file = "ConfigureVisibilityGroupErrorException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Configure visibility group errors.
    /// </summary>
    public enum ConfigureVisibilityGroupError
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
        /// Error to notify client of invalid portal id.
        /// </summary>
        InvalidPortalId,

        /// <summary>
        /// Error to notify client that there's too many portals in the request.
        /// </summary>
        TooManyPortals,

        /// <summary>
        /// Error to notify client of an invalid group name.
        /// </summary>
        InvalidGroupName,

        /// <summary>
        /// Error to notify client that not all of the portals are in the same class. 
        /// </summary>
        PortalsNotAllSameClass,

        /// <summary>
        /// Error to notify client that they do not own the portal.
        /// </summary>
        ClientDoesNotOwnResource
    }

    /// <summary>
    /// Specific exception for configuring visibility groups.
    /// </summary>
    public class ConfigureVisibilityGroupErrorException : PortalCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public readonly ConfigureVisibilityGroupError Error;

        /// <summary>
        /// Create a new ConfigureVisibilityGroupErrorException.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public ConfigureVisibilityGroupErrorException(ConfigureVisibilityGroupError error, 
            string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
