// -----------------------------------------------------------------------
// <copyright file = "ITouchScreenService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Cabinet
{
    using CabinetServices;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// The cabinet service that provides access to touch screen functionalities.
    /// </summary>
    public interface ITouchScreenService : ICabinetService
    {
        /// <summary>
        /// Sets the specified digitizer to the minimum debounce interval.
        /// </summary>
        /// <param name="role">The role of the digitizer.</param>
        void SetDigitizerToMinimumDebounce(DigitizerRole role);

        /// <summary>
        /// Resets the specified digitizer to the default debounce interval.
        /// </summary>
        /// <param name="role">The role of the digitizer.</param>
        void ResetDigitizerDebounceInterval(DigitizerRole role);

        /// <summary>
        /// Sets the specified digitizer to enter or exit exclusive mode.
        /// When in exclusive mode, the touch events will be restricted to the client owned windows.
        /// </summary>
        /// <remarks>
        /// The appropriate touchscreen CSI resource must be acquired in order for a digitizer to enter exclusive mode.
        /// Also note that exclusive mode will be disabled when the touchscreen resource is acquired.
        /// Should the CSI client lose ownership of the touchscreen resource, it is its responsibility to re-enable
        /// exclusive mode on any digitizers it desires to run in exclusive mode once resource ownership is returned.
        /// </remarks>
        /// <param name="role">The role of the digitizer.</param>
        /// <param name="exclusive">
        /// True for the digitizer to enter exclusive mode, restricting touch events to client owned windows.
        /// Or false for the default shared behavior.
        /// </param>
        void SetDigitizerExclusiveMode(DigitizerRole role, bool exclusive);
    }
}