// -----------------------------------------------------------------------
//  <copyright file = "DiscoveryResult.cs" company = "IGT">
//      Copyright (c) 2019 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServiceDiscovery
{
    /// <summary>
    /// Contains information about a single MEF discovery result - error, warning or success.
    /// </summary>
    public class DiscoveryResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="discoveryResult">The overall load result for a single assembly's discovery process.</param>
        /// <param name="message">Specific or additional information about a discovery load process step for a single assembly. </param>
        public DiscoveryResult(DiscoveryResultType discoveryResult, string message)
        {
            DiscoveryResultType = discoveryResult;
            Message = message;
        }

        /// <summary>
        /// Gets/sets the overall results of type <see cref="DiscoveryResultType"/>of a single file load or exception.
        /// </summary>
        public DiscoveryResultType DiscoveryResultType
        {
            get;
        }

        /// <summary>
        /// The optional specific details for this MEF result.
        /// </summary>
        public string Message
        {
            get;
        }
    }
}