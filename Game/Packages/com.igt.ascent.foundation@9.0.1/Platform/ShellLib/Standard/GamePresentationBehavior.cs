// -----------------------------------------------------------------------
// <copyright file = "GamePresentationBehavior.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IGamePresentationBehavior"/> that uses
    /// F2X to communicate with the Foundation to support game play status.
    /// </summary>
    internal sealed class GamePresentationBehavior : GamePresentationBehaviorBase
    {
        #region Private Fields

        /// <summary>
        /// The interface for the game presentation behavior category. 
        /// </summary>
        private readonly CategoryInitializer<IGamePresentationBehaviorCategory> gamePresentationCategory =
            new CategoryInitializer<IGamePresentationBehaviorCategory>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the instance of <see cref="GamePresentationBehavior"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IGamePresentationBehaviorCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            gamePresentationCategory.Initialize(category);
        }

        #endregion

        #region GamePresentationBehaviorBase Overrides

        /// <inheritdoc/>
        public override void NewContext(IShellContext shellContext)
        {
            var configData = gamePresentationCategory.Instance.GetConfigData();

            MinimumFreeSpinTime = (int)configData.FreeSpinPresentationTime;

            MinimumBaseGameTime = (int)configData.MinimumBaseGamePresentationTime;

            CreditMeterBehavior = (CreditMeterDisplayBehaviorMode)configData.CreditMeterDisplayBehavior;

            MaxBetButtonBehavior = (MaxBetButtonBehavior)configData.MaxBetButtonPanelBehavior;

            DisplayVideoReelsForStepper = configData.DisplayVideoReelsForStepper;
        }

        #endregion
    }
}