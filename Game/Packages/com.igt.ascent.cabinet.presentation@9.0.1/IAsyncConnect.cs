// -----------------------------------------------------------------------
// <copyright file = "IAsyncConnect.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation
{
    using System;
    using Communication.Cabinet;

    /// <summary>
    /// Defines asynchronous connection and initialization functionality.
    /// </summary>
    /// <remarks>
    /// Classes which implement this interface can do initialization
    /// on a separate thread to improve performance.
    /// All cabinet services should implement this interface.
    /// All cabinet controllers need to follow the pattern designed by this interface.
    /// </remarks>
    public interface IAsyncConnect
    {
        /// <summary>
        /// Gets the flag indicating if <see cref="AsyncConnect"/> is complete.
        /// </summary>
        bool AsyncConnectComplete { get; }

        /// <summary>
        /// Gets the flag indicating if <see cref="PostConnect"/> is complete.
        /// </summary>
        bool PostConnectComplete { get; }

        /// <summary>
        /// Connects and initializes on a separate thread to speed up performance.
        /// </summary>
        /// <remarks>
        /// Consider thread synchronization when accesses shared data.
        /// The recommended calls within this method are CSI communications,
        /// meanwhile higher layer should ensure there are no other CSI communications
        /// on the main thread.
        /// </remarks>
        /// <param name="cabinetLib">The CSI client to connect to.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cabinetLib"/> is null.
        /// </exception>
        void AsyncConnect(ICabinetLib cabinetLib);

        /// <summary>
        /// Initializes on the main thread.
        /// This will be called after <see cref="AsyncConnect"/>.
        /// </summary>
        /// <remarks>
        /// This method is executed on Unity main thread, typical calls within it
        /// are presentation codes that would access Unity Engine APIs,
        /// and the ones that may raise event or callback into game specific codes.
        /// </remarks>
        /// <exception cref="AsyncConnectException">
        /// Thrown if <see cref="PostConnect"/> failed.
        /// </exception>
        void PostConnect();
    }
}
