// -----------------------------------------------------------------------
// <copyright file = "CoplayerConfigurationProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Logic.Services;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A provider that provides the config data of a Coplayer.
    /// </summary>
    /// <remarks>
    /// As config data never changes during an activate context, services provided here are all Synchronous Game Services.
    /// </remarks>
    /// <devdoc>
    /// NO Asynchronous Game Service should be provided here.
    /// DO NOT subscribe to any Coplayer Lib events in this provider!
    /// </devdoc>
    public class CoplayerConfigurationProvider : NonObserverProviderBase
    {
        #region Constants

        private const string DefaultName = nameof(CoplayerConfigurationProvider);

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the denomination of the coplayer.
        /// </summary>
        [GameService]
        public long Denomination { get; }

        /// <summary>
        /// Gets the credit formatter.
        /// </summary>
        [GameService]
        public CreditFormatter CreditFormatter { get; }

        /// <summary>
        /// Gets the minimum game presentation time in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        [GameService]
        public int MinimumBaseGameTime { get; }

        /// <summary>
        /// Gets the minimum time for a single slot free spin cycle in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        [GameService]
        public int MinimumFreeSpinTime { get; }

        /// <summary>
        /// Gets the behavior that the credit meter should exhibit.
        /// </summary>
        [GameService]
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; }

        /// <summary>
        /// Gets the max bet button behavior.
        /// </summary>
        [GameService]
        public MaxBetButtonBehavior MaxBetButtonBehavior { get; }

        /// <summary>
        /// Gets the boolean flag that indicates whether a video reel presentation should be displayed for a stepper game.
        /// </summary>
        [GameService]
        public bool DisplayVideoReelsForStepper { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerConfigurationProvider"/>.
        /// </summary>
        /// <param name="coplayerLib">
        /// The coplayer lib that provides config data for the GameServices in this provider.
        /// </param>
        /// <param name="shellExposition">
        /// The interface that provides access to config data which are exposed by the Shell.
        /// </param>
        /// <param name="name">
        /// The name of the service provider.
        /// This parameter is optional.  If not specified, the provider name will be set to <see cref="DefaultName"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="coplayerLib"/> or <paramref name="shellExposition"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the context provided by <paramref name="coplayerLib"/> is null.
        /// </exception>
        public CoplayerConfigurationProvider(ICoplayerLib coplayerLib, IShellExposition shellExposition, string name = DefaultName)
            : base(name)
        {
            if(coplayerLib == null)
            {
                throw new ArgumentNullException(nameof(coplayerLib));
            }

            if(coplayerLib.Context == null)
            {
                throw new ArgumentException("The context provided by coplayerLib is null.", nameof(coplayerLib));
            }

            if(shellExposition == null)
            {
                throw new ArgumentNullException(nameof(shellExposition));
            }

            Denomination = coplayerLib.Context.Denomination;

            CreditFormatter = shellExposition.CreditFormatter;
            MinimumBaseGameTime = shellExposition.MinimumBaseGameTime;
            MinimumFreeSpinTime = shellExposition.MinimumFreeSpinTime;
            CreditMeterBehavior = shellExposition.CreditMeterBehavior;
            MaxBetButtonBehavior = shellExposition.MaxBetButtonBehavior;
            DisplayVideoReelsForStepper = shellExposition.DisplayVideoReelsForStepper;
        }

        #endregion
    }
}