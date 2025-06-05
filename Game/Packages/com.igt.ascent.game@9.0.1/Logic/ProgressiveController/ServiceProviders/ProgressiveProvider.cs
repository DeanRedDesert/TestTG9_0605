//-----------------------------------------------------------------------
// <copyright file = "ProgressiveProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
// There were warnings caused by protected fields/methods that were designed for inherited classes to use.
namespace IGT.Game.Core.Logic.ProgressiveController.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    /// Provide updated progressive amounts and prize strings to the presentation.
    /// </summary>
    public class ProgressiveProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region INotifyAsynchronousProviderChanged Event

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the dictionary of progressive broadcast data keyed by game levels.
        /// </summary>
        /// <remarks>
        /// This is for the denomination and paytable currently in play.
        /// </remarks>
        [AsynchronousGameService]
        public virtual Dictionary<int, ProgressiveBroadcastData> ProgressiveBroadcastDataByGameLevels => BroadcastDataList;

        /// <summary>
        /// Gets the progressive broadcast data for enabled denominations that have been configured with progressives.
        /// </summary>
        /// <remarks>
        /// This includes denominations that are not currently in play.
        /// 
        /// By default, this service is a Synchronous service only.
        /// Turning on the Asynchronous service might impact the runtime performance.
        /// 
        /// To turn on/off the Asynchronous service, call <see cref="TurnOnAsyncUpdateForEnabledDenominationsWithProgressives"/>
        /// and <see cref="TurnOffAsyncUpdateForEnabledDenominationsWithProgressives"/> respectively.
        /// </remarks>
        /// <returns>
        /// The progressive broadcast data for enabled denominations.
        /// Each denomination has a dictionary of progressive data keyed by game levels.
        /// Returns empty dictionary if NOT in Play mode.
        /// </returns>
        [AsynchronousGameService]
        public virtual IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> EnabledDenominationsWithProgressives()
        {
            return GameLib.GameContextMode != GameMode.Play
                       ? EmptyBroadcastData
                       : IsAsyncUpdateEdp
                           ? DenominationsBroadcastData
                           : GetEnabledDenominationsWithProgressives(); // If Async service is off, we need to actively retrieve the data
        }

        #endregion

        #region Properties

        /// <summary>
        /// Flag indicating if the progressive data is locked.
        /// </summary>
        public bool IsAnyProgressiveLocked
        {
            get
            {
                return BroadcastDataList.Count != 0 &&
                       BroadcastDataList.Any(progressive => progressive.Value.IsLocked);
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// An instance of empty broadcast data to avoid repeated new allocations.
        /// </summary>
        protected readonly IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> EmptyBroadcastData =
            new Dictionary<long, IDictionary<int, ProgressiveBroadcastData>>();

        /// <summary>
        /// A dictionary of progressive broadcast data keyed by game levels.
        /// </summary>
        protected Dictionary<int, ProgressiveBroadcastData> BroadcastDataList =
            new Dictionary<int, ProgressiveBroadcastData>();

        /// <summary>
        /// The progressive broadcast data for enabled denominations.
        /// Each denomination has a dictionary of progressive data keyed by game levels.
        /// </summary>
        protected IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> DenominationsBroadcastData =
            new Dictionary<long, IDictionary<int, ProgressiveBroadcastData>>();

        /// <summary>
        /// The game lib.
        /// </summary>
        protected readonly IGameLib GameLib;

        /// <summary>
        /// The controller for the game controlled progressive.
        /// </summary>
        protected readonly IProgressiveController GameProgressiveController;

        /// <summary>
        /// Denomination, used to handle progressive data changed.
        /// </summary>
        protected long Denomination;

        /// <summary>
        /// The flag indicating whether <see cref="EnabledDenominationsWithProgressives"/> service
        /// is Asynchronous or not.
        /// </summary>
        protected bool IsAsyncUpdateEdp;

        /// <summary>
        /// Names of services for propagating progressive data for current game denomination.
        /// </summary>
        protected readonly List<string> BroadcastServiceNames = new List<string>
                                                                {
                                                                    nameof(ProgressiveBroadcastDataByGameLevels)
                                                                };

        /// <summary>
        /// Names of services for propagating progressive data for enabled denominations.
        /// </summary>
        protected readonly List<string> DenominationsBroadcastServiceNames = new List<string>
                                                                             {
                                                                                 nameof(EnabledDenominationsWithProgressives)
                                                                             };

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a progressive provider that provides the access to
        /// the progressive broadcast data from the Foundation.
        /// </summary>
        /// <param name="iGameLib">
        /// Game Lib interface that broadcasts progressive data from the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/> is null.
        /// </exception>
        public ProgressiveProvider(IGameLib iGameLib)
        {
            GameLib = iGameLib ?? throw new ArgumentNullException(nameof(iGameLib));
            GameProgressiveController = null;

            Denomination = GameLib.GameDenomination;

            GameLib.ProgressiveBroadcastEvent += HandleProgressiveBroadcastEvent;
            GameLib.ActivateThemeContextEvent += HandleDenominationChanged;
        }

        /// <summary>
        /// Construct a progressive provider that provides the access to
        /// the progressive information from both the Foundation and the
        /// Game Controlled Progressive controller.
        /// </summary>
        /// <param name="iGameLib">
        /// Game Lib interface that broadcasts progressive data from the Foundation.
        /// </param>
        /// <param name="iProgressiveController">
        /// Game controlled progressive controller that broadcasts GCP progressive data.
        /// </param>
        /// <param name="registerForSystemProgressives">
        /// Controls registration of the foundation progressive information. True will
        /// register for foundation controlled progressive updates.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="iGameLib"/> or <paramref name="iProgressiveController"/> is null.
        /// </exception>
        /// <remarks>
        /// No system progressives will be registered.
        /// </remarks>
        public ProgressiveProvider(IGameLib iGameLib,
                                   IProgressiveController iProgressiveController,
                                   bool registerForSystemProgressives = false)
        {
            GameLib = iGameLib ?? throw new ArgumentNullException(nameof(iGameLib));
            GameProgressiveController = iProgressiveController ?? throw new ArgumentNullException(nameof(iProgressiveController));

            Denomination = GameLib.GameDenomination;

            if(registerForSystemProgressives)
            {
                GameLib.ProgressiveBroadcastEvent += HandleProgressiveBroadcastEvent;
                GameLib.ActivateThemeContextEvent += HandleDenominationChanged;
            }

            GameProgressiveController.ProgressiveBroadcastEvent += HandleProgressiveBroadcastEvent;

            // Initialize the data for GCP since we probably have missed the last broadcasting.
            // NOTE: Unlike GameLib which we know the implementation, GCP controller might not always
            // return fresh instances of ProgressiveBroadcastData in its APIs.  As a precaution, make
            // sure we always deep clone the data returned by the controller.
            BroadcastDataList = GameProgressiveController.GetAllProgressiveBroadcastData()
                                                         .ToDictionary(x => x.Key,
                                                                       x => (ProgressiveBroadcastData)x.Value.DeepClone());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lock the progressives at values given by the progressive value dictionary. 
        /// </summary>
        /// <param name="progressiveValues">
        /// Progressive lock values indexed by game level.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="progressiveValues"/> contains a negative progressive value.
        /// </exception>
        public virtual void LockProgressiveAtValues(IDictionary<long, long> progressiveValues)
        {
            if(progressiveValues.Any(pair => pair.Value < 0))
            {
                throw new ArgumentException("Progressive amounts cannot be locked to a negative number.",
                                            nameof(progressiveValues));
            }

            var progressiveValueChanged = false;

            // The BroadcastDataList will be empty if the initial progressive values are not set. In this case, add in 
            // the progressives at the given levels and lock it to the given value.
            foreach(var progressive in progressiveValues)
            {
                if(BroadcastDataList.ContainsKey((int)progressive.Key))
                {
                    progressiveValueChanged |= BroadcastDataList[(int)progressive.Key].LockAmount != progressive.Value;
                    BroadcastDataList[(int)progressive.Key].Lock(progressive.Value);
                }
                else
                {
                    BroadcastDataList.Add((int)progressive.Key,
                                          new ProgressiveBroadcastData(progressive.Value, null, progressive.Value));

                    progressiveValueChanged = true;
                }
            }

            if(progressiveValueChanged)
            {
                OnServiceUpdated(BroadcastServiceNames, null);
            }
        }

        /// <summary>
        /// Remove all locks from progressive values.
        /// </summary>
        public virtual void UnlockProgressiveValues()
        {
            // Do nothing if the progressive is already unlocked.
            if(!IsAnyProgressiveLocked)
            {
                return;
            }

            // Remove lock value from every broadcast item. 
            foreach(var progressive in BroadcastDataList)
            {
                progressive.Value.Lock(null);
            }

            OnServiceUpdated(BroadcastServiceNames, null);
        }

        /// <summary>
        /// Turns on the async update for <see cref="EnabledDenominationsWithProgressives"/> service.
        /// Turning on the async update might impact runtime performance.
        /// </summary>
        public virtual void TurnOnAsyncUpdateForEnabledDenominationsWithProgressives()
        {
            if(GameLib.GameContextMode != GameMode.Play)
            {
                return;
            }

            IsAsyncUpdateEdp = true;

            // Subscribe to events to turn on async updates.
            GameLib.AvailableDenominationsWithProgressivesBroadcastEvent -= HandleAvailableDenominationsWithProgressivesBroadcastEvent;
            GameLib.AvailableDenominationsWithProgressivesBroadcastEvent += HandleAvailableDenominationsWithProgressivesBroadcastEvent;

            if(GameProgressiveController != null)
            {
                GameProgressiveController.ProgressiveBroadcastEvent -= HandleGcpUpdatesEnabledDenominationsWithProgressives;
                GameProgressiveController.ProgressiveBroadcastEvent += HandleGcpUpdatesEnabledDenominationsWithProgressives;
            }

            // Get the initial values.
            DenominationsBroadcastData = GetEnabledDenominationsWithProgressives();
        }

        /// <summary>
        /// Turns off the async update for <see cref="EnabledDenominationsWithProgressives"/> service.
        /// </summary>
        public virtual void TurnOffAsyncUpdateForEnabledDenominationsWithProgressives()
        {
            IsAsyncUpdateEdp = false;

            GameLib.AvailableDenominationsWithProgressivesBroadcastEvent -= HandleAvailableDenominationsWithProgressivesBroadcastEvent;
            if(GameProgressiveController != null)
            {
                GameProgressiveController.ProgressiveBroadcastEvent -= HandleGcpUpdatesEnabledDenominationsWithProgressives;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for ProgressiveBroadcastEvent.
        /// Cache the updated progressive broadcast data, and post the AsynchronousProviderChanged event
        /// to update the presentation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        protected virtual void HandleProgressiveBroadcastEvent(object sender, ProgressiveBroadcastEventArgs eventArgs)
        {
            if(eventArgs != null)
            {
                var valueChanged = false;
                foreach(var entry in eventArgs.BroadcastDataList)
                {
                    valueChanged |= UpdateBroadcastDataIfChanged(BroadcastDataList, entry.Key, entry.Value);
                }

                if(valueChanged)
                {
                    OnServiceUpdated(BroadcastServiceNames, null);
                }
            }
        }

        /// <summary>
        /// Event handler for changes in ActivateThemeContextEventArgs.
        /// If data changes, the BroadcastDataList will be rebuilt.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The eventArgs, if Denomination is new update the progressive list.</param>
        protected virtual void HandleDenominationChanged(object sender, ActivateThemeContextEventArgs eventArgs)
        {
            if(eventArgs != null && Denomination != eventArgs.ThemeContext.Denomination)
            {
                Denomination = eventArgs.ThemeContext.Denomination;
                if(GameProgressiveController != null)
                {
                    // Grab gcp progressives.
                    var gcpData = GameProgressiveController.GetAllProgressiveBroadcastData()
                                                           .ToDictionary(x => x.Key,
                                                                         x => (ProgressiveBroadcastData)x.Value.DeepClone());

                    // Grab game lib progressives and merge with gcp ones.
                    BroadcastDataList = GameLib.GetAvailableProgressiveBroadcastData(Denomination)
                                               .Concat(gcpData)
                                               .ToDictionary(x => x.Key, x => x.Value);
                }
                else
                {
                    // Grab only the game lib progressives and place them in the list.
                    BroadcastDataList =
                        GameLib.GetAvailableProgressiveBroadcastData(Denomination)
                               .ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }

        /// <summary>
        /// Event handler for AvailableDenominationsWithProgressivesBroadcastEvent.
        /// Cache the updated progressive broadcast data, and post the AsynchronousProviderChanged event
        /// to update the presentation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        protected virtual void HandleAvailableDenominationsWithProgressivesBroadcastEvent(object sender, DenominationsWithProgressivesBroadcastEventArgs eventArgs)
        {
            if(eventArgs != null)
            {
                var valueChanged = false;

                foreach(var denominationEntry in eventArgs.BroadcastData)
                {
                    var denomination = denominationEntry.Key;
                    var newData = denominationEntry.Value;

                    if(DenominationsBroadcastData.TryGetValue(denomination, out var existingData))
                    {
                        foreach(var levelEntry in newData)
                        {
                            valueChanged |= UpdateBroadcastDataIfChanged(existingData, levelEntry.Key, levelEntry.Value);
                        }
                    }
                    else
                    {
                        // This is just to cover the ground.  In production environment this should not happen.
                        // When the async update was turned on, all applicable denominations should already have
                        // been added by the initial query.
                        DenominationsBroadcastData.Add(denomination, newData);
                        valueChanged = true;
                    }
                }

                if(valueChanged)
                {
                    OnServiceUpdated(DenominationsBroadcastServiceNames, null);
                }
            }
        }

        /// <summary>
        /// Event handler for ProgressiveBroadcastEvent from GCP controller for sake of
        /// EnabledDenominationsWithProgressives service.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        protected virtual void HandleGcpUpdatesEnabledDenominationsWithProgressives(object sender, ProgressiveBroadcastEventArgs eventArgs)
        {
            if(eventArgs != null)
            {
                var valueChanged = false;

                if(DenominationsBroadcastData.TryGetValue(Denomination, out var existingData))
                {
                    foreach(var levelEntry in eventArgs.BroadcastDataList)
                    {
                        valueChanged |= UpdateBroadcastDataIfChanged(existingData, levelEntry.Key, levelEntry.Value);
                    }
                }
                else
                {
                    // This is just to cover the ground.  In production environment this should not happen.
                    // When the async update was turned on, all applicable denominations should already have
                    // been added by the initial query.
                    DenominationsBroadcastData.Add(Denomination, eventArgs.BroadcastDataList);
                    valueChanged = true;
                }

                if(valueChanged)
                {
                    OnServiceUpdated(DenominationsBroadcastServiceNames, null);
                }
            }
        }

        /// <summary>
        /// Gets the progressive broadcast data for enabled denominations, including
        /// both the system controlled progressives and GCP.
        /// </summary>
        /// <returns>
        /// The progressive broadcast data for enabled denominations.
        /// Each denomination has a dictionary of progressive data keyed by game levels.
        /// </returns>
        protected virtual IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> GetEnabledDenominationsWithProgressives()
        {
            var result = GameLib.GetAvailableDenominationsWithProgressives();

            // For GCP, there is no way to get the progressive broadcast data for
            // paytables that are not currently loaded, since we don't know which
            // paytable is associated with which denomination.  Therefore, we only
            // add GCP progressives for the current denomination/paytable.
            if(GameProgressiveController != null)
            {
                var gcpData = GameProgressiveController.GetAllProgressiveBroadcastData()
                                                       .ToDictionary(x => x.Key,
                                                                     x => (ProgressiveBroadcastData)x.Value.DeepClone());

                // If the current denomination has other progressives, merge the GCP data into it.
                if(result.ContainsKey(Denomination))
                {
                    result[Denomination] = result[Denomination]
                                           .Concat(gcpData)
                                           .ToDictionary(x => x.Key, x => x.Value);
                }
                // Otherwise, add a new entry for the current denomination.
                else
                {
                    result.Add(Denomination, gcpData);
                }
            }

            return result;
        }

        /// <summary>
        /// In the <paramref name="dataList"/>, updates the entry of <paramref name="key"/> with <paramref name="newData"/>.
        /// </summary>
        /// <param name="dataList">The data list that holds the data entry to update.</param>
        /// <param name="key">The key identifying the entry to update.</param>
        /// <param name="newData">The new data of the entry.</param>
        /// <returns>
        /// True if <paramref name="dataList"/> has changed in regards of ProgressiveBroadcastData.Amount; False otherwise.
        /// Returning true means that an AsynchronousProviderChanged event should be raised.
        /// </returns>
        protected static bool UpdateBroadcastDataIfChanged(IDictionary<int, ProgressiveBroadcastData> dataList,
                                                           int key,
                                                           ProgressiveBroadcastData newData)
        {
            var valueChanged = false;

            // Do not attempt to add a null value to data list
            if(newData != null)
            {
                var keyFound = dataList.ContainsKey(key);

                // If a piece of existing data is locked, its value is considered "not changed" since
                // its Amount value will remain the same.
                valueChanged = !keyFound || !dataList[key].IsLocked && dataList[key] != newData;

                // No mater whether "value" is considered changed or not, we always want to update
                // the data with the latest "actual amount".
                if(keyFound)
                {
                    // Update the existing instance if one presents.
                    dataList[key].Update(newData.Amount, newData.PrizeString);
                }
                else
                {   // Add a new entry if key not found.
                    dataList[key] = newData;
                }
            }

            return valueChanged;
        }

        /// <summary>
        /// Post the AsynchronousProviderChanged event when an asynchronous member is changed.
        /// </summary>
        /// <param name="serviceNames">Names of the services that have changed.</param>
        /// <param name="serviceArguments">Arguments to the changed service.</param>
        /// <remarks>
        /// 'serviceArguments' will be applied to each of the services in 'serviceNames'.
        /// </remarks>
        protected virtual void OnServiceUpdated(IList<string> serviceNames, IDictionary<string, object> serviceArguments)
        {
            var serviceSignatures = serviceNames.Select(service => new ServiceSignature(service, serviceArguments)).ToList();

            // Create a temporary copy of the event handler for thread safety
            var handler = AsynchronousProviderChanged;

            // If there are any handlers registered with this event
            // Post the event with this service provider as the sender and the parameter values passed in.
            handler?.Invoke(this, new AsynchronousProviderChangedEventArgs(serviceSignatures, false));
        }

        #endregion

        #region IGameLibEventListener Implementation

        /// <inheritdoc />
        public virtual void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            iGameLib.ProgressiveBroadcastEvent -= HandleProgressiveBroadcastEvent;
            iGameLib.ActivateThemeContextEvent -= HandleDenominationChanged;
            iGameLib.AvailableDenominationsWithProgressivesBroadcastEvent -= HandleAvailableDenominationsWithProgressivesBroadcastEvent;
        }

        #endregion
    }
}
