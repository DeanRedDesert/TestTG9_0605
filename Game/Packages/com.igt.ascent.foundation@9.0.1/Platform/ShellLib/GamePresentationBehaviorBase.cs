// -----------------------------------------------------------------------
// <copyright file = "GamePresentationBehaviorBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib
{
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// Base class for <see cref="IGamePresentationBehavior"/> implementations.
    /// </summary>
    internal abstract class GamePresentationBehaviorBase : IGamePresentationBehavior, IContextCache<IShellContext>
    {
        #region IGamePresentationBehavior Implementation

        /// <inheritdoc/>
        public int MinimumBaseGameTime { get; protected set; }

        /// <inheritdoc/>
        public int MinimumFreeSpinTime { get; protected set; }

        /// <inheritdoc/>
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; protected set; }

        /// <inheritdoc/>
        public MaxBetButtonBehavior MaxBetButtonBehavior { get; protected set; }

        /// <inheritdoc />
        public bool DisplayVideoReelsForStepper { get; protected set; }

        #endregion

        #region IContextCacheImplementation

        /// <inheritdoc/>
        public abstract void NewContext(IShellContext shellContext);

        #endregion
    }
}