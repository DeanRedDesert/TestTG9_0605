//-----------------------------------------------------------------------
// <copyright file = "FrameRateReporting.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.AutomationServices
{
    using GL2PInterceptorLib; 

    /// <summary>
	/// Frame rate reporting class reports the frame rate to the 
	/// </summary>
    public class FrameRateReporting
    {
		/// <summary>
		/// Default constructor used for some serialization.
		/// </summary>
        private FrameRateReporting()
        { }
		
		/// <summary>
		/// Reports the frame rate through the GL2P service.
		/// </summary>
		/// <param name='framesPerSecond'>
		/// Frames per second.
		/// </param>
		/// <param name='socketPInterceptorService'>
		/// Presentation service for GL2P.
		/// </param>
        public static void ReportFrameRate (float framesPerSecond, IPresentationAutomationService socketPInterceptorService)
        {
            socketPInterceptorService.SendPerformanceData(framesPerSecond);
        }
    }
}
