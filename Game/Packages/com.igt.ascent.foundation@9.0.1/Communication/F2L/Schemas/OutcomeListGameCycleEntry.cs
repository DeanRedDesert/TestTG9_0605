//-----------------------------------------------------------------------
// <copyright file = "OutcomeListGameCycleEntry.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

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
            awardField = new List<Award>();
            progressiveAwardField = new List<ProgressiveAward>();
            bonusExtensionAwardField = new List<BonusExtensionAward>();
        }

        /// <summary>
        /// Constructor.  Converts an IGameCycleEntry into a GameCycleEntry.
        /// </summary>
        /// <param name="gameCycleEntry">An implementation of <see cref="IGameCycleEntry"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameCycleEntry"/> is null.
        /// </exception>
        public OutcomeListGameCycleEntry(IGameCycleEntry gameCycleEntry)
        {
            if(gameCycleEntry == null)
            {
                throw new ArgumentNullException("gameCycleEntry");
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

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeListGameCycleEntry -");

            builder.AppendFormat("\t  < {0} Awards >{1}", Award.Count, Environment.NewLine);
            for(int i = 0; i < Award.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", Award[i]);
            }
            builder.AppendLine("\t  < Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Progressive Awards >{1}", ProgressiveAward.Count, Environment.NewLine);
            for(var i = 0; i < ProgressiveAward.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", ProgressiveAward[i]);
            }
            builder.AppendLine("\t  < Progressive Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Bonus Extension Awards >{1}", BonusExtensionAward.Count, Environment.NewLine);
            for(var i = 0; i < BonusExtensionAward.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", BonusExtensionAward[i]);
            }
            builder.AppendLine("\t  < Bonus Extension Awards End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Award);
            CompactSerializer.WriteList(stream, ProgressiveAward);
            CompactSerializer.WriteList(stream, BonusExtensionAward);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Award = CompactSerializer.ReadListSerializable<Award>(stream);
            ProgressiveAward = CompactSerializer.ReadListSerializable<ProgressiveAward>(stream);
            BonusExtensionAward = CompactSerializer.ReadListSerializable<BonusExtensionAward>(stream);
        }

        #endregion
    }
}
