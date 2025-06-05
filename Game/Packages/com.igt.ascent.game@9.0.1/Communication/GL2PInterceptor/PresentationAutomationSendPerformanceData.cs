//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationSendPerformanceData.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPresentationAutomationService
    /// interface function SendPerformanceData.
    /// </summary>
    [Serializable]
    public class PresentationAutomationSendPerformanceData : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationAutomationSendPerformanceData()
        {}

        /// <summary>
        /// Constructor for creating the object.
        /// </summary>
        /// <param name="performanceData"></param>
        /// <param name="state"></param>
        public PresentationAutomationSendPerformanceData(float performanceData, string state)
        {
            PerformanceData = performanceData;
            MachineState = state;
        }
		
		/// <summary>
        /// Constructor for creating the object.
        /// </summary>
        /// <param name="performanceData"></param>
        public PresentationAutomationSendPerformanceData(float performanceData)
        {
            PerformanceData = performanceData;
            MachineState = null;
        }
		
        /// <summary>
        /// The state of the machine at the time the performance data is captured
        /// </summary>
        public string MachineState { private set; get; }

        /// <summary>
        /// Value of the performance data being measured
        /// </summary>
        public float PerformanceData { private set; get; }
    }
}
