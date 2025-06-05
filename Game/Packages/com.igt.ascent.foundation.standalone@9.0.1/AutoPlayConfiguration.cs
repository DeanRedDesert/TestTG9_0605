//-----------------------------------------------------------------------
// <copyright file = "AutoPlayConfiguration.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// The AutoPlayConfiguration enumeration is used to represent the configuration of auto play.
    /// </summary>
    /// <remarks> TODO: This may do change when foundation give the support.</remarks>>
    [Serializable]
    public enum AutoPlayConfiguration
    {
        /// <summary>
        /// It indicates auto play is not available.
        /// </summary>
        NotAvailable,

        /// <summary>
        /// It indicates auto play can be initiated by player.
        /// </summary>
        PlayerInitiatedAvailable,

        /// <summary>
        /// It indicates auto play is initiated by host.
        /// </summary>
        HostInitiatedAvailable
    }
}
