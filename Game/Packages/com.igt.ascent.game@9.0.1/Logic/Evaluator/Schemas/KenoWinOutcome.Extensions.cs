// -----------------------------------------------------------------------
// <copyright file = "KenoWinOutcome.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the KenoWinOutcome class.
    /// </summary>
    public partial class KenoWinOutcome : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, KenoWinOutcomeItems);
            CompactSerializer.WriteList(stream, KenoCardsList);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            KenoWinOutcomeItems = CompactSerializer.ReadListSerializable<KenoWinOutcomeItem>(stream);
            KenoCardsList = CompactSerializer.ReadListSerializable<KenoCard>(stream);
        }

        #endregion
    }
}
