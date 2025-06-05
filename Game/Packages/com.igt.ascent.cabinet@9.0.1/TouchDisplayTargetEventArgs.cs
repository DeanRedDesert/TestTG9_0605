// -----------------------------------------------------------------------
// <copyright file = "TouchDisplayTargetEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating a touch display was targeted during calibration.
    /// </summary>
    public class TouchDisplayTargetEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TouchDisplayTargetEventArgs"/>.
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
        public TouchDisplayTargetEventArgs(float x, float y, ushort displayIndex, bool isOffScreen)
        {
            X = x;
            Y = y;
            DisplayIndex = displayIndex;
            IsOffScreen = isOffScreen;
        }

        /// <summary>
        /// Gets the x-coordinate for the calibration target location.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the y-coordinate for the calibration target location.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the display index that the touch coordinate is displayed on.
        /// </summary>
        public ushort DisplayIndex { get; }

        /// <summary>
        /// Gets whether the target location would be off the screen 
        /// (i.e. extended touch screen calibration). 
        /// </summary>
        public bool IsOffScreen { get; }
    }
}
