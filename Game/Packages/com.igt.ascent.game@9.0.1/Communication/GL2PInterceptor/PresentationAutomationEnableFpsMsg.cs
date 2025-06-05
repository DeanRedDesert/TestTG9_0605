//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationEnableFpsMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

﻿namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPresentationAutomationClient
    /// interface function RequestFpsEnable.
    /// </summary>
    [Serializable]
    public class PresentationAutomationEnableFpsMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Private default constructor needed for some methods of serialization.
        /// </summary>
        private PresentationAutomationEnableFpsMsg()
        { }

        /// <summary>
        /// Constructor for assigning the value.
        /// </summary>
        /// <param name="enabled"></param>
        public PresentationAutomationEnableFpsMsg(bool enabled)
        {
            FpsLoggingEnabled = enabled;
        }

        /// <summary>
        /// The on if frame rate gathering is enabled or disabled.
        /// </summary>
        public bool FpsLoggingEnabled { private set; get; }
    }
}
