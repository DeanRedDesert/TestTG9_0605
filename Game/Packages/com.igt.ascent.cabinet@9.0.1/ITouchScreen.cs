//-----------------------------------------------------------------------
// <copyright file = "ITouchScreen.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Interface for TouchScreen related functionality of the cabinet.
    /// </summary>
    public interface ITouchScreen
    {
        /// <summary>
        /// Event that is raised when touch screen information changes.
        /// </summary>
        event EventHandler<TouchScreenInfoEventArgs> TouchScreenInfoEvent;

        /// <summary>
        /// Event that is raised when a touch display is targeted during calibration.
        /// </summary>
        event EventHandler<TouchDisplayTargetEventArgs> TouchDisplayTargetEvent;

        /// <summary>
        /// Event that is raised when touch calibration has completed for a display.
        /// </summary>
        event EventHandler<TouchCalibrationCompleteEventArgs> TouchCalibrationCompleteEvent;

        /// <summary>
        /// Event that is raised when a touch screen device has been connected or disconnected.
        /// </summary>
        event EventHandler<TouchScreenConnectionChangedEventArgs> TouchScreenConnectionChangedEvent;

        /// <summary>
        /// Event that is raised when a touch screen enters or exits exclusive mode.
        /// Currently, this event is only invoked in response to calling SetDigitizerExclusiveMode,
        /// as there is no use case with respect to the Ascent Foundation in which exclusive mode will
        /// be force entered or exited while the CSI client has ownership of the touchscreen resource.
        /// </summary>
        event EventHandler<TouchScreenExclusiveModeChangedEventArgs> TouchScreenExclusiveModeChangedEvent;

        /// <summary>
        /// Request information about the connected touch screens.
        /// </summary>
        /// <returns>
        /// A collection of connected touch screen devices which will be null or empty if no
        /// touch screen device is connected.
        /// </returns>
        IEnumerable<TouchScreenInfo> GetTouchScreenInfo();

        /// <summary>
        /// Request the minimum software debounce interval for the DPP's digitizer.
        /// </summary>
        void RequestMinimumDigitizerDebounceIntervalForDpp();

        /// <summary>
        /// Reset the software debounce interval for the DPP's digitizer.
        /// </summary>
        void ResetDigitizerDebounceIntervalForDpp();

        /// <summary>
        /// Set the specified digitizer to the minimum debounce interval.
        /// </summary>
        /// <param name="role">The role of the digitizer.</param>
        void SetDigitizerToMinimumDebounce(DigitizerRole role);

        /// <summary>
        /// Reset the specified digitizer to the default debounce interval.
        /// </summary>
        /// <param name="role">The role of the digitizer.</param>
        void ResetDigitizerDebounceInterval(DigitizerRole role);

        /// <summary>
        /// Set digitizers into an exclusive mode, restricting touch events to client owned windows.
        /// The result of the request can be monitored by subscribing to the <see cref="TouchScreenExclusiveModeChangedEvent"/>.
        /// </summary>
        /// <param name="role">The role of the digitizer.</param>
        /// <param name="exclusive">True for the digitizer to enter exclusive mode, restricting touch events to client owned windows.
        /// Or false for the default shared behavior.</param>
        /// <remarks>The appropriate touchscreen CSI resource must be acquired in order for a digitizer to enter exclusive mode.
        /// Also note that exclusive mode will be disabled when the touchscreen resource is acquired. Should the CSI client lose
        /// ownership of the touchscreen resource, it is its responsibility to re-enable exclusive mode on any digitizers it
        /// desires to run in exclusive mode once resource ownership is returned.
        /// </remarks>
        void SetDigitizerExclusiveMode(DigitizerRole role, bool exclusive);
    }
}
