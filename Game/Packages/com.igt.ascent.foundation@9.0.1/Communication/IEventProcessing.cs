// -----------------------------------------------------------------------
// <copyright file = "IEventProcessing.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// This interface is used to notify processings to the event sources.
    /// </summary>
    public interface IEventProcessing
    {
        /// <summary>
        /// Tiggered after an event source was processed.
        /// </summary>
        event EventHandler EventProcessed;
    }
}