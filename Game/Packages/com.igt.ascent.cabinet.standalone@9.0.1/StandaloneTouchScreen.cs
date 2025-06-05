//-----------------------------------------------------------------------
// <copyright file = "StandaloneTouchScreen.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Provide a virtual implementation of the touch screen category.
    /// </summary>
    internal class StandaloneTouchScreen : ITouchScreen, ICabinetUpdate
    {
        private readonly List<Action> pendingEvents = new List<Action>();
        private IEnumerable<TouchScreenInfo> touchScreenInfo = new List<TouchScreenInfo>();

        #region ITouchScreen Implementation

        /// <inheritdoc/> 
        public event EventHandler<TouchScreenInfoEventArgs> TouchScreenInfoEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchDisplayTargetEventArgs> TouchDisplayTargetEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchCalibrationCompleteEventArgs> TouchCalibrationCompleteEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchScreenConnectionChangedEventArgs> TouchScreenConnectionChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchScreenExclusiveModeChangedEventArgs> TouchScreenExclusiveModeChangedEvent;

        /// <inheritdoc/>
        public IEnumerable<TouchScreenInfo> GetTouchScreenInfo()
        {
            return touchScreenInfo;
        }

        /// <ineritdoc/>
        public void RequestMinimumDigitizerDebounceIntervalForDpp()
        {
        }

        /// <ineritdoc/>
        public void ResetDigitizerDebounceIntervalForDpp()
        {
        }

        /// <ineritdoc/>
        public void SetDigitizerToMinimumDebounce(DigitizerRole role)
        {
        }

        /// <ineritdoc/>
        public void ResetDigitizerDebounceInterval(DigitizerRole role)
        {
        }

        /// <ineritdoc/>
        public void SetDigitizerExclusiveMode(DigitizerRole role, bool exclusive)
        {
        }

        #endregion

        #region Implementation of ICabinetUpdate

        /// <inheritdoc/>
        public void Update()
        {
            foreach(var pendingEvent in pendingEvents)
            {
                pendingEvent();
            }

            pendingEvents.Clear();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the current touch screen info.
        /// </summary>
        /// <param name="touchScreenInfoList">List of touch screen info objects.</param>
        internal void SetTouchScreenInfo(List<TouchScreenInfo> touchScreenInfoList)
        {
            touchScreenInfo = touchScreenInfoList;
            pendingEvents.Add(() => RaiseEvent(TouchScreenInfoEvent,
                                               new TouchScreenInfoEventArgs(touchScreenInfoList)));
        }

        /// <summary>
        /// Simulates a touch display target event.
        /// </summary>
        /// <param name="x">
        /// The x-coordinate for the calibration target location.
        /// </param>
        /// <param name="y">
        /// The y-coordinate for the calibration target location.
        /// </param>
        /// <param name="displayIndex">
        /// The display index that the touch coordinate is displayed on.
        /// </param>
        /// <param name="isOffScreen">
        /// Whether the target location would be off the screen (i.e. extended touch screen calibration).
        /// </param>
        internal void SimulateTouchDisplayTargetEvent(float x, float y, ushort displayIndex, bool isOffScreen)
        {
            pendingEvents.Add(() => RaiseEvent(TouchDisplayTargetEvent,
                                               new TouchDisplayTargetEventArgs(x, y, displayIndex, isOffScreen)));
        }

        /// <summary>
        /// Simulates a touch calibration complete event.
        /// </summary>
        /// <param name="displayIndex">
        /// The display index that has completed touch calibration.
        /// </param>
        internal void SimulateTouchCalibrationCompleteEvent(ushort displayIndex)
        {
            pendingEvents.Add(() => RaiseEvent(TouchCalibrationCompleteEvent,
                                               new TouchCalibrationCompleteEventArgs(displayIndex)));
        }

        /// <summary>
        /// Simulates a touch screen connection changed event.
        /// </summary>
        /// <param name="deviceId">The id of the touch screen that was connected or disconnected.</param>
        /// <param name="driver">Driver identifier for the device.</param>
        /// <param name="driverSubClass">Sub-class identifier for the device.</param>
        /// <param name="connected">Whether the device has been connected or disconnected.</param>
        internal void SimulateTouchScreenConnectionChangedEvent(uint deviceId, ushort driver, ushort driverSubClass,
                                                                bool connected)
        {
            pendingEvents.Add(() => RaiseEvent(TouchScreenConnectionChangedEvent,
                                               new TouchScreenConnectionChangedEventArgs(deviceId, driver,
                                                                                         driverSubClass, connected)));
        }

        /// <summary>
        /// Simulates a touch screen exclusive mode changed event
        /// </summary>
        /// <param name="digitizerRole">
        /// The digitizer role that has entered or exited exclusive mode.
        /// </param>
        /// <param name="exclusive">
        /// True if the associated digitizer is now in exclusive mode.
        /// </param>
        internal void SimulateTouchScreenExclusiveModeChangedEvent(DigitizerRole digitizerRole, bool exclusive)
        {
            pendingEvents.Add(() => RaiseEvent(TouchScreenExclusiveModeChangedEvent, new TouchScreenExclusiveModeChangedEventArgs(digitizerRole, exclusive)));                                               
        }

        #endregion

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of event arguments.</typeparam>
        /// <param name="eventHandler">Event handler to raise.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void RaiseEvent<TEventArgs>(EventHandler<TEventArgs> eventHandler,
                                            TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(this, eventArgs);
        }
    }
}
