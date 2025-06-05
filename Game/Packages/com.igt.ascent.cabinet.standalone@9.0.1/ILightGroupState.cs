//-----------------------------------------------------------------------
// <copyright file = "ILightGroupState.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a read-only view of the state of a group of lights.
    /// </summary>
    public interface ILightGroupState
    {
        /// <summary>
        /// Get the states of the lights in this group.
        /// </summary>
        IEnumerable<ILightState> LightStates { get; }

        /// <summary>
        /// Flag which indicates if a sequence is active.
        /// </summary>
        bool SequenceActive { get; }

        /// <summary>
        /// Current sequence.
        /// </summary>
        uint CurrentSequence { get; }
    }
}
