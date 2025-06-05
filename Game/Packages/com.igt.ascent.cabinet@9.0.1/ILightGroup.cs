//-----------------------------------------------------------------------
// <copyright file = "ILightGroup.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Interface for light groups.
    /// </summary>
    public interface ILightGroup
    {
        /// <summary>
        /// Number which is used to identify this group.
        /// </summary>
        byte GroupNumber { get; }

        /// <summary>
        /// The number of lights in the group.
        /// </summary>
        ushort Count { get; }

        /// <summary>
        /// Flag indicating if the lights support RGB color.
        /// </summary>
        bool IsRgb { get; }
    }
}
