//-----------------------------------------------------------------------
// <copyright file = "ShakeLevel.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Enumeration which indicates the desired amount of motor shake.
    /// </summary>
    public enum ShakeLevel
    {
        /// <summary>
        /// No shaking.
        /// </summary>
        Off,

        /// <summary>
        /// Low level of shaking.
        /// </summary>
        Low,

        /// <summary>
        /// Medium level of shaking.
        /// </summary>
        Medium,

        /// <summary>
        /// High level of shaking.
        /// </summary>
        High,

        /// <summary>
        /// Maximum amount of shaking.
        /// </summary>
        Max
    }
}
