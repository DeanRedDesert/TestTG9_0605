//-----------------------------------------------------------------------
// <copyright file = "FeatureAward.cs" company = "IGT">
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
    /// of the automatically generated type FeatureAward.
    /// </summary>
    public partial class FeatureAward : IFeatureAward
    {
        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
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
                throw new ArgumentNullException("award");
            }

            var listProgressiveAwards = award.GetFeatureProgressiveAwards();
            var listProgressiveNearHits = award.GetProgressiveNearHits();

            if(listProgressiveAwards == null || listProgressiveAwards.Count < 1)
            {
                featureProgressiveAwardField = new List<FeatureProgressiveAward>();
            }
            else
            {
                featureProgressiveAwardField = listProgressiveAwards.Select(
                    entry => new FeatureProgressiveAward(entry)).ToList();
            }

            if(listProgressiveNearHits == null || listProgressiveNearHits.Count < 1)
            {
                progressiveNearHitField = new List<ProgressiveNearHit>();
            }
            else
            {
                progressiveNearHitField = listProgressiveNearHits.Select(
                    entry => new ProgressiveNearHit(entry)).ToList();
            }

            win_level_indexField = award.WinLevelIndex;
        }

        /// <inheritdoc />
        public override long GetDisplayableAmount()
        {
            checked
            {
                long total = 0;

                total += base.GetDisplayableAmount();

                if(FeatureProgressiveAward != null)
                {
                    total += FeatureProgressiveAward.Sum(award => award.GetDisplayableAmount());
                }

                return total;
            }
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("FeatureAward -");

            builder.AppendFormat("\t  < {0} Feature Progressive Awards >{1}", FeatureProgressiveAward.Count, Environment.NewLine);
            for(var i = 0; i < FeatureProgressiveAward.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", FeatureProgressiveAward[i]);
            }
            builder.AppendLine("\t  < Feature Progressive Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Progressive Near Hits >{1}", ProgressiveNearHit.Count, Environment.NewLine);
            for(var i = 0; i < ProgressiveNearHit.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", ProgressiveNearHit[i]);
            }
            builder.AppendLine("\t  < Progressive Near Hits End >");
            builder.AppendLine();

            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, win_level_index);
            CompactSerializer.WriteList(stream, FeatureProgressiveAward);
            CompactSerializer.WriteList(stream, ProgressiveNearHit);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            win_level_index = CompactSerializer.ReadUint(stream);
            FeatureProgressiveAward = CompactSerializer.ReadListSerializable<FeatureProgressiveAward>(stream);
            ProgressiveNearHit = CompactSerializer.ReadListSerializable<ProgressiveNearHit>(stream);
        }

        #endregion

        #region IFeatureAward Members

        /// <inheritdoc />
        public uint WinLevelIndex
        {
            get { return win_level_indexField; }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IFeatureProgressiveAward> GetFeatureProgressiveAwards()
        {
            return featureProgressiveAwardField.Cast<IFeatureProgressiveAward>().ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IProgressiveNearHit> GetProgressiveNearHits()
        {
            return progressiveNearHitField.Cast<IProgressiveNearHit>().ToList().AsReadOnly();
        }

        #endregion IFeatureAward Members
    }
}
