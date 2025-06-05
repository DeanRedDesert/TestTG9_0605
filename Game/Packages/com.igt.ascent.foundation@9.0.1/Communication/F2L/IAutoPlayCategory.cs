//-----------------------------------------------------------------------
// <copyright file = "IAutoPlayCategory.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    /// <summary>
    /// Interface which provides the functions available in the auto play category of the F2L.
    /// </summary>
    public interface IAutoPlayCategory
    {
        /// <summary>
        /// Get a boolean indicating if the player initiated auto play is enabled
        /// in the current theme context.
        /// </summary>
        /// <returns>True if the player initiated auto play is enabled.</returns>
        bool GetConfigDataPlayerAutoPlayEnabled();

        /// <summary>
        /// Get a boolean indicating if the auto play confirmation is enabled
        /// in the current theme context.
        /// </summary>
        /// <returns>True if the auto play confirmation is enabled.</returns>
        bool GetConfigDataPlayerAutoPlayConfirmationRequired();

        /// <summary>
        /// Get a nullable boolean indicating if increasing the game speed during autoplay is allowed
        /// in the current theme context.
        /// </summary>
        /// <returns>True if increasing the game speed during autoplay is allowed, false if not,
        /// null if the method is not supported by the Foundation.</returns>
        bool? GetConfigDataAutoPlaySpeedIncreaseAllowed();

        /// <summary>
        /// Get a boolean indicating if the auto play is currently on.
        /// </summary>
        /// <returns>True if the auto play is currently on.</returns>
        bool IsAutoPlayOn();

        /// <summary>
        /// Request the foundation to turn the auto play on.
        /// </summary>
        /// <returns>True if the auto play has been successfully turned on.</returns>
        bool SetAutoPlayOnRequest();

        /// <summary>
        /// Request the foundation to turn off the auto play.
        /// </summary>
        void SetAutoPlayOff();
    }
}
