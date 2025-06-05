//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpRunTimeGameEvent.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RunTimeGameEvent
{
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpRunTimeGameEvent extended interface.
    /// </summary>
    internal sealed class StandaloneUgpRunTimeGameEvent : IUgpRunTimeGameEvent, IInterfaceExtension
    {
        #region IUgpRunTimeGameEvent Implementation

        /// <inheritdoc/>
        public void WaitingForTakeWin(bool value)
        {
        }

        /// <inheritdoc/>
        public void WaitingForStartFeature(bool value)
        {
        }

        /// <inheritdoc/>
        public void WaitingForPlayerSelection(bool value)
        {
        }

        /// <inheritdoc/>
        public void WaitingForGenericInput(bool value)
        {
        }

        /// <inheritdoc/>
        public void PlayerChoice(uint playerChoiceIndex)
        {
        }

        /// <inheritdoc/>
        public void DenomSelectionActive(bool active)
        {
        }

        #endregion
    }
}
