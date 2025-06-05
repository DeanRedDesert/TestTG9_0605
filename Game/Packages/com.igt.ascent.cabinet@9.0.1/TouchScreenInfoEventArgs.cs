// -----------------------------------------------------------------------
// <copyright file = "TouchScreenInfoEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating that the touch screen info has changed.
    /// </summary>
    public class TouchScreenInfoEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TouchScreenInfoEventArgs"/>.
        /// </summary>
        /// <param name="touchScreenInfo">Collection of <see cref="TouchScreenInfo"/> objects.</param>
        public TouchScreenInfoEventArgs(IEnumerable<TouchScreenInfo> touchScreenInfo)
        {
            if(touchScreenInfo == null)
            {
                touchScreenInfo = new List<TouchScreenInfo>();
            }

            TouchScreenInfo = touchScreenInfo;
        }

        /// <summary>
        /// Gets a collection of the connected touch screen devices.
        /// </summary>
        /// <remarks>
        /// This will return an empty collection if no touch screens are connected.
        /// </remarks>
        public IEnumerable<TouchScreenInfo> TouchScreenInfo { get; }
    }
}
