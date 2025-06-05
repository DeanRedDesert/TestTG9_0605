//-----------------------------------------------------------------------
// <copyright file = "WinOutcome.Extensions.cs" company = "IGT">
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
    /// Extensions to the generated WinOutcome class.
    /// </summary>
    public partial class WinOutcome : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Override function, provide content of WinOutcome in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("=========WinOutcome===============\n");
            if(WinOutcomeItems.Count == 0)
            {
                resultBuilder.AppendLine("WinOutcomeItems.Count is zero.");
            }
            else
            {
                var count = 0;
                foreach(var winOutcome in WinOutcomeItems)
                {
                    resultBuilder.AppendFormat("WinOutcomeItems[{0}] \n", count);
                    resultBuilder.Append(winOutcome);
                    ++count;
                }
            }
            resultBuilder.AppendLine("totalWinAmount = " + totalWinAmount);
            resultBuilder.AppendLine("WinOutcome name = " + name);
            resultBuilder.AppendLine("=========WinOutcome items are listed.===============");
            return resultBuilder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, totalWinAmount);
            CompactSerializer.WriteList(stream, WinOutcomeItems);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            totalWinAmount = CompactSerializer.ReadLong(stream);
            WinOutcomeItems = CompactSerializer.ReadListSerializable<WinOutcomeItem>(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new WinOutcome
                       {
                           name = name,
                           totalWinAmount = totalWinAmount,
                           WinOutcomeItems = NullableClone.DeepCloneList(WinOutcomeItems)
                       };
            return copy;
        }

        #endregion
    }
}
