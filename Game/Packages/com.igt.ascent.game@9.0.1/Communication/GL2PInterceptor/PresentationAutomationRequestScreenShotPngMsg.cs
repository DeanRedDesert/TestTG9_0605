//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationRequestScreenShotPngMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPresentationAutomationClient
    /// interface function RequestScreenShotPng.
    /// </summary>
    [Serializable]
    public class PresentationAutomationRequestScreenShotPngMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationAutomationRequestScreenShotPngMsg()
        {

        }

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="requestedMonitor">The monitor to take a screen shot of.</param>
        public PresentationAutomationRequestScreenShotPngMsg(Monitor requestedMonitor)
        {
            RequestedMonitor = requestedMonitor;
        }

        /// <summary>
        /// The monitor to take a screen shot of.
        /// </summary>
        public Monitor RequestedMonitor
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Requested Monitor: " + RequestedMonitor;
        }
    }
}
