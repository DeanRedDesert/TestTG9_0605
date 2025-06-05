//-----------------------------------------------------------------------
// <copyright file = "StateStage.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;

    /// <summary>
    /// This enumeration is used to indicate the state
    /// stage of a state.
    /// </summary>
    [Serializable]
    public enum StateStage
    {
        /// <summary>
        /// Enumeration for the Processing stage of a state.
        /// </summary>
        Processing = 0,

        /// <summary>
        /// Enumeration for the committed stage of a state.
        /// </summary>
        Committed = 1
    }
}
