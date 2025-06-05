// -----------------------------------------------------------------------
// <copyright file = "TopScreenGameAdvertisementType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Enumeration describing the type of top screen advertisement.
    /// </summary>
    [Serializable]
    public enum TopScreenGameAdvertisementType
    {
        /// <summary>
        /// Indicates top screen game advertisement information is not available. 
        /// In this case, the game is responsible for displaying its standard advertisement.
        /// </summary>
        Invalid,

        /// <summary>
        /// Indicates games should show a general, graphical screen which promotes the game.
        /// </summary>
        Promo,

        /// <summary>
        /// Indicates games should show possible win combinations for the game.
        /// </summary>
        Paytable
    }
}