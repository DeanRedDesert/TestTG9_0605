//-----------------------------------------------------------------------
// <copyright file = "TransitionMode.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{

    /// <summary>
    /// Enumeration which indicates how a transition is to be made between light states and sequences.
    /// </summary>
    /// <remarks>
    /// EnumStorageShouldBeInt32 is suppressed because this enumeration is associated with a hardware device where the
    /// storage type is a byte.
    /// </remarks>
    public enum TransitionMode : byte
    {
        /// <summary>
        /// Any currently executing sequences will end immediately.
        /// </summary>
        Immediate = 0,

        /// <summary>
        /// The current sequence will be allowed to reach a logical stopping point before the transition.
        /// </summary>
        Sequenced
    }
}
