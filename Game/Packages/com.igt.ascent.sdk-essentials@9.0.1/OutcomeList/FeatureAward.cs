//-----------------------------------------------------------------------
// <copyright file = "FeatureAward.cs" company = "IGT">
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
    using Interfaces;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// Type used for awards associated with a game feature and payvar.
    /// </summary>
    [Serializable]
    public class FeatureAward : Award, IFeatureAward
    {
        private List<FeatureProgressiveAward> featureProgressiveAwards = new List<FeatureProgressiveAward>();
        private List<ProgressiveNearHit> progressiveNearHits = new List<ProgressiveNearHit>();

        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public FeatureAward()
        {
        }

        /// <summary>
        /// Constructor.  Creates a FeatureAward from an IFeatureAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IFeatureAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public FeatureAward(IFeatureAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            var listProgressiveAwards = award.GetFeatureProgressiveAwards();
            var listProgressiveNearHits = award.GetProgressiveNearHits();

            if(listProgressiveAwards?.Any() == true)
            {
                featureProgressiveAwards = listProgressiveAwards.Select(
                    entry => new FeatureProgressiveAward(entry)).ToList();
            }

            if(listProgressiveNearHits?.Any() == true)
            {
                progressiveNearHits = listProgressiveNearHits.Select(
                    entry => new ProgressiveNearHit(entry)).ToList();
            }

            WinLevelIndex = award.WinLevelIndex;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="FeatureAward"/> using passed in arguments.
        /// </summary>
        /// <param name="amount">
        /// Optional. The amount of the award in base units.
        /// </param>
        /// <param name="isDisplayable">
        /// Optional. Determines if this reward is displayable.
        /// </param>
        /// <param name="reason">
        /// Optional. The reason to display to the user for the award.
        /// </param>
        /// <param name="prize">
        /// Optional. The displayable prize string.
        /// </param>
        /// <param name="isTopAward">
        /// Optional. Determines if this is the top award available. Defaults to false.
        /// </param>
        /// <param name="winLevel">
        /// The associated win level for this award.
        /// </param>
        /// <param name="origin">
        /// The <see cref="OutcomeOrigin"/> for this outcome.
        /// </param>
        /// <param name="tag">
        /// Optional. The identifying tag for the outcome. Defaults to null.
        /// </param>
        /// <param name="source">
        /// Optional. The source field of who created this outcome. Defaults to null.
        /// </param>
        /// <param name="sourceDetail">
        /// Optional. Additional details for the source of this outcome. Defaults to null.
        /// </param>
        /// <param name="featureProgressiveAwards">
        /// Optional. The collection of <see cref="FeatureProgressiveAward"/>s. Defaults to null.
        /// </param>
        /// <param name="progressiveNearHits">
        /// Optional. The collection of <see cref="ProgressiveNearHit"/>s. Defaults to null.
        /// </param>
        public FeatureAward(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long amount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            uint winLevel = 0,
            bool isTopAward = false,
            string tag = null,
            string source = null,
            string sourceDetail = null,
            IEnumerable<FeatureProgressiveAward> featureProgressiveAwards = null,
            IEnumerable<ProgressiveNearHit> progressiveNearHits = null)
            : base(origin, amount, isDisplayable, reason, prize, isTopAward, tag, source, sourceDetail)
        {
            WinLevelIndex = winLevel;

            if(featureProgressiveAwards != null)
            {
                this.featureProgressiveAwards = featureProgressiveAwards.ToList();
            }

            if(progressiveNearHits != null)
            {
                this.progressiveNearHits = progressiveNearHits.ToList();
            }
        }

        #endregion Constructors
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="award"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateFeatureProgressiveAward(int index, FeatureProgressiveAward award)
        {
            featureProgressiveAwards[index] = award ?? throw new ArgumentNullException(nameof(award));
        }

        /// <summary>
        /// Adds a Feature Progressive Award to the Feature Award.
        /// </summary>
        /// <param name="award">The Feature Progressive Award to be added.</param>
        /// <returns>
        /// A new instance with the FeatureProgressiveAwards list updated
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public void AddFeatureProgressiveAward(FeatureProgressiveAward award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            featureProgressiveAwards.Add(award);
        }

        /// <summary>
        /// Adds a collection of <see cref="FeatureProgressiveAward"/>s to the Feature Award.
        /// </summary>
        /// <param name="awards"></param>
        /// <returns>
        /// A new instance with the FeatureProgressiveAwards list updated
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="awards"/> is null.
        /// </exception>
        public void AddFeatureProgressiveAwards(IEnumerable<FeatureProgressiveAward> awards)
        {
            if(awards == null)
            {
                throw new ArgumentNullException(nameof(awards));
            }

            featureProgressiveAwards.AddRange(awards);
        }

        /// <summary>
        /// Removes the Feature Progressive Award at the specified index.
        /// </summary>
        /// <param name="index">Index of the Feature Progressive Award to be removed.</param>
        public void RemoveFeatureProgressiveAward(int index)
        {
            featureProgressiveAwards.RemoveAt(index);
        }

        /// <summary>
        /// Adds a Progressive Near Hit to the Feature Award.
        /// </summary>
        /// <param name="nearHit">The Progressive Near Hit to be added.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nearHit"/> is null.
        /// </exception>
        public void AddProgressiveNearHit(ProgressiveNearHit nearHit)
        {
            if(nearHit == null)
            {
                throw new ArgumentNullException(nameof(nearHit));
            }

            progressiveNearHits.Add(nearHit);
        }

        /// <summary>
        /// Removes the Progressive Near Hit at the specified index.
        /// </summary>
        /// <param name="index">Index of the Progressive Near Hit to be removed.</param>
        public void RemoveProgressiveNearHit(int index)
        {
            progressiveNearHits.RemoveAt(index);
        }

        #region IFeatureAward Members

        /// <inheritdoc />
        public uint WinLevelIndex { get; private set; }

        /// <inheritdoc />
        public ReadOnlyCollection<IFeatureProgressiveAward> GetFeatureProgressiveAwards()
        {
            var entries = featureProgressiveAwards.Cast<IFeatureProgressiveAward>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IProgressiveNearHit> GetProgressiveNearHits()
        {
            var entries = progressiveNearHits.Cast<IProgressiveNearHit>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public override long GetDisplayableAmount()
        {
            checked
            {
                var total = base.GetDisplayableAmount();

                if(featureProgressiveAwards != null)
                {
                    total += featureProgressiveAwards.Sum(award => award.GetDisplayableAmount());
                }

                return total;
            }
        }

        #endregion IFeatureAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, WinLevelIndex);
            CompactSerializer.WriteList(stream, featureProgressiveAwards);
            CompactSerializer.WriteList(stream, progressiveNearHits);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            WinLevelIndex = CompactSerializer.ReadUint(stream);
            featureProgressiveAwards = CompactSerializer.ReadListSerializable<FeatureProgressiveAward>(stream);
            progressiveNearHits = CompactSerializer.ReadListSerializable<ProgressiveNearHit>(stream);
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

            builder.AppendLine("FeatureAward -");

            builder.AppendFormat("\t  < {0} Feature Progressive Awards >{1}", featureProgressiveAwards.Count, Environment.NewLine);
            for(var i = 0; i < featureProgressiveAwards.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", featureProgressiveAwards[i]);
            }
            builder.AppendLine("\t  < Feature Progressive Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Progressive Near Hits >{1}", progressiveNearHits.Count, Environment.NewLine);
            for(var i = 0; i < progressiveNearHits.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", progressiveNearHits[i]);
            }
            builder.AppendLine("\t  < Progressive Near Hits End >");
            builder.AppendLine();

            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
