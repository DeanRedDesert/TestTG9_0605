//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationSendScreenShotPngMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPresentationAutomationService
    /// interface function SendScreenShotPng.
    /// </summary>
    [Serializable]
    public class PresentationAutomationSendScreenShotPngMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationAutomationSendScreenShotPngMsg()
        {
        }

        /// <summary>
        /// Constructor for creating PresentationAutomationSendScreenShotPngMsg.
        /// </summary>
        /// <param name="pngFile">Byte buffer containing png file contents.</param>
        /// <param name="monitor">The monitor the <see cref="PngFile"/> represents.</param>
        public PresentationAutomationSendScreenShotPngMsg(byte[] pngFile, Monitor monitor)
        {
            PngFile = pngFile;
            Monitor = monitor;
        }

        /// <summary>
        /// Byte buffer containing PNG file contents.
        /// </summary>
        public byte[] PngFile
        {
            get;
            private set;
        }

        /// <summary>
        /// The monitor(s) represented in the screen shot.
        /// </summary>
        public Monitor Monitor
        {
            get;
            private set;
        }
    }
}
