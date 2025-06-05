//-----------------------------------------------------------------------
// <copyright file = "BetModifiers.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Ascent.Logic.BettingPermit.Interfaces;
    using Attributes;
    using Exceptions;
    using Interfaces;

    /// <summary>
    /// Contains bet modifier functions, as well as support for adding bet modifier delegates.
    /// </summary>
    public class BetModifiers : IBetModifiers
    {
        /// <summary>
        /// The validated result of executing a bet modifier.
        /// </summary>
        public class ValidatedResult : IBetModifierResult
        {
            /// <inheritdoc/>
            public string ModifierName { get; set; }

            /// <inheritdoc/>
            public bool Valid { get; set; }

            /// <inheritdoc/>
            public IBetData Data { get; set; }
        }

        /// <summary>
        /// Delegate definition for functions that consume bet data and bet
        /// configuration information and generate new bet data.
        /// </summary>
        /// <param name="betData">Original bet data to modify.</param>
        /// <param name="betConfiguration">Bet rules.</param>
        /// <returns>A new bet data.</returns>
        public delegate IBetData Modifier(IBetData betData, IBetConfiguration betConfiguration);

        /// <summary>The binding flags used in reflection.</summary>
        public const BindingFlags ReflectionBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// All modifiers to use during validation. Populated at construction and by <see cref="AddModifier"/>.
        /// </summary>
        private readonly Dictionary<string, Modifier> allModifiers = new Dictionary<string, Modifier>();

        /// <summary>
        /// All disabled modifiers.
        /// </summary>
        private readonly HashSet<string> disabledModifiers = new HashSet<string>(); 

        /// <summary>
        /// Constructor.
        /// </summary>
        public BetModifiers()
        {
            foreach(var method in GetType().GetMethods(ReflectionBindingFlags))
            {
                if(method.GetCustomAttributes(typeof(BetModifierAttribute), true).Length > 0)
                {
                    var target = method.IsStatic ? null : this;
                    var modifier = (Modifier)Delegate.CreateDelegate(typeof(Modifier), target, method);
                    allModifiers.Add(method.Name, modifier);
                }
            }
        }

        /// <summary>
        /// Add a modifier to the dictionary at runtime. Must have a unique name.
        /// </summary>
        /// <param name="modifierName">Name of the modifier.</param>
        /// <param name="modifier">Modifier delegate.</param>
        /// <returns>True if the name was unique and the modifier was added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public bool AddModifier(string modifierName, Modifier modifier)
        {
            if(modifierName == null)
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            if(modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }
            if(allModifiers.ContainsKey(modifierName))
            {
                // No duplicates
                return false;
            }
            allModifiers.Add(modifierName, modifier);
            return true;
        }

        #region IBetModifiers

        /// <inheritdoc/>
        public IEnumerable<IBetModifierResult> Validate(IBetData betData,
                                                        IBetConfiguration betConfiguration,
                                                        IBettingPermit bettingPermit)
        {
            if(betData == null)
            {
                throw new ArgumentNullException(nameof(betData));
            }
            if(betConfiguration == null)
            {
                throw new ArgumentNullException(nameof(betConfiguration));
            }
            if(bettingPermit == null)
            {
                throw new ArgumentNullException(nameof(bettingPermit));
            }

            // Only execute and validate the enabled modifiers
            var enabledModifiers = allModifiers.Where(pair => !disabledModifiers.Contains(pair.Key)).ToList();

            if(!enabledModifiers.Any())
            {
                return new List<IBetModifierResult>();
            }

            // Execute the modifiers.
            var modifierResults = enabledModifiers.Select(pair => new
                                                                      {
                                                                          ModifierName = pair.Key,
                                                                          Data = pair.Value(betData, betConfiguration),
                                                                      })
                                                  .ToList();

            // Check whether the bets are permitted, using the default denomination of the bettingPermit instance.
            // TODO:
            // We have to call betting permit for all bets at the same time to take advantage of the consolidated F2L message.
            // With GameSideBettingPermit, there is no benefits doing that, therefore we probably should be able to
            // avoid the "zipping" for GameSideBettingPermit if we no longer have to support FoundationBettingPermit,
            // or if we find a way to customize this piece of logic here for different betting permit implementations.
            var betPermits = bettingPermit.CanBet(modifierResults.Select(modifierResult => modifierResult.Data.Total()));

            return modifierResults.Zip(betPermits,
                                       (modifierResult, betPermitted) =>
                                           new ValidatedResult
                                               {
                                                   ModifierName = modifierResult.ModifierName,
                                                   Data = modifierResult.Data,

                                                   // Firstly, bet data itself has to be valid and enabled;
                                                   // Secondly, if it is not a Commit bet, we skip all the rule checking;
                                                   // Rule checking #1, the bet should be valid according to Bet Configuration;
                                                   // Rule checking #2, the bet amount must be permitted in current game state;
                                                   // There could be overlapped checking here among BetData, BetConfiguration
                                                   // and BettingPermit.
                                                   Valid = modifierResult.Data.IsValid() &&
                                                           modifierResult.Data.Enabled &&
                                                           (!modifierResult.Data.Commit ||
                                                            betConfiguration.IsValid(modifierResult.Data) && betPermitted)
                                               } as IBetModifierResult);
        }

        /// <inheritdoc/>
        public IBetData Execute(string modifierName, IBetData betData, IBetConfiguration betConfiguration)
        {
            if(modifierName == null)
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            if(betData == null)
            {
                throw new ArgumentNullException(nameof(betData));
            }
            if(betConfiguration == null)
            {
                throw new ArgumentNullException(nameof(betConfiguration));
            }
            if(!allModifiers.ContainsKey(modifierName))
            {
                throw new InvalidModifierException(modifierName);
            }

            return allModifiers[modifierName](betData, betConfiguration);
        }

        /// <inheritdoc/>
        public IEnumerable<string> AllModifiers => allModifiers.Keys;

        /// <inheritdoc/>
        public IEnumerable<string> EnabledModifiers => allModifiers.Keys.Except(disabledModifiers);

        /// <inheritdoc/>
        public IEnumerable<string> DisabledModifiers => disabledModifiers;

        /// <inheritdoc/>
        public void Enable(string modifierName)
        {
            if(modifierName == null)
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            if (!allModifiers.ContainsKey(modifierName))
            {
                throw new InvalidModifierException(modifierName);
            }
            if(disabledModifiers.Contains(modifierName))
            {
                disabledModifiers.Remove(modifierName);
            }
        }

        /// <inheritdoc/>
        public void Disable(string modifierName)
        {
            if (modifierName == null)
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            if (!allModifiers.ContainsKey(modifierName))
            {
                throw new InvalidModifierException(modifierName);
            }
            if(!disabledModifiers.Contains(modifierName))
            {
                disabledModifiers.Add(modifierName);
            }
        }

        #endregion
    }
}
