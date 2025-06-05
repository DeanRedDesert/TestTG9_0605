// -----------------------------------------------------------------------
// <copyright file = "GameInformationDisplayStyle.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    /// <summary>
    /// Defines the game information display style.
    /// </summary>
    public enum GameInformationDisplayStyle
    {
        /// <summary>
        /// Don't display the game information.
        /// </summary>
        None = 0,

        /// <summary>
        /// Display the game information using the "Victorian" style.
        /// </summary>
        Victoria = 1,

        /// <summary>
        /// Display the game information using the "Queensland" style.
        /// </summary>
        Queensland = 2,

        /// <summary>
        /// Display the game information using the "NewZealand" style.
        /// </summary>
        NewZealand = 3,
    }
}
