//-----------------------------------------------------------------------
// <copyright file = "EnvironmentAttribute.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// The Environment Attribute enumeration is used to represent
    /// the attributes of the environment in which the gaming system
    /// runs.
    /// </summary>
    [Serializable]
    public enum EnvironmentAttribute
    {
        /// <summary>
        /// This attribute indicates an environment where a distinction
        /// is made between player credits that may be wagered and those
        /// that may not.
        /// </summary>
        BankedCredits,

        /// <summary>
        /// This attribute indicates a Central Determination environment.
        /// </summary>
        Cds,

        /// <summary>
        /// This attribute indicates a demonstration and/or show environment.
        /// </summary>
        ShowDemo,
    }
}
