//-----------------------------------------------------------------------
// <copyright file = "Prize.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated Prize class.
    /// </summary>
    public partial class Prize : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// The valid prize strings for any progressives in the prize.
        /// </summary>
        [XmlIgnore]
        public List<string> ProgressivePrizeStrings { get; set; }

        /// <summary>
        /// Override function, provide content of Prize in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("===============Prize===============\n");
            if (ProgressiveLevels.Count == 0)
            {
                resultBuilder.AppendLine("ProgressiveLevels.Count is zero");
            }
            else
            {
                for (var i = 0; i < ProgressiveLevels.Count; i++)
                {
                    resultBuilder.AppendFormat("ProgressiveLevels[{0}] = {1} \n", i, ProgressiveLevels[i]);
                }
            }
            if (Trigger.Count == 0)
            {
                resultBuilder.AppendLine("Trigger.Count is zero.");
            }
            else
            {
                var count = 0;
                foreach(var trigger in Trigger)
                {
                    resultBuilder.AppendFormat("Trigger[{0}] \n", count);
                    resultBuilder.Append(trigger);
                    ++count;
                }
            }
            resultBuilder.AppendLine("multiplier = " + multiplier);
            resultBuilder.AppendLine("prizeScaleName = " + prizeScaleName);
            resultBuilder.AppendLine("winAmount = " + winAmount);
            resultBuilder.AppendLine("prizeName = " + prizeName);
            resultBuilder.AppendLine("nearHitProgressive = " + nearHitProgressive);
            resultBuilder.AppendLine("averageBonusPay = " + averageBonusPay);
            resultBuilder.AppendLine("averageBonusPaySpecified = " + averageBonusPaySpecified);
            resultBuilder.AppendLine("===============Prize items are listed.===============");
            return resultBuilder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, prizeName);
            CompactSerializer.Write(stream, prizeScaleName);

            CompactSerializer.Write(stream, winAmount);
            CompactSerializer.Write(stream, multiplier);
            CompactSerializer.Write(stream, nearHitProgressive);
            CompactSerializer.Write(stream, averageBonusPay);
            CompactSerializer.Write(stream, averageBonusPaySpecified);

            CompactSerializer.WriteList(stream, ProgressiveLevels);
            CompactSerializer.WriteList(stream, Trigger);

            CompactSerializer.WriteList(stream, ProgressivePrizeStrings);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            prizeName = CompactSerializer.ReadString(stream);
            prizeScaleName = CompactSerializer.ReadString(stream);

            winAmount = CompactSerializer.ReadLong(stream);
            multiplier = CompactSerializer.ReadUint(stream);
            nearHitProgressive = CompactSerializer.ReadBool(stream);
            averageBonusPay = CompactSerializer.ReadLong(stream);
            averageBonusPaySpecified = CompactSerializer.ReadBool(stream);

            ProgressiveLevels = CompactSerializer.ReadListString(stream);
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);

            ProgressivePrizeStrings = CompactSerializer.ReadListString(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new Prize
                       {
                           prizeName = prizeName,
                           prizeScaleName = prizeScaleName,
                           winAmount = winAmount,
                           multiplier = multiplier,
                           nearHitProgressive = nearHitProgressive,
                           averageBonusPay = averageBonusPay,
                           averageBonusPaySpecified = averageBonusPaySpecified,
                           ProgressiveLevels = NullableClone.ShallowCloneList(ProgressiveLevels),
                           Trigger = NullableClone.DeepCloneList(Trigger),
                           ProgressivePrizeStrings = NullableClone.ShallowCloneList(ProgressivePrizeStrings)
                       };
            return copy;
        }

        #endregion
    }
}
