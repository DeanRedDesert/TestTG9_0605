// -----------------------------------------------------------------------
// <copyright file = "KenoWinCategory.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the KenoWinCategory class.
    /// </summary>
    public partial class KenoWinCategory : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Win);
            CompactSerializer.WriteList(stream, ProgressiveLevel);
            CompactSerializer.WriteList(stream, OddsList);
            CompactSerializer.Write(stream, name);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Win = CompactSerializer.ReadListSerializable<KenoWinCategoryWin>(stream);
            ProgressiveLevel = CompactSerializer.ReadListString(stream);
            OddsList = CompactSerializer.ReadListSerializable<KenoWinCategoryOddsList>(stream);
            name = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
