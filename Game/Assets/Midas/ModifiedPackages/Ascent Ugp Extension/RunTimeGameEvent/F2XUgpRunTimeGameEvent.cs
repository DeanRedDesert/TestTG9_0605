//-----------------------------------------------------------------------
// <copyright file = "F2XUgpRunTimeGameEvent.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RunTimeGameEvent
{
    using System;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpRunTimeGameEvent extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpRunTimeGameEvent : IUgpRunTimeGameEvent, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpRunTimeGameEvent category handler.
        /// </summary>
        private readonly IUgpRunTimeGameEventCategory ugpRunTimeGameEventCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpRunTimeGameEvent"/>.
        /// </summary>
        /// <param name="ugpRunTimeGameEventCategory">
        /// The UgpRunTimeGameEvent category handler used to communicate with the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="ugpRunTimeGameEventCategory"/> is null.
        /// </exception>
        public F2XUgpRunTimeGameEvent(IUgpRunTimeGameEventCategory ugpRunTimeGameEventCategory)
        {
            this.ugpRunTimeGameEventCategory = ugpRunTimeGameEventCategory ?? throw new ArgumentNullException(nameof(ugpRunTimeGameEventCategory));
        }

        #endregion

        #region IUgpRunTimeGameEvents Implementation

        /// <inheritdoc/>
        public void WaitingForTakeWin(bool value)
        {
            ugpRunTimeGameEventCategory.WaitingForTakeWin(value);
        }

        /// <inheritdoc/>
        public void WaitingForStartFeature(bool value)
        {
            ugpRunTimeGameEventCategory.WaitingForStartFeature(value);
        }

        /// <inheritdoc/>
        public void WaitingForPlayerSelection(bool value)
        {
            ugpRunTimeGameEventCategory.WaitingForPlayerSelection(value);
        }

        /// <inheritdoc/>
        public void WaitingForGenericInput(bool value)
        {
            ugpRunTimeGameEventCategory.WaitingForGenericInput(value);
        }

        /// <inheritdoc/>
        public void PlayerChoice(uint playerChoiceIndex)
        {
            ugpRunTimeGameEventCategory.PlayerChoice(playerChoiceIndex);
        }

        /// <inheritdoc/>
        public void DenomSelectionActive(bool active)
        {
            ugpRunTimeGameEventCategory.DenomSelectionActive(active);
        }

        #endregion
    }
}
