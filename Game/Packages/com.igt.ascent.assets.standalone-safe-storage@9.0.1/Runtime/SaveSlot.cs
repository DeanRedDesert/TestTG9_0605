// -----------------------------------------------------------------------
// <copyright file = "SaveSlot.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Assets.StandaloneSafeStorage
{
    /// <summary>
    /// Defines the standalone safe storage slot.  Each slot represents a file based safe storage
    /// that can be selected to use when running game in Standalone mode.
    /// </summary>
    public enum SaveSlot
    {
        /// <summary>
        /// The default file used when file backed standalone safe storage is enabled.
        /// </summary>
        Default,

        /// <summary>
        /// Data from SaveSlot1 in the Tools menu.
        /// </summary>
        Slot1,

        /// <summary>
        /// Data from SaveSlot2 in the Tools menu.
        /// </summary>
        Slot2,

        /// <summary>
        /// Data from SaveSlot3 in the Tools menu.
        /// </summary>
        Slot3,

        /// <summary>
        /// Data from SaveSlot4 in the Tools menu.
        /// </summary>
        Slot4,

        /// <summary>
        /// Data from SaveSlot5 in the Tools menu.
        /// </summary>
        Slot5
    }
}
