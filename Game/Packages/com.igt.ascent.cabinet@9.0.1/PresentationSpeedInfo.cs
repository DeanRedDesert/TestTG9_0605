// -----------------------------------------------------------------------
// <copyright file = "PresentationSpeedInfo.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// The default value and the current value of the presentation speed.
    /// </summary>
    public struct PresentationSpeedInfo
    {
        /// <summary>
        /// Gets the default value of the presentation speed. The speed of the presentation is restricted between 0
        /// and 100. 100 indicates the maximum possible speed, while 0 the minimum possible speed.
        /// </summary>
        public uint DefaultPresentationSpeed { get; }

        /// <summary>
        /// Gets the current value of the presentation speed. The speed of the presentation is restricted
        /// between 0 and 100. 100 indicates the maximum possible speed, while 0 the minimum possible speed.
        /// </summary>
        public uint PresentationSpeed { get; }

        /// <summary>
        /// The constructor for the <see cref="PresentationSpeedInfo"/>.
        /// </summary>
        /// <param name="defaultSpeed">The default value of the presentation speed.</param>
        /// <param name="currentSpeed">The current value of the presentation speed.</param>
        /// <remarks>The speed of the presentation is restricted between 0 and 100.</remarks>
        public PresentationSpeedInfo(uint defaultSpeed, uint currentSpeed) : this()
        {
            DefaultPresentationSpeed = Math.Min(defaultSpeed, 100);
            PresentationSpeed = Math.Min(currentSpeed, 100);
        }
    }
}