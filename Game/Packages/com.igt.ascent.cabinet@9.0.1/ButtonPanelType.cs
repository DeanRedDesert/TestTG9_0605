//-----------------------------------------------------------------------
// <copyright file = "ButtonPanelType.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// This class defines the button panel type.
    /// </summary>
    public enum ButtonPanelType
    {
        /// <summary>
        /// Type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Static panel which only has static buttons.  
        /// </summary>
        Static,

        /// <summary>
        /// Dynamic panel which only has dynamic buttons.  
        /// </summary>
        Dynamic,

        /// <summary>
        /// Hybrid panel which has both static and dynamic buttons.  
        /// </summary>
        StaticDynamic
    }
}
