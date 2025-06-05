//-----------------------------------------------------------------------
// <copyright file = "IUgpRunTimeGameEventCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RunTimeGameEvent
{
    /// <summary>
    /// Interface of UgpRunTimeGameEvent category of messages.
    /// </summary>
    public interface IUgpRunTimeGameEventCategory
    {
        ///<summary>
        /// Informs the foundation that taking a win is finished.
        ///</summary>
        ///<param name="value">True if it is waiting for taking win, false if the waiting is finished.</param>
        void WaitingForTakeWin(bool value);

        ///<summary>
        /// Informs the foundation that the feature is finished.
        ///</summary>
        ///<param name="value">True if it is waiting for the feature, false if the waiting is finished.</param>
        void WaitingForStartFeature(bool value);

        ///<summary>
        /// Informs the foundation that player selection is finished.
        ///</summary>
        ///<param name="value">True if it is waiting for player selection, false if the waiting is finished.</param>
        void WaitingForPlayerSelection(bool value);

        ///<summary>
        /// Informs the foundation that the input is finished.
        ///</summary>
        ///<param name="value">True if it is waiting for input, false if the waiting is finished.</param>
        void WaitingForGenericInput(bool value);

        /// <summary>
        /// Informs the foundation of the player choice index.
        /// </summary>
        /// <param name="playerChoiceIndex">The index of the player choice.</param>
        void PlayerChoice(uint playerChoiceIndex);

        /// <summary>
        /// Informs the foundation that denom selection is active.
        /// </summary>
        /// <param name="active">True if active, otherwise false.</param>
        void DenomSelectionActive(bool active);
    }
}
