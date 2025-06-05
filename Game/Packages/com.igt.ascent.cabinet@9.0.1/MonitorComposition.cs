//-----------------------------------------------------------------------
// <copyright file = "MonitorComposition.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Class that represents the monitor composition including the desktop rectangle.
    /// </summary>
    public class MonitorComposition
    {
        /// <summary>
        /// List of available monitors and their specifications.
        /// </summary>
        public IEnumerable<Monitor> Monitors { get; }

        /// <summary>
        /// Definition of the desktop rectangle. The rectangle may span monitors.
        /// </summary>
        public DesktopRectangle Desktop { get; }

        /// <summary>
        /// Create an instance of the composition with the given configuration.
        /// </summary>
        /// <param name="monitors">The monitors in the composition.</param>
        /// <param name="rectangle">The bounding rectangle of the desktop.</param>
        public MonitorComposition(IEnumerable<Monitor> monitors, DesktopRectangle rectangle)
        {
            Monitors = monitors;
            Desktop = rectangle;
        }
    }
}
