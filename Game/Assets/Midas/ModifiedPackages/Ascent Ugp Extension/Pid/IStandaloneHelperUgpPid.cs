//-----------------------------------------------------------------------
// <copyright file = "IStandaloneHelperUgpPid.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    /// <summary>
    /// Standalone helper interface for UGP PID.
    /// </summary>
    public interface IStandaloneHelperUgpPid
    {
        /// <summary>
        /// Sets the PID configuration and raises the Pid configuration changed event.
        /// </summary>
        /// <param name="configuration">
        /// The PID configuration to set.
        /// </param>
        void SetPidConfiguration(PidConfiguration configuration);
    }
}
