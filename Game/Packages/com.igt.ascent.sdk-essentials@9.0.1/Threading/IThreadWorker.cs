//-----------------------------------------------------------------------
// <copyright file = "IThreadWorker.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System.Threading;

    /// <summary>
    /// Interface to define the worker function for a worker thread.
    /// </summary>
    public interface IThreadWorker
    {
        /// <summary>
        /// Gets the name of the worker and the thread.
        /// </summary>
        string ThreadName { get; }

        /// <summary>
        /// This method does the work for the worker thread.  When this returns the thread will end.
        /// </summary>
        /// <param name="exitHandle">
        /// This handle will be signaled when the worker thread is being terminated.
        /// This function needs to return when it is signaled.
        /// </param>
        void DoWork(WaitHandle exitHandle);
    }
}