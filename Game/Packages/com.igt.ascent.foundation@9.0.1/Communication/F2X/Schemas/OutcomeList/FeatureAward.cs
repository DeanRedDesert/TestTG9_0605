//-----------------------------------------------------------------------
// <copyright file = "FeatureAward.cs" company = "IGT">
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
    /// of the automatically generated type FeatureAward.
    /// </summary>
    public partial class FeatureAward : IFeatureAward
    {
        #region Constructors

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
                throw new ArgumentNullException(nameof(award));
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

            WinLevelIndex = award.WinLevelIndex;
        }

        #endregion Constructors
        
        #region IFeatureAward Members
        
        /// <inheritdoc />
        public ReadOnlyCollection<IFeatureProgressiveAward> GetFeatureProgressiveAwards()
        {
            var entries = featureProgressiveAwardField.Cast<IFeatureProgressiveAward>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IProgressiveNearHit> GetProgressiveNearHits()
        {
            var entries = progressiveNearHitField.Cast<IProgressiveNearHit>().ToList();

            return entries.AsReadOnly();
        }

        /// <inheritdoc />
        public override long GetDisplayableAmount()
        {
            checked
            {
                var total = base.GetDisplayableAmount();

                if(featureProgressiveAwardField != null)
                {
                    total += featureProgressiveAwardField.Sum(award => award.GetDisplayableAmount());
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
            CompactSerializer.WriteList(stream, featureProgressiveAwardField);
            CompactSerializer.WriteList(stream, progressiveNearHitField);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            WinLevelIndex = CompactSerializer.ReadUint(stream);
            featureProgressiveAwardField = CompactSerializer.ReadListSerializable<FeatureProgressiveAward>(stream);
            progressiveNearHitField = CompactSerializer.ReadListSerializable<ProgressiveNearHit>(stream);
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

            builder.AppendFormat("\t  < {0} Feature Progressive Awards >{1}", featureProgressiveAwardField.Count, Environment.NewLine);
            for(var i = 0; i < featureProgressiveAwardField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", featureProgressiveAwardField[i]);
            }
            builder.AppendLine("\t  < Feature Progressive Awards End >");
            builder.AppendLine();

            builder.AppendFormat("\t  < {0} Progressive Near Hits >{1}", progressiveNearHitField.Count, Environment.NewLine);
            for(var i = 0; i < progressiveNearHitField.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", progressiveNearHitField[i]);
            }
            builder.AppendLine("\t  < Progressive Near Hits End >");
            builder.AppendLine();

            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
