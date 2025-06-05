//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationSendMonitorConfiguration.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Sends the current monitor configuration.
    /// </summary>
    [Serializable]
    public class PresentationAutomationSendMonitorConfiguration : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationAutomationSendMonitorConfiguration() { }

        /// <summary>
        /// Constructor for PresentationAutomationSendMonitorConfiguration.
        /// </summary>
        /// <param name="monitors">The list of connected monitors.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="monitors"/> contains Monitor.All.
        /// </exception>
        public PresentationAutomationSendMonitorConfiguration(IList<Monitor> monitors)
        {
            if(monitors != null && monitors.Any(monitor => monitor == Monitor.All))
            {
                throw new ArgumentException("Monitor.All is not a valid item.", "monitors");
            }

            Monitors = monitors;
        }

        /// <summary>
        /// The list of connected monitors.
        /// </summary>
        public IList<Monitor> Monitors
        {
            get;
            private set;
        }
    }
}
