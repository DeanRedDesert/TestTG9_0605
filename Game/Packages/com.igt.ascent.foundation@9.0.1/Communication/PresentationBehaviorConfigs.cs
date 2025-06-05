// -----------------------------------------------------------------------
// <copyright file = "PresentationBehaviorConfigs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// A simple implementation of <see cref="IPresentationBehaviorConfigs"/>.
    /// All properties are mutable to save the effort of implementing constructors.
    /// </summary>
    internal sealed class PresentationBehaviorConfigs : IPresentationBehaviorConfigs
    {
        #region Implementation of IPresentationBehaviorConfigs

        /// <inheritdoc />
        public int MinimumBaseGameTime { get; set; }

        /// <inheritdoc />
        public int MinimumFreeSpinTime { get; set; }

        /// <inheritdoc />
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; set; }

        /// <inheritdoc />
        public TopScreenGameAdvertisementType TopScreenGameAdvertisement { get; set; }

        /// <inheritdoc />
        public bool DisplayVideoReelsForStepper { get; set; }

        /// <inheritdoc />
        public BonusSoaaSettings BonusSoaaSettings { get; set; }

        #endregion
    }
}