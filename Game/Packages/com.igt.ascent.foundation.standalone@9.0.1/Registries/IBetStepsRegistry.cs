// -----------------------------------------------------------------------
// <copyright file = "IBetStepsRegistry.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System.Collections.Generic;
    using Core.Registries.Internal.F2X.F2XBetStepsRegistryVer1;

    /// <summary>
    /// This interface represents a bet steps registry object and is used to retrieve the information from
    /// a bet steps registry file.
    /// </summary>
    public interface IBetStepsRegistry
    {
        #region Properties

        /// <summary>
        /// Gets the name of the bet steps registry file.
        /// </summary>
        string BetStepsRegistryFileName { get; }

        /// <summary>
        /// Gets the relative path to payvar registry the bet steps are defined for.
        /// </summary>
        string PayvarRegistryField { get; }

        /// <summary>
        ///  Gets the key for localized text to label the bet steps information in the game setup page. For example, "Available Bet Multipliers."
        /// </summary>
        string BetStepsLabelKey { get; }

        /// <summary>
        /// Gets the list of each maximum bet defined in the payvar registry and that maximum bet's corresponding bet steps.
        /// </summary>
        IList<BetStepSpecification> PreconfiguredBetSteps { get; }

        #endregion
    }
}