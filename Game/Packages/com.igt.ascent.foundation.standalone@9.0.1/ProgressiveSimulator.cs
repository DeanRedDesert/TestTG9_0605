//-----------------------------------------------------------------------
// <copyright file = "ProgressiveSimulator.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Timers = System.Timers;
    using Foundation;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// A class to simulate networked progressive contributions in the background (should only be used when
    /// running in Unity editor mode.)
    /// </summary>
    internal class ProgressiveSimulator : IProgressiveSimulator, IDisposable
    {
        #region Private fields

        /// <summary>
        /// Contribution credit.
        /// </summary>
        private readonly int contribution;

        /// <summary>
        /// Contribution frequency and limits, in ms. The incoming user specified frequencies will be noted in seconds,
        /// so conversions are necessary. The limits below are from various studios providing feedback as to what
        /// frequency range is most useful.
        /// </summary>
        private const int ContributionFrequencyMinLimit = 1000;
        private const int ContributionFrequencyMaxLimit = 120000;

        /// <summary>
        /// Flags to indicate enabled and initialized states.
        /// </summary>
        private bool enabled;
        private bool initialized;

        /// <summary>
        /// The timer that drives the contributions.
        /// </summary>
        private Timers.Timer timer;

        // The various interfaces need by this controller. They are assigned from the single IStateMachineFramework
        // instance passed in.
        private readonly IStandaloneProgressiveManagerDependency progressiveManager;
        private readonly ITransactionAugmenter transactionAugmenter;

        #endregion

        #region NetworkProgressiveSimulator implementation

        /// <inheritdoc />
        public void Start()
        {
            enabled = true;
            if(initialized)
            {
                timer.Start();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            enabled = false;
            if(initialized)
            {
                timer.Stop();
            }
        }

        #region Constructors

        ///  <summary>
        ///  Construct an instance of <see cref="ProgressiveSimulator"/> with the supplied <see cref="IGameLib"/>
        ///  instance.
        ///  </summary>
        ///  <param name="gameLib">
        ///      The <see cref="IGameLib"/> interface. Must support the <see cref="IGameLibRestricted"/> interface.
        ///  </param>
        ///  <param name="progressiveManager">
        ///      An instance of <see cref="IStandaloneProgressiveManagerDependency"/> passed in from game lib.
        ///  </param>
        ///  <param name="progressiveSimulatorParser">
        ///      An instance of <see cref="ProgressiveSimulatorParser"/> which parses out config values.
        ///  </param>
        ///  <exception cref="ArgumentException">
        ///     Thrown if any of the required interfaces passed in are null.
        /// </exception>
        public ProgressiveSimulator(IGameLib gameLib,
                                    IStandaloneProgressiveManagerDependency progressiveManager,
                                    ProgressiveSimulatorParser progressiveSimulatorParser)
        {
            if(gameLib == null)
            {
                throw new ArgumentException("The passed in " + nameof(gameLib) + " interface is null.");
            }

            if(!(gameLib is IGameLibRestricted gameLibRestricted))
            {
                throw new ArgumentException("The passed in " + nameof(gameLib) + " interface does not support " + nameof(IGameLibRestricted) + ".");
            }

            this.progressiveManager = progressiveManager ??
                throw new ArgumentException("The passed in " + nameof(progressiveManager) + " interface is null.");

            transactionAugmenter = gameLibRestricted.GetServiceInterface<ITransactionAugmenter>();

            if(transactionAugmenter == null)
            {
                throw new ArgumentException("The passed in " + nameof(gameLib)+ " interface does not support an " +
                                                     nameof(ITransactionAugmenter) + " interface.");
            }

            contribution = progressiveSimulatorParser.Credits;

            // The passed in frequency parameter from the user created configuration file is in seconds, convert to ms.
            var contributionFrequency = progressiveSimulatorParser.ContributionFrequency * 1000;

            contributionFrequency = contributionFrequency < ContributionFrequencyMinLimit
                ? ContributionFrequencyMinLimit
                : contributionFrequency;

            contributionFrequency = contributionFrequency > ContributionFrequencyMaxLimit
                ? ContributionFrequencyMaxLimit
                : contributionFrequency;

            Initialize(contributionFrequency);
        }

        /// <summary>
        /// The progressive timer tick event handler.
        /// </summary>
        /// <param name="state">The state of this event.</param>
        /// <param name="eventArgs">The payload of this event.</param>
        private void HandleProgressiveTimer(object state, Timers.ElapsedEventArgs eventArgs)
        {
            if(enabled)
            {
                ContributeToProgressives(false);
            }
        }

        /// <summary>
        /// Handles the TransactionClosingEvent when the supplied game lib raises it. This is used
        /// mainly to flush progressive data to safe storage.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="eventArgs">The payload of this event.</param>
        private void HandleTransactionClosingEvent(object sender, EventArgs eventArgs)
        {
            if(enabled)
            {
                ContributeToProgressives(true);
            }
        }

        /// <summary>
        /// The method that contributes to progressives.
        /// </summary>
        /// <param name="saveToCriticalData">
        /// A flag indicating whether the current progressive value should be persisted to critical data.
        /// Note that only the thread used when the <see cref="HandleTransactionClosingEvent"/> is raised
        /// should write to critical data, as an open, valid transaction will be open. The timer thread should
        /// not write to critical data, as there is no guarantee that there is a transaction open, and critical
        /// data access may interfere with the logic thread's critical data access.
        /// </param>
        private void ContributeToProgressives(bool saveToCriticalData)
        {
            if(progressiveManager != null)
            {
                // These two methods are thread-safe.
                // The denom is set to 1 as the underlying progressive mappings will determine which levels are contributed to
                // and this will determine the denomination used.
                progressiveManager.ContributeToAllProgressives(contribution, 1, saveToCriticalData);
                progressiveManager.ContributeToAllEventBasedProgressives(contribution, 1, saveToCriticalData);
            }
        }

        #endregion

        /// <summary>
        /// Creates the timer, and registers event handlers for transaction augmentation.
        /// </summary>
        private void Initialize(double interval)
        {
            transactionAugmenter.TransactionClosingEvent += HandleTransactionClosingEvent;
            timer = new Timers.Timer(interval);
            timer.Elapsed += HandleProgressiveTimer;
            initialized = true;
            enabled = false;
        }

        /// <summary>
        /// Shuts down and cleans up timers and event handlers.
        /// </summary>
        private void Deinitialize()
        {
            enabled = false;
            initialized = false;

            if(timer != null)
            {
                timer.Stop();
                timer.Elapsed -= HandleProgressiveTimer;
                timer = null;
            }

            if(transactionAugmenter != null)
            {
                transactionAugmenter.TransactionClosingEvent -= HandleTransactionClosingEvent;
            }
        }

        #endregion

        #region IDisposable implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposeInternal(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Handle cleanup.
        /// </summary>
        ~ProgressiveSimulator()
        {
            DisposeInternal(false);
        }

        /// <summary>
        /// Handle internal dispose if necessary.
        /// </summary>
        /// <param name="disposing"></param>
        private void DisposeInternal(bool disposing)
        {
            if(disposing)
            {
                Deinitialize();
                timer?.Stop();
                timer?.Dispose();
            }
        }

        #endregion
    }
}
