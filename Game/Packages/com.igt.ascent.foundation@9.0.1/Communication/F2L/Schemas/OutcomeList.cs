//-----------------------------------------------------------------------
// <copyright file = "OutcomeList.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;
    using System.Linq;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// </summary>
    public partial class OutcomeList : IOutcomeList, ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public OutcomeList()
        { }

        /// <summary>
        /// Initializes  a new instance of <see cref="OutcomeList"/> from an existing <see cref="IOutcomeList"/> object
        /// </summary>
        /// <param name="list"></param>
        public OutcomeList(IOutcomeList list)
        {
            if(list == null)
            {
                throw new ArgumentNullException("list");
            }

            featureEntryField = list.GetFeatureEntries().Select(entry => new OutcomeListFeatureEntry(entry)).ToList();
            gameCycleEntryField = list.GetGameCycleEntries().Select(entry => new OutcomeListGameCycleEntry(entry)).ToList();
        }

        #endregion

        #region IOutcomeList Members

        /// <inheritdoc />
        public ReadOnlyCollection<IFeatureEntry> GetFeatureEntries()
        {
            return featureEntryField.Cast<IFeatureEntry>().ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IGameCycleEntry> GetGameCycleEntries()
        {
            return gameCycleEntryField.Cast<IGameCycleEntry>().ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public long GetTotalDisplayableAmount()
        {
            checked
            {
                long total = 0;

                if(featureEntryField != null)
                {
                    total += featureEntryField.Sum(entry => entry.GetTotalDisplayableAmount());
                }

                if(gameCycleEntryField != null)
                {
                    total += gameCycleEntryField.Sum(entry => entry.GetTotalDisplayableAmount());
                }

                return total;
            }
        }

        #endregion IOutcomeList Members

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeList -");

            builder.AppendFormat("\t  < {0} Feature Entries >{1}", FeatureEntry.Count, Environment.NewLine);
            for(var i = 0; i < FeatureEntry.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", FeatureEntry[i]);
            }
            builder.AppendLine("\t  < Feature Entries End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Game Cycle Entries >{1}", GameCycleEntry.Count, Environment.NewLine);
            for(var i = 0; i < GameCycleEntry.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", GameCycleEntry[i]);
            }
            builder.AppendLine("\t  < Game Cycle Entries End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, FeatureEntry);
            CompactSerializer.WriteList(stream, GameCycleEntry);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            FeatureEntry = CompactSerializer.ReadListSerializable<OutcomeListFeatureEntry>(stream);
            GameCycleEntry = CompactSerializer.ReadListSerializable<OutcomeListGameCycleEntry>(stream);
        }

        #endregion
    }
}
