//-----------------------------------------------------------------------
// <copyright file = "StandaloneInterfaceExtensionDependencies.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Class for managing dependencies needed by standalone extension implementations.
    /// </summary>
    public class StandaloneInterfaceExtensionDependencies : InterfaceExtensionDependencies
    {
        /// <summary>
        /// Interface for accessing game information.
        /// </summary>
        public IStandaloneGameInformationDependency GameInformation { get; set; }

        /// <summary>
        /// Interface for accessing critical data as if it were the foundation.
        /// </summary>
        public IStandaloneCriticalDataDependency FoundationCriticalDataProvider { get; set; }

        /// <summary>
        /// An interface for querying the current Foundation game cycle state.
        /// </summary>
        public IGameCycleStateQuery GameCycleStateQuery { get; set; }

        /// <summary>
        /// An interface for posting transactional foundation events in the main event queue.
        /// </summary>
        public IStandaloneEventPosterDependency EventPoster { get; set; }

        /// <summary>
        /// An interface for extending the outcome adjustment routine.
        /// </summary>
        public IStandaloneOutcomeAdjusterDependency OutcomeAdjuster { get; set; }

        /// <summary>
        /// An interface for providing specific registry values.
        /// </summary>
        public IStandaloneRegistryInformationDependency RegistryInformation { get; set; }

        /// <summary>
        /// An interface for providing progressive settings.
        /// </summary>
        public IStandaloneProgressiveManagerDependency ProgressiveManager { get; set; }

        /// <summary>
        /// An interface for providing play status information.
        /// </summary>
        public IStandalonePlayStatusDependency PlayStatus { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneInterfaceExtensionDependencies"/> with
        /// default values.
        /// </summary>
        public StandaloneInterfaceExtensionDependencies()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneInterfaceExtensionDependencies"/>.
        /// </summary>
        /// <param name="baseDependencies">
        /// A dependencies object that fulfills the common dependency requirements.
        /// </param>
        public StandaloneInterfaceExtensionDependencies(IInterfaceExtensionDependencies baseDependencies)
            : base(baseDependencies)
        {
        }
    }
}
