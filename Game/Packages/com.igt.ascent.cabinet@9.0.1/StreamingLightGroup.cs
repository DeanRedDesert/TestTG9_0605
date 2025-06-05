//-----------------------------------------------------------------------
// <copyright file = "StreamingLightGroup.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// A streaming light group.
    /// </summary>
    public class StreamingLightGroup : LightGroup
    {
        /// <summary>
        /// Construct a new <see cref="StreamingLightGroup"/> that specifies if symbol highlights are supported.
        /// </summary>
        /// <param name="group">The number for the group.</param>
        /// <param name="count">The number of lights in the group.</param>
        /// <param name="realTimeControlSupported">
        /// Flag indicating if the group supports real time control.
        /// </param>
        /// <param name="intensityControlSupported">
        /// Flag indicating if the group supports intensity control.
        /// </param>
        /// <param name="symbolHighlightsSupported">
        /// Flag indicating if the group supports symbol highlights.
        /// </param>
        public StreamingLightGroup(byte group, ushort count,
                                   bool realTimeControlSupported,
                                   bool intensityControlSupported,
                                   bool symbolHighlightsSupported)
            : base(group, count, true)
        {
            RealTimeControlSupported = realTimeControlSupported;
            IntensityControlSupported = intensityControlSupported;
            SymbolHighlightsSupported = symbolHighlightsSupported;
        }

        /// <summary>
        /// Construct a new <see cref="StreamingLightGroup"/> that does not specify symbol highlights being supported.
        /// </summary>
        /// <param name="group">The number for the group.</param>
        /// <param name="count">The number of lights in the group.</param>
        /// <param name="realTimeControlSupported">
        /// Flag indicating if the group supports real time control.
        /// </param>
        /// <param name="intensityControlSupported">
        /// Flag indicating if the group supports intensity control.
        /// </param>
        public StreamingLightGroup(byte group, ushort count,
                                   bool realTimeControlSupported,
                                   bool intensityControlSupported)
            : this(group, count, realTimeControlSupported, intensityControlSupported, false)
        {
        }

        /// <summary>
        /// If this group supports real time control.
        /// </summary>
        public bool RealTimeControlSupported
        {
            get;
        }

        /// <summary>
        /// Gets if this group supports intensity control.
        /// </summary>
        public bool IntensityControlSupported
        {
            get;
        }

        /// <summary>
        /// Gets if this group supports symbol tracking and hot positions.
        /// </summary>
        public bool SymbolHighlightsSupported
        {
            get;
        }
    }
}
