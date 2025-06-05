//-----------------------------------------------------------------------
// <copyright file = "OutcomeListGameCycleEntry.cs" company = "IGT">
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
    /// This class contains methods which extend the functionality
    /// of the automatically generated type OutcomeListGameCycleEntry.
    /// </summary>
    public partial class OutcomeListGameCycleEntry : IGameCycleEntry, ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public OutcomeListGameCycleEntry()
        {
        }

        /// <summary>
        /// Constructor.  Converts an IGameCycleEntry into an OutcomeListGameCycleEntry.
        /// </summary>
        /// <param name="gameCycleEntry">
        /// An implementation of <see cref="IGameCycleEntry"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameCycleEntry"/> is null.
        /// </exception>
        public OutcomeListGameCycleEntry(IGameCycleEntry gameCycleEntry)
        {
            if(gameCycleEntry == null)
            {
                throw new ArgumentNullException(nameof(gameCycleEntry));
            }

            var awardsList = gameCycleEntry.GetAwards();
            var progAwardsList = gameCycleEntry.GetProgressiveAwards();
            var bonusExtAwardsList = gameCycleEntry.GetBonusExtensionAwards();

            if(awardsList == null || awardsList.Count < 1)
            {
                awardField = new List<Award>();
            }
            else
            {
                awardField = awardsList.Select(
                    entry => new Award(entry)).ToList();
            }

            if(progAwardsList == null || progAwardsList.Count < 1)
            {
                progressiveAwardField = new List<ProgressiveAward>();
            }
            else
            {
                progressiveAwardField = progAwardsList.Select(
                    entry => new ProgressiveAward(entry)).ToList();
            }

            if(bonusExtAwardsList == null || bonusExtAwardsList.Count < 1)
            {
                bonusExtensionAwardField = new List<BonusExtensionAward>();
            }
            else
            {
                bonusExtensionAwardField = bonusExtAwardsList.Select(
                    entry => new BonusExtensionAward(entry)).ToList();
            }
        }

        #endregion Constructors
        
        #region IGameCycleEntry Members
        
        /// <inheritdoc />
        public long GetTotalDisplayableAmount()
        {
            checked
            {
                var total = awardField.Sum(award => award.GetDisplayableAmount());

                total += progressiveAwardField.Sum(award => award.GetDisplayableAmount());

                total += bonusExtensionAwardField.Sum(award => award.GetDisplayableAmount());

                return total;
            }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IAward> GetAwards()
        {
            var entries = awardField.Cast<IAward>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IProgressiveAward> GetProgressiveAwards()
        {
            var entries = progressiveAwardField.Cast<IProgressiveAward>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IBonusExtensionAward> GetBonusExtensionAwards()
        {
            var entries = bonusExtensionAwardField.Cast<IBonusExtensionAward>().ToList();

            return entries.AsReadOnly();
        }

        #endregion IGameCycleEntry Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, awardField);
            CompactSerializer.WriteList(stream, progressiveAwardField);
            CompactSerializer.WriteList(stream, bonusExtensionAwardField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            awardField = CompactSerializer.ReadListSerializable<Award>(stream);
            progressiveAwardField = CompactSerializer.ReadListSerializable<ProgressiveAward>(stream);
            bonusExtensionAwardField = CompactSerializer.ReadListSerializable<BonusExtensionAward>(stream);
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

            builder.AppendLine("OutcomeListGameCycleEntry -");

            builder.AppendFormat("\t  < {0} Awards >{1}", awardField.Count, Environment.NewLine);
            for(int i = 0; i < awardField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", awardField[i]);
            }
            builder.AppendLine("\t  < Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Progressive Awards >{1}", progressiveAwardField.Count, Environment.NewLine);
            for(var i = 0; i < progressiveAwardField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", progressiveAwardField[i]);
            }
            builder.AppendLine("\t  < Progressive Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Bonus Extension Awards >{1}", bonusExtensionAwardField.Count, Environment.NewLine);
            for(var i = 0; i < bonusExtensionAwardField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", bonusExtensionAwardField[i]);
            }
            builder.AppendLine("\t  < Bonus Extension Awards End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
