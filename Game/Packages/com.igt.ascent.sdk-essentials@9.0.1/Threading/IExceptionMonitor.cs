//-----------------------------------------------------------------------
// <copyright file = "IExceptionMonitor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// An interface that defines the signals and methods that are used for notifying and retrieving
    /// the exceptions crossing threads.
    /// </summary>
    public interface IExceptionMonitor
    {
        /// <summary>
        /// The wait handle which will be signaled when an exception has occurred in the target thread.
        /// </summary>
        WaitHandle ExceptionSignalHandle { get; }

        /// <summary>
        /// Get any exception which may have occurred in the target thread.
        /// </summary>
        /// <returns>An exception if one has occurred.</returns>
        /// <remarks>
        /// Exceptions which happen on the target threads are not automatically propagated to the main thread.
        /// This function allows us to check if an exception has occurred.
        /// </remarks>
        Exception CheckException();
    }
}
