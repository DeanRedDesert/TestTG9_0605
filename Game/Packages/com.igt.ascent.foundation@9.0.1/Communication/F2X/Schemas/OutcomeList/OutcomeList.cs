//-----------------------------------------------------------------------
// <copyright file = "OutcomeList.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CompactSerialization;
    using IGT.Ascent.OutcomeList.Interfaces;

    /// <summary>
    /// This class contains methods which extend the functionality of
    /// the automatically generated OutcomeList class.
    /// </summary>
    public partial class OutcomeList : IOutcomeList, ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public OutcomeList()
        {

        }

        /// <summary>
        /// Constructor.
        /// Creates a new OutcomeList from an existing IOutcomeList implementation.
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

            if(featureEntryList == null || featureEntryList.Count < 1)
            {
                featureEntryField = new List<OutcomeListFeatureEntry>();
            }
            else
            {
                featureEntryField = featureEntryList.Select(
                    entry => new OutcomeListFeatureEntry(entry)).ToList();
            }

            if(gameEntryList == null || gameEntryList.Count < 1)
            {
                gameCycleEntryField = new List<OutcomeListGameCycleEntry>();
            }
            else
            {
                gameCycleEntryField = gameEntryList.Select(
                    entry => new OutcomeListGameCycleEntry(entry)).ToList();
            }
        }

        #endregion Constructors
        
        #region IOutcomeList Members

        /// <inheritdoc />
        public ReadOnlyCollection<IFeatureEntry> GetFeatureEntries()
        {
            var entries = featureEntryField.Cast<IFeatureEntry>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IGameCycleEntry> GetGameCycleEntries()
        {
            var entries = gameCycleEntryField.Cast<IGameCycleEntry>().ToList();

            return entries.AsReadOnly();
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

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, featureEntryField);
            CompactSerializer.WriteList(stream, gameCycleEntryField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            featureEntryField = CompactSerializer.ReadListSerializable<OutcomeListFeatureEntry>(stream);
            gameCycleEntryField = CompactSerializer.ReadListSerializable<OutcomeListGameCycleEntry>(stream);
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

            builder.AppendFormat("\t  < {0} Feature Entries >{1}", featureEntryField.Count, Environment.NewLine);
            for(var i = 0; i < featureEntryField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", featureEntryField[i]);
            }
            builder.AppendLine("\t  < Feature Entries End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Game Cycle Entries >{1}", gameCycleEntryField.Count, Environment.NewLine);
            for(var i = 0; i < gameCycleEntryField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", gameCycleEntryField[i]);
            }
            builder.AppendLine("\t  < Game Cycle Entries End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
