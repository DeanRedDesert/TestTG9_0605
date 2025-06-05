//-----------------------------------------------------------------------
// <copyright file = "PortalVisibilityStateOptions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using CSI.Schemas.Internal;

    /// <summary>
    /// Enumeration containing portal visibility state options.
    /// </summary>
    public enum PortalVisibilityStateOptions
    {
        /// <summary>
        /// Option indicating an invalid portal visibility state.
        /// </summary>
        InvalidPortalVisibilityState,

        /// <summary>
        /// Option indicating a hidden portal visibility state.
        /// </summary>
        Hidden,

        /// <summary>
        /// Option indicating a shown portal visibility state.
        /// </summary>
        Shown
    }

    /// <summary>
    /// Internal class containing utility methods for <see cref="PortalVisibilityStateOptions"/>.
    /// </summary>
    internal class PortalVisibility
    {
        /// <summary>
        /// Converts a <see cref="PortalVisibilityState"/> (CSI) enumeration to a 
        /// <see cref="PortalVisibilityStateOptions"/> enumeration (SDK).
        /// </summary>
        /// <param name="portalVisibilityState">The CSI enumeration to convert.</param>
        /// <returns>The converted enumeration.</returns>
        public static PortalVisibilityStateOptions ToPublic(PortalVisibilityState portalVisibilityState)
        {
            switch(portalVisibilityState)
            {
                case PortalVisibilityState.HIDDEN:
                    return PortalVisibilityStateOptions.Hidden;

                case PortalVisibilityState.SHOWN:
                    return PortalVisibilityStateOptions.Shown;

                // Default case implies an invalid portal state was encountered.
                default:
                    return PortalVisibilityStateOptions.InvalidPortalVisibilityState;
            }
        }
    }
}
