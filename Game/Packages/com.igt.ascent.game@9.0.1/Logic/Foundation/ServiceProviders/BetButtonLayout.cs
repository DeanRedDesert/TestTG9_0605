// -----------------------------------------------------------------------
// <copyright file = "BetButtonLayout.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    /// <summary>
    /// Specifies the layout of bet buttons.
    /// </summary>
    public enum BetButtonLayout
    {
        /// <summary>
        /// Specifies that the lines button should be displayed on the top row of buttons. 
        /// </summary>
        LinesDominant = 0,

        /// <summary>
        /// Specifies that the bet multiplier buttons should be displayed on the top row of buttons. 
        /// </summary>
        MultiplierDominant,
    }
}