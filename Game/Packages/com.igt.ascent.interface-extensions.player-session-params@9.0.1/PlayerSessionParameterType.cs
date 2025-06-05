//-----------------------------------------------------------------------
// <copyright file = "PlayerSessionParameterType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;

    /// <summary>
    /// This enumeration is used to define the player session parameter type.
    /// </summary>
    [Serializable]
    public enum PlayerSessionParameterType
    {
        /// <summary>
        /// Indicates that the player session parameter is culture.
        /// </summary>
        Culture = 0,

        /// <summary>
        /// Indicates that the player session parameter is denomination.
        /// </summary>
        Denomination,

        /// <summary>
        /// Indicates that the player session parameter is player volume.
        /// </summary>
        PlayerVolume,

        /// <summary>
        /// Indicates that the player session parameter is game speed.
        /// </summary>
        GameSpeed,

        /// <summary>
        /// Indicates that the player session parameter is credit display mode.
        /// </summary>
        CreditDisplay,

        /// <summary>
        /// Indicates that the player session parameter is bet selection.
        /// </summary>
        BetSelection,

        /// <summary>
        /// Indicates that the player session parameter is theme specific.
        /// </summary>
        ThemeSpecific,

        /// <summary>
        /// Indicates that the player session parameter is chooser specific.
        /// </summary>
        ChooserSpecific,
    }
}
