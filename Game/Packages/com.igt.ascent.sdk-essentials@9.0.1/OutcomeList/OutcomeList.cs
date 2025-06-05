//-----------------------------------------------------------------------
// <copyright file = "OutcomeList.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Game.Core.CompactSerialization;
    using Interfaces;

    /// <summary>
    /// This class defines outcome/award lists to communicate with the Foundation 
    /// for evaluation and potential adjustment.
    /// </summary>
    [Serializable]
    public class OutcomeList : IOutcomeList, ICompactSerializable
    {
        private List<FeatureEntry> featureEntries = new List<FeatureEntry>();
        private List<GameCycleEntry> gameCycleEntries = new List<GameCycleEntry>();

        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public OutcomeList()
        {
        }

        /// <summary>
        /// Constructor.
        /// Creates a new OutcomeList from an existing IOutcomeList (IE. to convert 
        /// from F2X.Schemas.OutcomeList to IGT.Ascent.OutcomeList).
        /// </summary>
        /// <param name="outcomeList">
        /// An implementation of <see cref="IOutcomeList"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="outcomeList"/> is null.
        /// </exception>
        public OutcomeList(IOutcomeList outcomeList)
        {
            if(outcomeList == null)
            {
                throw new ArgumentNullException(nameof(outcomeList));
            }

            var featureEntryList = outcomeList.GetFeatureEntries();
            var gameEntryList = outcomeList.GetGameCycleEntries();

            if(featureEntryList != null)
            {
                featureEntries = featureEntryList.Select(entry => new FeatureEntry(entry)).ToList();
            }

            if(gameEntryList != null)
            {
                gameCycleEntries = gameEntryList.Select(entry => new GameCycleEntry(entry)).ToList();
            }
        }

        /// <summary>
        /// Constructor.
        /// Creates a new OutcomeList with feature and/or gameCycle entries.
        /// </summary>
        /// <param name="featureEntries"></param>
        /// <param name="gameCycleEntries"></param>
        public OutcomeList(IEnumerable<FeatureEntry> featureEntries = null, IEnumerable<GameCycleEntry> gameCycleEntries = null)
        {
            if(featureEntries != null)
            {
                this.featureEntries = featureEntries.Select(entry => new FeatureEntry(entry)).ToList();
            }

            if(gameCycleEntries != null)
            {
                this.gameCycleEntries = gameCycleEntries.Select(entry => new GameCycleEntry(entry)).ToList();
            }
        }

        #endregion Constructors

        #region public methods

        /// <summary>
        /// Merges an <see cref="IOutcomeList"/> into this outcome list.
        /// </summary>
        /// <param name="other">
        /// The <see cref="IOutcomeList"/> to be merged into this one.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="other"/> is null.
        /// </exception>
        public void Merge(IOutcomeList other)
        {
            if(other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            featureEntries.AddRange(other.GetFeatureEntries().Select(fe => new FeatureEntry(fe)));
            gameCycleEntries.AddRange(other.GetGameCycleEntries().Select(ge => new GameCycleEntry(ge)));
        }

        /// <summary>
        /// Removes the <see cref="FeatureEntry"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index of the <see cref="FeatureEntry"/> to be removed.
        /// </param>
        public void RemoveFeatureEntry(int index)
        {
            featureEntries.RemoveAt(index);
        }

        /// <summary>
        /// Removes the <see cref="GameCycleEntry"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index of the <see cref="GameCycleEntry"/> to be removed.
        /// </param>
        public void RemoveGameCycleEntry(int index)
        {
            gameCycleEntries.RemoveAt(index);
        }

        /// <summary>
        /// Updates the Feature Entry at the specified index.
        /// </summary>
        /// <param name="index">
        /// Index of the Feature Entry to be removed.
        /// </param>
        /// <param name="newEntry"></param>
        public void UpdateFeatureEntry(int index, FeatureEntry newEntry)
        {
            featureEntries[index] = newEntry;
        }

        /// <summary>
        /// Adds a Feature Entry to the Outcome List.
        /// </summary>
        /// <param name="entry">
        /// The Feature Entry to be added.
        /// </param>
        public void AddFeatureEntry(FeatureEntry entry)
        {
            featureEntries.Add(entry);
        }

        /// <summary>
        /// Adds a list of Feature Entries to the Outcome List.
        /// </summary>
        /// <param name="entries">
        /// List of feature entries to be added.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="entries"/> is null.
        /// </exception>
        public void AddFeatureEntries(IList<FeatureEntry> entries)
        {
            if(entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            featureEntries.AddRange(entries);
        }

        /// <summary>
        /// Adds a Game Cycle Entry to the Outcome List.
        /// </summary>
        /// <param name="entry">
        /// The Feature Entry to be added.
        /// </param>
        public void AddGameCycleEntry(GameCycleEntry entry)
        {
            gameCycleEntries.Add(entry);
        }

        /// <summary>
        /// Adds a list of game cycle entries to the Outcome List.
        /// </summary>
        /// <param name="entries">
        /// List of game cycle entries to be added.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="entries"/> is null.
        /// </exception>
        public void AddGameCycleEntries(IList<GameCycleEntry> entries)
        {
            if(entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            gameCycleEntries.AddRange(entries);
        }

        #endregion public methods

        #region IOutcomeList Members

        /// <inheritdoc />
        public ReadOnlyCollection<IFeatureEntry> GetFeatureEntries()
        {
            return featureEntries.Cast<IFeatureEntry>().ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IGameCycleEntry> GetGameCycleEntries()
        {
            return gameCycleEntries.Cast<IGameCycleEntry>().ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public long GetTotalDisplayableAmount()
        {
            checked
            {
                long total = 0;

                if(featureEntries != null)
                {
                    total += featureEntries.Sum(entry => entry.GetTotalDisplayableAmount());
                }

                if(gameCycleEntries != null)
                {
                    total += gameCycleEntries.Sum(entry => entry.GetTotalDisplayableAmount());
                }

                return total;
            }
        }

        #endregion IOutcomeList Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, featureEntries);
            CompactSerializer.WriteList(stream, gameCycleEntries);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            featureEntries = CompactSerializer.ReadListSerializable<FeatureEntry>(stream);
            gameCycleEntries = CompactSerializer.ReadListSerializable<GameCycleEntry>(stream);
        }

        #endregion

        #region ToString Override

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeList -");

            builder.AppendFormat("\t  < {0} Feature Entries >{1}", featureEntries.Count, Environment.NewLine);
            for(var i = 0; i < featureEntries.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", featureEntries[i]);
            }
            builder.AppendLine("\t  < Feature Entries End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Game Cycle Entries >{1}", gameCycleEntries.Count, Environment.NewLine);
            for(var i = 0; i < gameCycleEntries.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", gameCycleEntries[i]);
            }
            builder.AppendLine("\t  < Game Cycle Entries End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
