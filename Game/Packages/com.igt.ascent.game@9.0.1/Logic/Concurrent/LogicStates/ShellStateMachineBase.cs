// -----------------------------------------------------------------------
// <copyright file = "ShellStateMachineBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System.Diagnostics.CodeAnalysis;
    using Communication.Platform.ShellLib.Interfaces;
    using Interfaces;
    using ServiceProviders;

    /// <inheritdoc cref="IShellStateMachine"/>
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public abstract class ShellStateMachineBase : StateMachineBase<IShellState, IShellFrameworkInitialization, IShellFrameworkExecution>,
                                                  IShellStateMachine
    {
        #region Properties

        /// <summary>
        /// Gets the provider that holds the information of available cothemes.
        /// </summary>
        public SelectableThemesProvider SelectableThemesProvider { get; private set; }

        /// <summary>
        /// Gets the provider that holds the information of running cothemes.
        /// </summary>
        public RunningCothemesProvider RunningCothemesProvider { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// This allows the derived classes not have to call the other constructor.
        /// </summary>
        protected ShellStateMachineBase()
        {
        }

        /// <summary>
        /// Demonstration of how states are created and chained.
        /// Derived classes should NOT need to call this base class constructor
        /// since they usually have their own states setup.
        /// </summary>
        /// <param name="shellContext">
        /// The shell context for which the state machine is created.
        /// </param>
        protected ShellStateMachineBase(IShellContext shellContext)
        {
            // Instantiate and add states.
            var idleState = (BaseShellIdleState)AddState(new BaseShellIdleState());
            var themeLoadingState = (BaseShellThemeLoadingState)AddState(new BaseShellThemeLoadingState());

            InitialStateName = idleState.StateName;

            // State Transitions.
            idleState.NextOnLoading = themeLoadingState;
            themeLoadingState.NextOnLoadingComplete = idleState;
        }

        #endregion

        #region IShellStateMachine Implementation

        /// <inheritdoc/>
        public virtual void Initialize(IShellFrameworkInitialization framework)
        {
            var configurationProvider = new ShellConfigurationProvider(framework.ShellLib);
            framework.ServiceController.AddProvider(configurationProvider, configurationProvider.Name);

            var displayControlStateProvider = new DisplayControlStateProvider(framework.ObservableCollection.ObservableDisplayControlState);
            framework.ServiceController.AddProvider(displayControlStateProvider, displayControlStateProvider.Name);

            var gamingMetersProvider = new GamingMetersProvider(framework.ObservableCollection.ObservableGamingMeters);
            framework.ServiceController.AddProvider(gamingMetersProvider, gamingMetersProvider.Name);

            SelectableThemesProvider = new SelectableThemesProvider();
            framework.ServiceController.AddProvider(SelectableThemesProvider, SelectableThemesProvider.Name);

            RunningCothemesProvider = new RunningCothemesProvider();
            framework.ServiceController.AddProvider(RunningCothemesProvider, RunningCothemesProvider.Name);

            var chooserServicesProvider = new ChooserServicesProvider(framework.ObservableCollection.ObservableChooserProperties);
            framework.ServiceController.AddProvider(chooserServicesProvider, chooserServicesProvider.Name);

            var bankPlayPropertiesProvider = new BankPlayPropertiesProvider(framework.ObservableCollection.ObservableBankPlayProperties);
            framework.ServiceController.AddProvider(bankPlayPropertiesProvider, bankPlayPropertiesProvider.Name);

            var gameInProgressProvider = new GameInProgressProvider(framework.ObservableCollection.ObservableGameInProgress);
            framework.ServiceController.AddProvider(gameInProgressProvider, gameInProgressProvider.Name);

            var cultureProvider = new CultureProvider(framework.ObservableCollection.ObservableCulture);
            framework.ServiceController.AddProvider(cultureProvider, cultureProvider.Name);
        }

        /// <inheritdoc/>
        public virtual void ReadConfiguration(IShellLib shellLib)
        {
        }

        /// <inheritdoc/>
        public abstract void CleanUp(IShellFrameworkInitialization framework);

        #endregion
    }
}