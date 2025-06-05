//-----------------------------------------------------------------------
// <copyright file = "LightGroup.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Basic light group. Used for sub types which do not have any special attributes.
    /// </summary>
    public class LightGroup : ILightGroup
    {
        #region ILightGroup Members

        /// <inheritdoc />
        public byte GroupNumber { get; }

        /// <inheritdoc />
        public ushort Count { get; }

        /// <inheritdoc />
        public bool IsRgb { get; }

        #endregion

        /// <summary>
        /// Construct an instance of the light group.
        /// </summary>
        /// <param name="group">The number for the group.</param>
        /// <param name="count">The number of lights in the group.</param>
        /// <param name="isRgb">Flag indicating if the group is RGB.</param>
        public LightGroup(byte group, ushort count, bool isRgb)
        {
            GroupNumber = group;
            Count = count;
            IsRgb = isRgb;
        }
    }
}
