// -----------------------------------------------------------------------
// <copyright file = "BetProviderBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Logic.BettingPermit.Interfaces;
    using Interfaces;
    using Services;

    /// <summary>
    /// A base implementation of bet provider.
    /// </summary>
    public class BetProviderBase : INotifyAsynchronousProviderChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BetProviderBase"/>.
        /// </summary>
        /// <param name="data">
        /// <see cref="IBetData"/> instance to expose to the presentation.
        /// </param>
        /// <param name="configuration">
        /// <see cref="IBetConfiguration"/> instance to expose to the presentation.
        /// </param>
        /// <param name="modifiers">
        /// <see cref="IBetModifiers"/> instance to expose to the presentation.
        /// </param>
        /// <param name="bettingPermit">
        /// Interface to the can-bet logic.
        /// </param>
        /// <param name="denomination">
        /// The denomination of current game context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any parameter is null.
        /// </exception>
        public BetProviderBase(IBetData data, IBetConfiguration configuration, IBetModifiers modifiers,
                               IBettingPermit bettingPermit, long denomination)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
            BettingPermit = bettingPermit ?? throw new ArgumentNullException(nameof(bettingPermit));
            GameDenomination = denomination;
            AllFixedCommittalModifiers = new List<string>();
        }

        #endregion

        #region Game Services

        /// <summary>
        /// The current valid bet modifiers.
        /// </summary>
        /// <remarks>
        /// The bet modifiers should be updated when the bank is locked on a bank status changed.
        /// </remarks>
        [AsynchronousGameService]
        public List<string> AvailableModifiers
        {
            get
            {
                return allModifierResults.Where(result => result.Valid)
                    .Select(result => result.ModifierName).ToList();
            }
        }

        /// <summary>
        /// The total credit values of all available modifiers
        /// </summary>
        [AsynchronousGameService]
        public Dictionary<string, long> AllModifierTotals
        {
            get
            {
                return allModifierResults.ToDictionary(result => result.ModifierName, result => result.Data.Total());
            }
        }

        /// <summary>
        /// A list of all fixed committal modifiers.
        /// </summary>
        [AsynchronousGameService]
        public List<string> AllFixedCommittalModifiers { get; private set; }

        /// <summary>
        /// Gets the current value of the bet-changed flag.
        /// </summary>
        [GameService]
        public bool BetChanged => Data.BetChanged;

        /// <summary>
        /// Gets the total value of the current bet configuration, in base units.
        /// </summary>
        [GameService]
        public long GetTotalInBaseValue()
        {
            return Data.Total() * GameDenomination;
        }

        /// <summary>
        /// Gets the total value of the current bet configuration, in the smallest unit of currency.
        /// </summary>
        [GameService]
        public long GetTotalInCurrencyValue()
        {
            return Data.Total() * GameDenomination;
        }

        /// <summary>
        /// Gets the total value of the current bet configuration, in units game denomination.
        /// </summary>
        [GameService]
        public long GetTotalInGameCredits()
        {
            return Data.Total();
        }

        /// <summary>
        /// Gets the current value of the commit flag.
        /// </summary>
        /// <returns>The commit flag. i.e., should the game commit the current bet.</returns>
        [GameService]
        public bool GetCommit()
        {
            return Data.Commit;
        }

        /// <summary>
        /// Gets the minimum wager that can be placed given the current bet configuration.
        /// </summary>
        [AsynchronousGameService]
        public long MinimumWager
        {
            get
            {
                long wager = 0;

                foreach(var modifier in allModifierResults)
                {
                    var total = modifier.Data.Total();
                    // Find the smallest total that isn't 0.
                    if(total != 0 && (wager == 0 || wager > total))
                    {
                        wager = total;
                    }
                }

                return wager;
            }
        }

        /// <summary>
        /// Gets the maximum per-game wager allowed.
        /// </summary>
        [GameService]
        public long MaximumWager => Configuration.MaximumWager;

        /// <summary>
        /// The number of components (i.e., paylines) that may be bet on. This must be set.
        /// </summary>
        [GameService]
        public long NumberOfBetItems => Configuration.NumberOfBetItems;

        /// <summary>
        /// Gets the maximum per-item wager allowed.
        /// </summary>
        [GameService]
        public long MaximumWagerPerBetItem => Configuration.MaximumWagerPerBetItem;

        #region Game Services of Type-specific Getters

        /// <summary>
        /// Type specific get service for int variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public int GetInt(string variableName)
        {
            return Data.GetVariable<int>(variableName);
        }

        /// <summary>
        /// Type specific get service for uint variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public uint GetUint(string variableName)
        {
            return Data.GetVariable<uint>(variableName);
        }

        /// <summary>
        /// Type specific get service for long variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public long GetLong(string variableName)
        {
            return Data.GetVariable<long>(variableName);
        }

        /// <summary>
        /// Type specific get service for float variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public float GetFloat(string variableName)
        {
            return Data.GetVariable<float>(variableName);
        }

        /// <summary>
        /// Type specific get service for double variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public double GetDouble(string variableName)
        {
            return Data.GetVariable<double>(variableName);
        }

        /// <summary>
        /// Type specific get service for string variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public string GetString(string variableName)
        {
            return Data.GetVariable<string>(variableName);
        }

        /// <summary>
        /// Type specific get service for boolean variables.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of the variable.</returns>
        [GameService]
        public bool GetBool(string variableName)
        {
            return Data.GetVariable<bool>(variableName);
        }

        /// <summary>
        /// Type specific get service for all int variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, int> GetIntByModifier(string variableName)
        {
            return GetValueByModifier<int>(variableName);
        }

        /// <summary>
        /// Type specific get service for all uint variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, uint> GetUintByModifier(string variableName)
        {
            return GetValueByModifier<uint>(variableName);
        }

        /// <summary>
        /// Type specific get service for all long variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>A container with the modifier and variable associated with it.</returns>
        [GameService]
        public IDictionary<string, long> GetLongByModifier(string variableName)
        {
            return GetValueByModifier<long>(variableName);
        }

        /// <summary>
        /// Type specific get service for all long variables by this name with a non committal modifier.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, long> GetLongByNonCommittalModifier(string variableName)
        {
            var byModifier = new Dictionary<string, long>();
            foreach(var result in allModifierResults)
            {
                if(!result.Data.StartGame && !result.Data.Commit)
                {
                    byModifier[result.ModifierName] = result.Data.GetVariable<long>(variableName);
                }
            }

            return byModifier;
        }
        /// <summary>
        /// Type specific get service for all float variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, float> GetFloatByModifier(string variableName)
        {
            return GetValueByModifier<float>(variableName);
        }

        /// <summary>
        /// Type specific get service for all double variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, double> GetDoubleByModifier(string variableName)
        {
            return GetValueByModifier<double>(variableName);
        }

        /// <summary>
        /// Type specific get service for all string variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, string> GetStringByModifier(string variableName)
        {
            return GetValueByModifier<string>(variableName);
        }

        /// <summary>
        /// Type specific get service for all bool variables by this name.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>The modifier and the variable associate with it.</returns>
        [GameService]
        public IDictionary<string, bool> GetBoolByModifier(string variableName)
        {
            return GetValueByModifier<bool>(variableName);
        }

        #endregion

        #endregion

        #region Public Members

        /// <summary>
        /// <see cref="IBetData"/> instance to expose to the presentation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if null is set.</exception>
        public IBetData Data
        {
            get => data;
            set => data = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Bet configuration to expose to the presentation.
        /// </summary>
        public IBetConfiguration Configuration { get; }

        /// <summary>
        /// Validate the modifiers in <see cref="Modifiers"/> and update
        /// <see cref="AvailableModifiers"/> and <see cref="AllModifierTotals"/>.
        /// </summary>
        /// <remarks>Raises the <see cref="AsynchronousProviderChanged"/> event.</remarks>
        public void UpdateModifiers()
        {
            allModifierResults = Modifiers.Validate(Data, Configuration, BettingPermit).ToList();
            RaiseAsynchronousProviderChanged(UpdateModifierServices);
        }

        /// <summary>
        /// Set the <see cref="AllFixedCommittalModifiers"/> list.
        /// </summary>
        /// <param name="committalModifiers">Enumeration of the committal modifiers.</param>
        /// <remarks>Raises the <see cref="AsynchronousProviderChanged"/> event.</remarks>
        public void SetFixedCommittalModifiers(IEnumerable<string> committalModifiers)
        {
            AllFixedCommittalModifiers = committalModifiers.ToList();
            RaiseAsynchronousProviderChanged("AllFixedCommittalModifiers");
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Implementation

        /// <inheritdoc/>
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region Protected Members

        /// <summary>
        /// Bet modifiers to expose to the presentation.
        /// </summary>
        protected IBetModifiers Modifiers { get; }

        /// <summary>
        /// Denomination of current game context.
        /// </summary>
        protected readonly long GameDenomination;

        /// <summary>
        /// Interface to the can-bet logic.
        /// </summary>
        protected readonly IBettingPermit BettingPermit;

        #endregion

        #region Private Members

        private static readonly List<ServiceSignature> UpdateModifierServices = new List<ServiceSignature>
                                                                                    {
                                                                                        new ServiceSignature("AvailableModifiers"),
                                                                                        new ServiceSignature("AllModifierTotals"),
                                                                                        new ServiceSignature("MinimumWager")
                                                                                    };

        /// <summary>
        /// A collection of all modifier results after validation.
        /// </summary>
        private List<IBetModifierResult> allModifierResults = new List<IBetModifierResult>();

        /// <summary>
        /// Bet data instance to expose to the presentation.
        /// </summary>
        private IBetData data;

        /// <summary>
        /// Raise the AsynchronousProviderChanged event for a service.
        /// </summary>
        /// <param name="service">The name of the updated service.</param>
        private void RaiseAsynchronousProviderChanged(string service)
        {
            var handler = AsynchronousProviderChanged;
            handler?.Invoke(this, new AsynchronousProviderChangedEventArgs(service));
        }

        /// <summary>
        /// Raise the AsynchronousProviderChanged event for a list of services.
        /// </summary>
        /// <param name="services">All updated services.</param>
        private void RaiseAsynchronousProviderChanged(IList<ServiceSignature> services)
        {
            var handler = AsynchronousProviderChanged;
            handler?.Invoke(this, new AsynchronousProviderChangedEventArgs(services, true));
        }

        /// <summary>
        /// Type specific getter for all variables by this name.
        /// </summary>
        /// <typeparam name="TVariable">The variable type.</typeparam>
        /// <param name="variableName">The variable name.</param>
        /// <returns>The modifier and value for the variable.</returns>
        private IDictionary<string, TVariable> GetValueByModifier<TVariable>(string variableName)
        {
            var byModifier = new Dictionary<string, TVariable>();
            foreach(var result in allModifierResults)
            {
                byModifier[result.ModifierName] = result.Data.GetVariable<TVariable>(variableName);
            }

            return byModifier;
        }

        #endregion
    }
}