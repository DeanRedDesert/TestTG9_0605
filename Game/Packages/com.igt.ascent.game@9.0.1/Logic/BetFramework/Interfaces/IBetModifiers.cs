//-----------------------------------------------------------------------
// <copyright file = "IBetModifiers.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Ascent.Logic.BettingPermit.Interfaces;
    using Exceptions;

    /// <summary>
    /// Interface for classes containing bet modifiers.
    /// </summary>
    public interface IBetModifiers
    {
        /// <summary>
        /// Validates enabled modifiers by executing the modifiers against a bet data and a bet configuration,
        /// then checking whether it is permitted by the given betting permit implementation.
        /// </summary>
        /// <param name="betData">Bet data to validate against.</param>
        /// <param name="betConfiguration">Bet configuration to validate with.</param>
        /// <param name="bettingPermit">Betting permit instance to perform the validation.</param>
        /// <returns>An enumeration of validated modifier results. See <see cref="IBetModifierResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        IEnumerable<IBetModifierResult> Validate(IBetData betData,
                                                 IBetConfiguration betConfiguration,
                                                 IBettingPermit bettingPermit);

        /// <summary>
        /// Execute a modifier on a bet data.
        /// </summary>
        /// <param name="modifierName">Name of the modifier to execute.</param>
        /// <param name="betData">Bet data to modify.</param>
        /// <param name="betConfiguration">Bet configuration to modify with.</param>
        /// <returns>A new bet data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="InvalidModifierException">Thrown if the requested modifier cannot be found.</exception>
        IBetData Execute(string modifierName, IBetData betData, IBetConfiguration betConfiguration);

        /// <summary>
        /// An enumeration of all modifiers found in the class, including disabled modifiers.
        /// </summary>
        IEnumerable<string> AllModifiers { get; }

        /// <summary>
        /// An enumeration of all enabled modifiers. Subset of AllModifiers.
        /// </summary>
        IEnumerable<string> EnabledModifiers { get; }

        /// <summary>
        /// An enumeration of all disabled modifiers. Subset of AllModifiers.
        /// </summary>
        IEnumerable<string> DisabledModifiers { get; } 

        /// <summary>
        /// Enable a modifier. Enabling an enabled modifier has no effect.
        /// </summary>
        /// <param name="modifierName">Name of the modifier to enable.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="InvalidModifierException">Thrown if the requested modifier cannot be found.</exception>
        void Enable(string modifierName);

        /// <summary>
        /// Disable a modifier. Disabling an disabled modifier has no effect.
        /// </summary>
        /// <param name="modifierName">Name of the modifier to disable.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="InvalidModifierException">Thrown if the requested modifier cannot be found.</exception>
        void Disable(string modifierName);
    }
}
