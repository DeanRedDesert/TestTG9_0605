//-----------------------------------------------------------------------
// <copyright file = "GameCycleEntry.cs" company = "IGT">
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
    /// Contains awards that are NOT associated with a specific Feature or payvar's win level index. 
    /// May be used for win capping or external bonusing, including mystery progressives.
    /// </summary>
    [Serializable]
    public class GameCycleEntry : IGameCycleEntry, ICompactSerializable
    {
        private List<Award> awards = new List<Award>();
        private List<ProgressiveAward> progressiveAwards = new List<ProgressiveAward>();
        private List<BonusExtensionAward> bonusExtensionAwards = new List<BonusExtensionAward>();

        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public GameCycleEntry()
        {
        }

        /// <summary>
        /// Constructor.  Converts an IGameCycleEntry into a GameCycleEntry.
        /// </summary>
        /// <param name="gameCycleEntry">An implementation of <see cref="IGameCycleEntry"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameCycleEntry"/> is null.
        /// </exception>
        public GameCycleEntry(IGameCycleEntry gameCycleEntry)
        {
            if(gameCycleEntry == null)
            {
                throw new ArgumentNullException(nameof(gameCycleEntry));
            }

            var awardsList = gameCycleEntry.GetAwards();
            var progAwardsList = gameCycleEntry.GetProgressiveAwards();
            var bonusExtAwardsList = gameCycleEntry.GetBonusExtensionAwards();

            if(awardsList?.Any() == true)
            {
                awards = awardsList.Select(entry => new Award(entry)).ToList();
            }

            if(progAwardsList?.Any() == true)
            {
                progressiveAwards = progAwardsList.Select(entry => new ProgressiveAward(entry)).ToList();
            }

            if(bonusExtAwardsList?.Any() == true)
            {
                bonusExtensionAwards = bonusExtAwardsList.Select(entry => new BonusExtensionAward(entry)).ToList();
            }
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="GameCycleEntry"/> using passed in arguments.
        /// </summary>
        /// <param name="awards">
        /// Optional. The <see cref="Award"/>s.
        /// </param>
        /// <param name="progressiveAwards">
        /// Optional. The <see cref="ProgressiveAward"/>s.
        /// </param>
        /// <param name="bonusExtensionAwards">
        /// Optional. The <see cref="BonusExtensionAward"/>s.
        /// </param>
        public GameCycleEntry(
            IEnumerable<Award> awards = null,
            IEnumerable<ProgressiveAward> progressiveAwards = null,
            IEnumerable<BonusExtensionAward> bonusExtensionAwards = null)
        {
            if(awards != null)
            {
                this.awards = awards.ToList();
            }

            if(progressiveAwards != null)
            {
                this.progressiveAwards = progressiveAwards.ToList();
            }

            if(bonusExtensionAwards != null)
            {
                this.bonusExtensionAwards = bonusExtensionAwards.ToList();
            }
        }

        #endregion Constructors

        /// <summary>
        /// Adds an Award to the Game Cycle Entry.
        /// </summary>
        /// <param name="award">The Award to be added.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public void AddAward(Award award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            awards.Add(award);
        }

        /// <summary>
        /// Removes the Award at the specified index.
        /// </summary>
        /// <param name="index">Index of the Award to be removed.</param>
        public void RemoveAward(int index)
        {
            awards.RemoveAt(index);
        }

        /// <summary>
        /// Adds a Progressive Award to the Game Cycle Entry.
        /// </summary>
        /// <param name="award">The Progressive Award to be added.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public void AddProgressiveAward(ProgressiveAward award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            progressiveAwards.Add(award);
        }

        /// <summary>
        /// Removes the Progressive Award at the specified index.
        /// </summary>
        /// <param name="index">Index of the Progressive Award to be removed.</param>
        public void RemoveProgressiveAward(int index)
        {
            progressiveAwards.RemoveAt(index);
        }

        /// <summary>
        /// Adds a Bonus Extension Award to the Game Cycle Entry.
        /// </summary>
        /// <param name="award">The Bonus Extension Award to be added.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public void AddBonusExtensionAward(BonusExtensionAward award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            bonusExtensionAwards.Add(award);
        }

        /// <summary>
        /// Removes the Bonus Extension Award at the specified index.
        /// </summary>
        /// <param name="index">Index of the Bonus Extension Award to be removed.</param>
        public void RemoveBonusExtensionAward(int index)
        {
            bonusExtensionAwards.RemoveAt(index);
        }

        #region IGameCycleEntry Members

        /// <inheritdoc />
        public long GetTotalDisplayableAmount()
        {
            checked
            {
                long total = 0;

                total += awards.Sum(award => award.GetDisplayableAmount());

                total += progressiveAwards.Sum(award => award.GetDisplayableAmount());

                total += bonusExtensionAwards.Sum(award => award.GetDisplayableAmount());

                return total;
            }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IAward> GetAwards()
        {
            var entries = awards.Cast<IAward>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IProgressiveAward> GetProgressiveAwards()
        {
            var entries = progressiveAwards.Cast<IProgressiveAward>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IBonusExtensionAward> GetBonusExtensionAwards()
        {
            var entries = bonusExtensionAwards.Cast<IBonusExtensionAward>().ToList();

            return entries.AsReadOnly();
        }

        #endregion IGameCycleEntry Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, awards);
            CompactSerializer.WriteList(stream, progressiveAwards);
            CompactSerializer.WriteList(stream, bonusExtensionAwards);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            awards = CompactSerializer.ReadListSerializable<Award>(stream);
            progressiveAwards = CompactSerializer.ReadListSerializable<ProgressiveAward>(stream);
            bonusExtensionAwards = CompactSerializer.ReadListSerializable<BonusExtensionAward>(stream);
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

            builder.AppendFormat("\t  < {0} Awards >{1}", awards.Count, Environment.NewLine);
            for(var i = 0; i < awards.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", awards[i]);
            }
            builder.AppendLine("\t  < Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Progressive Awards >{1}", progressiveAwards.Count, Environment.NewLine);
            for(var i = 0; i < progressiveAwards.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", progressiveAwards[i]);
            }
            builder.AppendLine("\t  < Progressive Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Bonus Extension Awards >{1}", bonusExtensionAwards.Count, Environment.NewLine);
            for(var i = 0; i < bonusExtensionAwards.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", bonusExtensionAwards[i]);
            }
            builder.AppendLine("\t  < Bonus Extension Awards End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
