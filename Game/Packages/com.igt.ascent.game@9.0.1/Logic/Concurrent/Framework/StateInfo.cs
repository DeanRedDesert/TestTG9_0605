// -----------------------------------------------------------------------
// <copyright file = "StateInfo.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.IO;
    using Game.Core.CompactSerialization;
    using Interfaces;

    /// <summary>
    /// This class encapsulates the critical data of a state
    /// that needs to be written into the safe storage.
    /// </summary>
    [Serializable]
    internal sealed class StateInfo : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the current state name.
        /// </summary>
        public string CurrentStateName { get; set; }

        /// <summary>
        /// Gets or sets the pending state name.
        /// </summary>
        public string PendingStateName { get; set; }

        /// <summary>
        /// Gets or sets the current step in current state.
        /// </summary>
        public StateStep CurrentStep { get; set; }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, CurrentStateName);
            CompactSerializer.Write(stream, PendingStateName);
            CompactSerializer.Write(stream, CurrentStep);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            CurrentStateName = CompactSerializer.ReadString(stream);
            PendingStateName = CompactSerializer.ReadString(stream);
            CurrentStep = CompactSerializer.ReadEnum<StateStep>(stream);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{CurrentStateName ?? "null"}.{CurrentStep}";
        }

        #endregion
    }
}