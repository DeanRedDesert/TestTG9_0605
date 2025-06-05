//-----------------------------------------------------------------------
// <copyright file = "OutcomeCell.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using System.Text;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated OutcomeCell class.
    /// </summary>
    public partial class OutcomeCell : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Override function, provide content of OutcomeCell in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("=================OutcomeCell===============\n");
            if (Cell == null)
            {
                resultBuilder.AppendLine("Cell = NULL");
            }
            else
            {
                resultBuilder.Append(Cell);
            }
            resultBuilder.AppendLine("symbolID = " + symbolID);
            resultBuilder.AppendLine("stop = " + stop);
            resultBuilder.AppendLine("stopSpecified = " + stopSpecified);
            resultBuilder.AppendLine("=================OutcomeCell items are listed.===============");
            return resultBuilder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Cell);
            CompactSerializer.Write(stream, symbolID);
            CompactSerializer.Write(stream, stop);
            CompactSerializer.Write(stream, stopSpecified);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Cell = CompactSerializer.ReadSerializable<Cell>(stream);
            symbolID = CompactSerializer.ReadString(stream);
            stop = CompactSerializer.ReadUint(stream);
            stopSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new OutcomeCell
                       {
                           Cell = NullableClone.DeepClone(Cell),
                           symbolID = symbolID,
                           stop = stop,
                           stopSpecified = stopSpecified,
                       };
            return copy;
        }

        #endregion
    }
}
