//-----------------------------------------------------------------------
// <copyright file = "LightBezelGroup.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Light group for the Light Bezel sub feature.
    /// </summary>
    public class LightBezelGroup : LightGroup
    {
        /// <summary>
        /// Gets the number of lights in the top row.
        /// </summary>
        public byte TopRowLights { get; }

        /// <summary>
        /// Gets the number of lights in the bottom row.
        /// </summary>
        public byte BottomRowLights { get; }
        
        /// <summary>
        /// Gets the number of lights in the left column.
        /// </summary>
        public byte LeftColumnLights { get; }

        /// <summary>
        /// Gets the number of lights in the right column.
        /// </summary>
        public byte RightColumnLights { get; }

        /// <summary>
        /// Construct an instance of the light group.
        /// </summary>
        /// <param name="group">The number for the group.</param>
        /// <param name="isRgb">Flag indicating if the group is RGB.</param>
        /// <param name="topRowLights">The number of lights in the top row.</param>
        /// <param name="bottomRowLights">The number of lights in the bottom row.</param>
        /// <param name="leftColumnLights">The number of lights in the left column.</param>
        /// <param name="rightColumnLights">The number of lights in the right column.</param>
        public LightBezelGroup(byte group, bool isRgb, byte topRowLights, byte bottomRowLights,
                               byte leftColumnLights, byte rightColumnLights)
            : base(group, (ushort)(topRowLights + bottomRowLights + leftColumnLights + rightColumnLights), isRgb)
        {
            TopRowLights = topRowLights;
            BottomRowLights = bottomRowLights;
            LeftColumnLights = leftColumnLights;
            RightColumnLights = rightColumnLights;
        }
    }
}
