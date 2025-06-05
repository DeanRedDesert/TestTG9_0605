//-----------------------------------------------------------------------
// <copyright file = "PortalContentStateOptions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using CSI.Schemas.Internal;

    /// <summary>
    /// Enumeration containing portal content state options.
    /// </summary>
    public enum PortalContentStateOptions
    {
        /// <summary>
        /// Option indicating an invalid portal state.
        /// </summary>
        InvalidPortalContentState,

        /// <summary>
        /// Option indicating the portal content has been released.
        /// </summary>
        Released,

        /// <summary>
        /// Option indicating the portal content has been loaded.
        /// </summary>
        Loaded,

        /// <summary>
        /// Option indicating the portal content is pending.
        /// </summary>
        Pending,

        /// <summary>
        /// Option indicating the portal content is executing.
        /// </summary>
        Executing,

        /// <summary>
        /// Option indicating there is an error in the portal content.
        /// </summary>
        Error
    }

    /// <summary>
    /// Internal class containing utility methods for <see cref="PortalContentStateOptions"/>.
    /// </summary>
    internal class PortalContent
    {
        /// <summary>
        /// Converts a <see cref="PortalContentStateOptions"/> (CSI) enumeration to a 
        /// <see cref="PortalContentState"/> enumeration (SDK).
        /// </summary>
        /// <param name="portalContentState">The CSI enumeration to convert.</param>
        /// <returns>The converted enumeration.</returns>
        public static PortalContentStateOptions ToPublic(PortalContentState portalContentState)
        {
            switch(portalContentState)
            {
                case PortalContentState.EXECUTING:
                    return PortalContentStateOptions.Executing;

                case PortalContentState.INVALID_PORTAL_CONTENT_STATE:
                    return PortalContentStateOptions.InvalidPortalContentState;

                case PortalContentState.LOADED:
                    return PortalContentStateOptions.Loaded;

                case PortalContentState.PENDING:
                    return PortalContentStateOptions.Pending;

                case PortalContentState.RELEASED:
                    return PortalContentStateOptions.Released;

                // Default case implies an error was encountered.
                default:
                    return PortalContentStateOptions.Error;
            }
        }
    }
}
