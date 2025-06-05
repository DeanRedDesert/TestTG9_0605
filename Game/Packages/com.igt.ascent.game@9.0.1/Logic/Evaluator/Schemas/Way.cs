//-----------------------------------------------------------------------
// <copyright file = "Way.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Cloneable;
    using CompactSerialization;

    /// <summary>Collects position and symbol information for a single paying way in a multiway outcome.</summary>
    public class Way : ICompactSerializable, IDeepCloneable
    {
        /// <summary>List of outcome cells corresponding to this way.</summary>
        public List<OutcomeCell> Cells { get; private set; }

        /// <summary>Dictionary of symbols to the counts associated with those symbols.</summary>
        public Dictionary<string, int> SymbolCounts { get; private set; }

        /// <summary>Initialize a Way instance, with no way content given.</summary>
        public Way()
        {
            Cells = new List<OutcomeCell>();
            SymbolCounts = new Dictionary<string, int>();
        }

        /// <summary>Initialize a Way instance from an existing Way and an additional symbol.</summary>
        /// <param name="baseWay">The Way to start from.</param>
        /// <param name="cell">The <see cref="OutcomeCell"/> corresponding to the symbol added.</param>
        public Way(Way baseWay, OutcomeCell cell)
        {
            // copy the base way's list of cells and add the given cell to it.
            Cells = new List<OutcomeCell>(baseWay.Cells) { cell };

            // Copy the baseWay's symbolCounts and add the new symbolId to it.
            SymbolCounts = new Dictionary<string, int>(baseWay.SymbolCounts);
            if(SymbolCounts.ContainsKey(cell.symbolID))
            {
                SymbolCounts[cell.symbolID]++;
            }
            else
            {
                SymbolCounts[cell.symbolID] = 1;
            }
        }

        /// <summary>
        /// Override function, provide content of Way in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("=================Way===============\n");
            if (Cells.Count == 0)
            {
                resultBuilder.AppendLine("Cells.Count is zero.");
            }
            else
            {
                var count = 0;
                foreach(var cell in Cells)
                {
                    resultBuilder.AppendFormat("Cells[{0}] \n", count);
                    resultBuilder.Append(cell);
                    ++count;
                }
            }
            if (SymbolCounts.Count == 0)
            {
                resultBuilder.AppendLine("SymbolCounts.Count is zero.");
            }
            else
            {
                foreach (var item in SymbolCounts)
                {
                    resultBuilder.AppendLine("SymbolCount: " + item.Key + " " + item.Value);
                }
            }
            resultBuilder.AppendLine("=================Way items are listed===============");
            return resultBuilder.ToString();
        }

        #region ICompactSerializable implementation

	    /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Cells);
            CompactSerializer.WriteDictionary(stream, SymbolCounts);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Cells = CompactSerializer.ReadListSerializable<OutcomeCell>(stream);
            SymbolCounts = CompactSerializer.ReadDictionary<string, int>(stream);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new Way
                       {
                           Cells = NullableClone.DeepCloneList(Cells),
                           SymbolCounts = NullableClone.ShallowCloneDictionary(SymbolCounts)
                       };
            return copy;
        }

        #endregion
    }
}
