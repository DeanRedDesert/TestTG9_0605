//-----------------------------------------------------------------------
// <copyright file = "LightBarGroup.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Light group for the Light Bar sub type.
    /// </summary>
    public class LightBarGroup : LightGroup
    {
        /// <summary>
        /// Each entry represents the number of lights in a bar. The total number of bars is the number of entries.
        /// </summary>
        public IEnumerable<byte> LightCounts { get; }

        /// <summary>
        /// Construct an instance of the light group.
        /// </summary>
        /// <param name="group">The number for the group.</param>
        /// <param name="isRgb">Flag indicating if the group is RGB.</param>
        /// <param name="bars">List of the bars in the group. Each entry is the number of lights in a bar.</param>
        // TODO: Convert IEnumerable to IReadOnlyCollection or IReadOnlyList
        // ReSharper disable PossibleMultipleEnumeration
        public LightBarGroup(byte group, bool isRgb, IEnumerable<byte> bars) : base(group, (ushort)bars.Sum(bar => bar), isRgb)
        {
            LightCounts = new List<byte>(bars);
        }
        // ReSharper restore PossibleMultipleEnumeration
    }
}
