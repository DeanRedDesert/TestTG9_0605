//-----------------------------------------------------------------------
// <copyright file = "IUgpPidCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    /// <summary>
    /// Interface to accept callbacks from the UGP PID category.
    /// </summary>
    public interface IUgpPidCategoryCallbacks
    {
        /// <summary>
        /// Method called when UgpPidCategory ConfigurationChanged message is received from the foundation.
        /// </summary>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessPidConfigurationChanged();

        /// <summary>
        /// Method called when UgpPidCategory PidActivation message is received from the foundation.
        /// </summary>
        /// <param name="status">The flag indicating if Pid is activated or not.</param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessPidActivation(bool status);

        /// <summary>
        /// Method called to notify AttendantServiceRequested.
        /// </summary>
        /// <param name="isServiceRequested">The flag indicating if service is requested or not.</param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string NotifyAttendantServiceRequested(bool isServiceRequested);
    }
}
