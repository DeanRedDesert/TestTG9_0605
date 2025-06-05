//-----------------------------------------------------------------------
// <copyright file = "CoplayerShutDownEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    /// <summary>
    /// The event notifying the shell that a coplayer is being shut down by Foundation.
    /// </summary>
    internal class CoplayerShutDownEventArgs : BaseCoplayerEventArgs
    {
        /// <inheritdoc/>
        public CoplayerShutDownEventArgs(int coplayerId) : base(coplayerId)
        {
        }
    }
}
