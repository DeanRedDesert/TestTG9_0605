// -----------------------------------------------------------------------
// <copyright file = "ShellObservableCollection.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Interfaces;
    using ObserverPattern;

    /// <inheritdoc cref="IShellObservableCollection"/>
    internal sealed class ShellObservableCollection : ObservableCollectionBase, IShellObservableCollection
    {
        #region Private Fields

        private readonly IShellLib shellLib;
        private readonly IShellLibRestricted shellLibRestricted;

        private Observable<DisplayControlState> observableDisplayControlState;
        private Observable<GamingMeters> observableGamingMeters;
        private Observable<ChooserProperties> observableChooserProperties;
        private Observable<BankPlayProperties> observableBankPlayProperties;
        private Observable<bool> observableGameInProgress;
        private Observable<string> observableCulture;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellObservableCollection"/>
        /// that only instantiates observables but does not provide any functionality.
        /// Designed to be used by LDC.
        /// </summary>
        public ShellObservableCollection()
        {
            CreateObservables();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShellObservableCollection"/>.
        /// </summary>
        /// <param name="shellLib">The reference to the shell lib interface.</param>
        /// <param name="shellLibRestricted">The reference to the shell lib restricted interface.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="shellLib"/> or <paramref name="shellLibRestricted"/> is null.
        /// </exception>
        public ShellObservableCollection(IShellLib shellLib, IShellLibRestricted shellLibRestricted)
        {
            this.shellLib = shellLib ?? throw new ArgumentNullException(nameof(shellLib));
            this.shellLibRestricted = shellLibRestricted ?? throw new ArgumentNullException(nameof(shellLibRestricted));

            CreateObservables();
            SubscribeEventHandlers();

            // Set initial values of the observables.
            observableDisplayControlState.SetValue(shellLibRestricted.DisplayControlState);
            observableGamingMeters.SetValue(shellLib.BankPlay.GamingMeters);
            observableChooserProperties.SetValue(shellLib.ChooserServices.ChooserProperties);
            observableBankPlayProperties.SetValue(shellLib.BankPlay.BankPlayProperties);
            observableGameInProgress.SetValue(shellLib.GamePlayStatus.GameInProgress);
            observableCulture.SetValue(shellLib.GameCulture.Culture);
        }

        #endregion

        #region IShellObservableCollection Implementation

        /// <inheritdoc/>
        public IObservable<DisplayControlState> ObservableDisplayControlState => observableDisplayControlState;

        /// <inheritdoc/>
        public IObservable<GamingMeters> ObservableGamingMeters => observableGamingMeters;

        /// <inheritdoc/>
        public IObservable<ChooserProperties> ObservableChooserProperties => observableChooserProperties;

        /// <inheritdoc/>
        public IObservable<BankPlayProperties> ObservableBankPlayProperties => observableBankPlayProperties;

        /// <inheritdoc/>
        public IObservable<bool> ObservableGameInProgress => observableGameInProgress;

        /// <inheritdoc />
        public IObservable<string> ObservableCulture => observableCulture;

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override void Dismiss()
        {
            UnsubscribeEventHandlers();
            DismissObservables();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates all the shell driven observables.
        /// </summary>
        private void CreateObservables()
        {
            // Observables driven by Shell Lib.
            observableDisplayControlState = new Observable<DisplayControlState>();
            observableGamingMeters = new Observable<GamingMeters>();
            observableChooserProperties = new Observable<ChooserProperties>();
            observableBankPlayProperties = new Observable<BankPlayProperties>();
            observableGameInProgress = new Observable<bool>();
            observableCulture = new Observable<string>();
        }

        /// <summary>
        /// Dismisses all the shell driving observables.
        /// </summary>
        private void DismissObservables()
        {
            // Observables driven by Shell Lib.
            observableDisplayControlState.Dismiss();
            observableGamingMeters.Dismiss();
            observableChooserProperties.Dismiss();
            observableBankPlayProperties.Dismiss();
            observableGameInProgress.Dismiss();
            observableCulture.Dismiss();
        }

        /// <summary>
        /// Subscribe to all necessary ShellLib events (money, chooser, display, etc).
        /// </summary>
        private void SubscribeEventHandlers()
        {
            if(shellLib == null || shellLibRestricted == null)
            {
                return;
            }

            shellLibRestricted.DisplayControlEvent += HandleDisplayControl;

            shellLib.BankPlay.MoneyChangedEvent += HandleMoneyChanged;
            shellLib.BankPlay.BankPlayPropertiesUpdateEvent += HandleBankPlayPropertiesUpdate;

            shellLib.ChooserServices.ChooserPropertiesUpdateEvent += HandleChooserPropertiesUpdateEvent;

            shellLib.GamePlayStatus.GameInProgressChangedEvent += HandleGameInProgressChanged;

            shellLib.GameCulture.CultureChangedEvent += HandleCultureChanged;
        }

        /// <summary>
        /// Unsubscribe from all events that the ShellLib has utilized.
        /// </summary>
        private void UnsubscribeEventHandlers()
        {
            if(shellLib == null || shellLibRestricted == null)
            {
                return;
            }

            shellLibRestricted.DisplayControlEvent -= HandleDisplayControl;

            shellLib.BankPlay.MoneyChangedEvent -= HandleMoneyChanged;
            shellLib.BankPlay.BankPlayPropertiesUpdateEvent -= HandleBankPlayPropertiesUpdate;

            shellLib.ChooserServices.ChooserPropertiesUpdateEvent -= HandleChooserPropertiesUpdateEvent;

            shellLib.GamePlayStatus.GameInProgressChangedEvent -= HandleGameInProgressChanged;

            shellLib.GameCulture.CultureChangedEvent -= HandleCultureChanged;
        }

        /// <summary>
        /// Sets the value of the observable <see cref="DisplayControlState"/> based on the given event args.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="eventArgs">The updated <see cref="DisplayControlState"/>.</param>
        private void HandleDisplayControl(object sender, DisplayControlEventArgs eventArgs)
        {
            observableDisplayControlState.SetValue(shellLibRestricted.DisplayControlState);
        }

        /// <summary>
        /// Sets the value of the observable <see cref="GamingMeters"/> based on the given event args.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="eventArgs">The updated <see cref="GamingMeters"/>.</param>
        private void HandleMoneyChanged(object sender, MoneyChangedEventArgs eventArgs)
        {
            observableGamingMeters.SetValue(shellLib.BankPlay.GamingMeters);
        }

        /// <summary>
        /// Sets the value of the observable <see cref="ChooserProperties"/> based on the given event args.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="eventArgs">The updated <see cref="ChooserProperties"/>.</param>
        private void HandleChooserPropertiesUpdateEvent(object sender, ChooserPropertiesUpdateEventArgs eventArgs)
        {
            observableChooserProperties.SetValue(shellLib.ChooserServices.ChooserProperties);
        }

        /// <summary>
        /// Sets the value of the observable <see cref="BankPlayProperties"/> based on the given event args.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="eventArgs">The updated <see cref="BankPlayProperties"/>.</param>
        private void HandleBankPlayPropertiesUpdate(object sender, BankPlayPropertiesUpdateEventArgs eventArgs)
        {
            observableBankPlayProperties.SetValue(shellLib.BankPlay.BankPlayProperties);
        }

        /// <summary>
        /// Sets the value of the observable game-in-progress flag based on the given event args.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">The updated game in progress flag.</param>
        private void HandleGameInProgressChanged(object sender, GameInProgressChangedEventArgs eventArgs)
        {
            observableGameInProgress.SetValue(shellLib.GamePlayStatus.GameInProgress);
        }

        /// <summary>
        /// Sets the value of the observable culture string based on the given event args.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">The updated culture string.</param>
        private void HandleCultureChanged(object sender, CultureChangedEventArgs eventArgs)
        {
            observableCulture.SetValue(shellLib.GameCulture.Culture);
        }

        #endregion
    }
}