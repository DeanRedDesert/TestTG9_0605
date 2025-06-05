//-----------------------------------------------------------------------
// <copyright file = "WaitHandleExtension.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Extension class providing wrappers of WaitHandle.WaitAny methods that work with
    /// Unity Debugger, as well as new extension methods which make sure that wait handles
    /// would be notified when there are exceptions thrown on the target thread.
    /// </summary>
    /// <remarks>
    /// Note that not all WaitAny methods are wrapped here, such as
    /// <see cref="WaitHandle.WaitAny(WaitHandle[], TimeSpan, Boolean)"/> etc.
    /// New wrappers can be added per requests.
    /// </remarks>
    public static class WaitHandleExtension
    {
        // ReSharper disable InconsistentNaming
        private const uint WAIT_TIMEOUT = 0x00000102;  // Same as WaitHandle.WaitTimeout
        private const uint WAIT_IO_COMPLETION = 0x000000c0;
        private const uint WAIT_FAILED = 0xFFFFFFFF;
        // ReSharper restore InconsistentNaming

        #region Extensions to Wait Handle Array

        /// <summary>
        /// Blocks the current thread until any of the wait handles in <paramref name="waitHandles"/> receives a signal,
        /// or the <see cref="IExceptionMonitor"/> signals an exception.
        /// </summary>
        /// <param name="waitHandles">Wait handles to wait for a signal.</param>
        /// <param name="exceptionMonitor">A <see cref="IExceptionMonitor"/> to notify exceptions.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown <paramref name="waitHandles"/> is null.
        /// </exception>
        /// <returns>The handle which was signaled.</returns>
        public static WaitHandle WaitAny(this WaitHandle[] waitHandles,
                                         IExceptionMonitor exceptionMonitor)
        {
            if(waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles));
            }

            return waitHandles.WaitAny(exceptionMonitor, Timeout.Infinite);
        }

        /// <summary>
        /// Blocks the current thread until any of the wait handles in <paramref name="waitHandles"/> receives a signal,
        /// the timeout elapses, or the <see cref="IExceptionMonitor"/> signals an exception.
        /// </summary>
        /// <param name="waitHandles">Wait handles to wait for a signal.</param>
        /// <param name="millisecondsTimeout">Timeout to wait for a signal.</param>
        /// <param name="exceptionMonitor">A <see cref="IExceptionMonitor"/> to notify exceptions.</param>
        /// <returns>
        /// The handle which was signaled, or null if the timeout elapses. If the <paramref name="exceptionMonitor"/>
        /// reports an exception, then that exception will be thrown.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown <paramref name="waitHandles"/> or <paramref name="exceptionMonitor"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="exceptionMonitor"/> contains a null exception signal handle.
        /// </exception>
        public static WaitHandle WaitAny(this WaitHandle[] waitHandles,
                                         IExceptionMonitor exceptionMonitor,
                                         int millisecondsTimeout)
        {
            if(waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles));
            }

            if(exceptionMonitor == null)
            {
                throw new ArgumentNullException(nameof(exceptionMonitor));
            }

            if(exceptionMonitor.ExceptionSignalHandle == null)
            {
                throw new ArgumentException("exceptionMonitor provides a null wait handle.", nameof(exceptionMonitor));
            }

            var combinedHandles = new WaitHandle[waitHandles.Length + 1];
            combinedHandles[0] = exceptionMonitor.ExceptionSignalHandle;
            waitHandles.CopyTo(combinedHandles, 1);

            var signaled = combinedHandles.WaitAny(millisecondsTimeout);

            if(signaled == exceptionMonitor.ExceptionSignalHandle)
            {
                throw new RelayedException(exceptionMonitor.CheckException());
            }

            return signaled;
        }

        /// <summary>
        /// Waits for any of the handles in the <paramref name="waitHandles"/> array to be signaled.
        /// </summary>
        /// <remarks>
        /// This method is designed to replace <see cref="WaitHandle.WaitAny(WaitHandle[])"/> method.
        /// It fixes some threading bugs when running with Unity debugger.
        /// Please call this method instead whenever <see cref="WaitHandle.WaitAny(WaitHandle[])"/> is needed.
        /// </remarks>
        /// <param name="waitHandles">Wait handles to wait for a signal.</param>
        /// <returns>The handle which was signaled.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown <paramref name="waitHandles"/> is null.
        /// </exception>
        public static WaitHandle WaitAny(this WaitHandle[] waitHandles)
        {
            if(waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles));
            }

            return waitHandles.WaitAny(Timeout.Infinite);
        }

        /// <summary>
        /// Waits for any of the handles in the <paramref name="waitHandles"/> array to be signaled or for the timeout
        /// to elapse.
        /// </summary>
        /// <remarks>
        /// This method is designed to replace <see cref="WaitHandle.WaitAny(WaitHandle[], Int32)"/> method.
        /// It fixes some threading bugs when running with Unity debugger.
        /// Please call this method instead whenever <see cref="WaitHandle.WaitAny(WaitHandle[], Int32)"/> is needed.
        /// </remarks>
        /// <param name="waitHandles">Wait handles to wait for a signal.</param>
        /// <param name="millisecondsTimeout">Timeout to wait for a signal.</param>
        /// <returns>The handle which was signaled, or null if the timeout elapses.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown <paramref name="waitHandles"/> is null.
        /// </exception>
        public static WaitHandle WaitAny(this WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            if(waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles));
            }

            WaitHandle result = null;
            var waiting = true;

            while(waiting)
            {
                // Always resume waiting for the original timeout after being interrupted.
                // This is to simplify implementation until further requests.
                // Otherwise we'll have to add timers in this function to remember
                // how long we had waited before the interruption.
                var signal = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);

                switch((uint)signal)
                {
                    case WAIT_TIMEOUT:
                        // When timeout is infinite, we should not receive TIMEOUT.
                        // This is just a double check to make sure that we are not
                        // returning null in case of infinite timeout.
                        waiting = millisecondsTimeout == Timeout.Infinite;
                        break;

                    case WAIT_IO_COMPLETION:
                    case WAIT_FAILED:
                        // Wait has been interrupted.  Keep waiting.
                        break;

                    default:
                        if(signal < waitHandles.Length)
                        {
                            result = waitHandles[signal];
                            waiting = false;
                        }
                        break;
                }
            }

            return result;
        }

        #endregion

        #region Extensions to Wait Handle

        /// <summary>
        /// Blocks the current thread until the <see cref="WaitHandle"/> receives a signal;
        /// the current thread may also be unblocked if the <see cref="IExceptionMonitor"/> notifies an exception.
        /// </summary>
        /// <param name="waitHandle">A <see cref="WaitHandle"/> to wait for a signal.</param>
        /// <param name="exceptionMonitor">A <see cref="IExceptionMonitor"/> to notify exceptions.</param>
        /// <returns>true if the <paramref name="waitHandle"/> receives a signal. If the 
        /// <paramref name="exceptionMonitor"/> notifies an exception, the exception will be thrown on current 
        /// thread and this method does not return.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="waitHandle"/> or <paramref name="exceptionMonitor"/> is null.
        /// </exception>
        public static bool WaitOne(this WaitHandle waitHandle, IExceptionMonitor exceptionMonitor)
        {
            if(waitHandle == null)
            {
                throw new ArgumentNullException(nameof(waitHandle));
            }

            if(exceptionMonitor == null)
            {
                throw new ArgumentNullException(nameof(exceptionMonitor));
            }

            return waitHandle.WaitOne(exceptionMonitor, Timeout.Infinite);
        }

        /// <summary>
        /// Blocks the current thread until the <see cref="WaitHandle"/> receives a signal or timeout;
        /// the current thread may also be unblocked if the <see cref="IExceptionMonitor"/> notifies an exception.
        /// </summary>
        /// <param name="waitHandle">A <see cref="WaitHandle"/> to wait for a signal.</param>
        /// <param name="exceptionMonitor">A <see cref="IExceptionMonitor"/> to notify exceptions.</param>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or 
        /// <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
        /// <returns>
        /// True if the <paramref name="waitHandle"/> receives a signal; otherwise, false. If the 
        /// <paramref name="exceptionMonitor"/> notifies an exception, the exception will be thrown on current
        /// thread and this method does not return.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="waitHandle"/> or <paramref name="exceptionMonitor"/> is null.
        /// </exception>
        public static bool WaitOne(this WaitHandle waitHandle,
                                   IExceptionMonitor exceptionMonitor,
                                   int millisecondsTimeout)
        {
            if(waitHandle == null)
            {
                throw new ArgumentNullException(nameof(waitHandle));
            }

            if(exceptionMonitor == null)
            {
                throw new ArgumentNullException(nameof(exceptionMonitor));
            }

            // exceptionMonitor.ExceptionSignalHandle is added by WaitAny(this WaitHandle[], IExceptionMonitor, int) above.
            var handles = new [] { waitHandle };

            return waitHandle == handles.WaitAny(exceptionMonitor, millisecondsTimeout);
        }
    }

    #endregion
}
