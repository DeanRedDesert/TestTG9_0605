// -----------------------------------------------------------------------
// <copyright file = "TouchScreenExclusiveModeChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating a touchscreen has entered or exited exclusive mode.
    /// </summary>
    public class TouchScreenExclusiveModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="TouchCalibrationCompleteEventArgs"/>.
        /// </summary>
        /// <param name="digitizerRole">
        /// The digitizer role that has entered or exited exclusive mode.
        /// </param>
        /// <param name="exclusive">
        /// True if the associated digitizer is now in exclusive mode.
        /// </param>
        public TouchScreenExclusiveModeChangedEventArgs(DigitizerRole digitizerRole, bool exclusive)
        {
            DigitizerRole = digitizerRole;
            Exclusive = exclusive;
        }

        /// <summary>
        /// Gets the digitizer role that has entered or exited exclusive mode.
        /// </summary>
        public DigitizerRole DigitizerRole { get; }

        /// <summary>
        /// Gets whether the associated digitizer is in exclusive mode.
        /// </summary>
        public bool Exclusive { get; }
    }
}
