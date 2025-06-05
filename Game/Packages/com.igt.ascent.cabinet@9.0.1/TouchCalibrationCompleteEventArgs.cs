// -----------------------------------------------------------------------
// <copyright file = "TouchCalibrationCompleteEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating touch calibration has completed for a display.
    /// </summary>
    public class TouchCalibrationCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="TouchCalibrationCompleteEventArgs"/>.
        /// </summary>
        /// <param name="displayIndex">
        /// The display index that has completed touch calibration.
        /// </param>
        public TouchCalibrationCompleteEventArgs(ushort displayIndex)
        {
            DisplayIndex = displayIndex;
        }

        /// <summary>
        /// Gets the display index that has completed touch calibration.
        /// </summary>
        public ushort DisplayIndex { get; }
    }
}
