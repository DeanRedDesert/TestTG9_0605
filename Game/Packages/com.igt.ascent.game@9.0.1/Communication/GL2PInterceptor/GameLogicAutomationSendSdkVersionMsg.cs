//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendSdkVersionMsg.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// This message class is responsible for sending the SDK version.
    /// </summary>
    [Serializable]
    public class GameLogicAutomationSendSdkVersionMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Intitalizes a new instance with the current version of the SDK.
        /// </summary>
        /// <param name="sdkVersion">The current version of the SDK.</param>
        public GameLogicAutomationSendSdkVersionMsg(Version sdkVersion)
        {
            SdkVersion = sdkVersion;
        }

        /// <summary>
        /// The SDK version that we are currently on.
        /// </summary>
        public Version SdkVersion { private set; get; }
    }
}
