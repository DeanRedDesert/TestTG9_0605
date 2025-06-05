// -----------------------------------------------------------------------
// <copyright file = "BetStepsRegistryProxy.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using F2XBetStepsRegistryTip = Core.Registries.Internal.F2X.F2XBetStepsRegistryVer1;

    /// <summary>
    /// A proxy for the <see cref="F2XBetStepsRegistryTip"/> object that is used to
    /// retrieve information from the tip version of a bet steps registry.
    /// </summary>
    internal class BetStepsRegistryProxy : IBetStepsRegistry
    {
        #region Private Members

        /// <summary>
        /// The bet steps object the proxy represents.
        /// </summary>
        private readonly F2XBetStepsRegistryTip.BetStepsRegistry betStepsRegistry;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a <see cref="BetStepsRegistryProxy"/>.
        /// </summary>
        /// <param name="betStepsRegistry">
        /// The bet steps registry object.
        /// </param>
        /// <param name="betStepsRegistryFileName">The file name of the registry</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="betStepsRegistry" /> or <paramref name="betStepsRegistryFileName" /> is null.
        /// </exception>
        public BetStepsRegistryProxy(F2XBetStepsRegistryTip.BetStepsRegistry betStepsRegistry,
            string betStepsRegistryFileName)
        {
            this.betStepsRegistry = betStepsRegistry ?? throw new ArgumentNullException(nameof(betStepsRegistry), "Parameters may not be null.");
            BetStepsRegistryFileName = betStepsRegistryFileName ?? throw new ArgumentNullException(nameof(betStepsRegistryFileName), "Parameters may not be null.");
        }

        #endregion

        #region IBetStepsRegistryImplementation

        /// <inheritdoc />
        public string BetStepsRegistryFileName { get; }

        /// <inheritdoc />
        public string PayvarRegistryField => betStepsRegistry.PayvarRegistry;

        /// <inheritdoc />
        public string BetStepsLabelKey => betStepsRegistry.BetStepsLabelKey;

        /// <inheritdoc />
        public IList<F2XBetStepsRegistryTip.BetStepSpecification> PreconfiguredBetSteps => betStepsRegistry.PreconfiguredBetSteps;

        #endregion
    }
}