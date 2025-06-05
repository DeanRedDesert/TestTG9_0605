//-----------------------------------------------------------------------
// <copyright file = "LightSubFeature.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{

    /// <summary>
    /// Enumeration which indicates the sub feature type of a light group.
    /// </summary>
    public enum LightSubFeature
    {
        /// <summary>
        /// Bonus game lights. Bonus game lights have no special attributes.
        /// </summary>
        BonusGameLights = 0,

        /// <summary>
        /// Light bezel. Light bezels produce a frame and have a top and bottom row and a right and left column.
        /// </summary>
        LightBezel,

        /// <summary>
        /// Light bars have several sets of lights and the length of each set can be determined.
        /// </summary>
        LightBars,

        /// <summary>
        /// Reel backlights. Reel backlights have no special attributes.
        /// </summary>
        ReelBacklights,

        /// <summary>
        /// Candle lights. Candle lights have no special attributes.
        /// </summary>
        Candle,

        /// <summary>
        /// Accent lights.
        /// </summary>
        AccentLights,

        /// <summary>
        /// The bezel for the card reader. These lights have no special attributes.
        /// </summary>
        CardReaderBezel,

        /// <summary>
        /// Topper light ring. These lights have no special attributes.
        /// </summary>
        TopperLightRing,

        /// <summary>
        /// The reel divider lights. These lights have no special attributes.
        /// </summary>
        ReelDividerLights,

        /// <summary>
        /// The reel highlight lights. These lights have no special attributes.
        /// </summary>
        ReelHighlights
    }
}
