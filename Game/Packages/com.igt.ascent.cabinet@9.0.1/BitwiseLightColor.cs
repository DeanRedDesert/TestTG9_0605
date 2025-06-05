//-----------------------------------------------------------------------
// <copyright file = "BitwiseLightColor.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{

    /// <summary>
    /// Enumeration used for bitwise light color.
    /// </summary>
    /// <remarks>
    /// EnumStorageShouldBeInt32 is suppressed because this enumeration is associated with a hardware device where the
    /// storage type is a byte.
    /// </remarks>
    public enum BitwiseLightColor : byte
    {
        /// <summary>
        /// Four bit black color.
        /// </summary>
        Black = 0,

        /// <summary>
        /// Four bit red/brown color.
        /// </summary>
        RedBrown,

        /// <summary>
        /// Four bit green color.
        /// </summary>
        Green,

        /// <summary>
        /// Four bit brown/green color.
        /// </summary>
        BrownGreen,

        /// <summary>
        /// Four bit dark blue color.
        /// </summary>
        DarkBlue,

        /// <summary>
        /// Four bit purple color.
        /// </summary>
        Purple,

        /// <summary>
        /// Four bit blue/green color.
        /// </summary>
        BlueGreen,

        /// <summary>
        /// Four bit dark grey color.
        /// </summary>
        DarkGrey,

        /// <summary>
        /// Four bit light grey color.
        /// </summary>
        LightGrey,

        /// <summary>
        /// Four bit orange/red color.
        /// </summary>
        OrangeRed,

        /// <summary>
        /// Four bit vivid green color.
        /// </summary>
        VividGreen,

        /// <summary>
        /// Four bit yellow color.
        /// </summary>
        Yellow,

        /// <summary>
        /// Four bit blue color.
        /// </summary>
        Blue,

        /// <summary>
        /// Four bit pink/purple color.
        /// </summary>
        PinkPurple,

        /// <summary>
        /// Four bit light blue color.
        /// </summary>
        LightBlue,

        /// <summary>
        /// Four bit white color.
        /// </summary>
        White
    }
}
