// -----------------------------------------------------------------------
//  <copyright file = "DiscoveryResultType.cs" company = "IGT">
//      Copyright (c) 2019 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServiceDiscovery
{
    /// <summary>
    /// An enumeration describing the overall MEF assembly load/discover result for a single step of the process.
    /// </summary>
    public enum DiscoveryResultType
    {
        /// <summary>
        /// An error occurred or no compatible MEF exports were discovered.
        /// </summary>
        Failure,

        /// <summary>
        /// An error occurred but it may ignorable if the containing application does not need an MEF plug-in.
        /// </summary>
        Warning,

        /// <summary>
        /// The process step succeeded. 
        /// </summary>
        Success,

        /// <summary>
        /// Additional information related to the process.
        /// </summary>
        Informational
    }
}
