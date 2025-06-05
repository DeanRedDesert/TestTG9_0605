//-----------------------------------------------------------------------
// <copyright file = "OutcomeListFeatureEntry.cs" company = "IGT">
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
    /// of the automatically generated type OutcomeListFeatureEntry.
    /// </summary>
    public partial class OutcomeListFeatureEntry : IFeatureEntry, ICompactSerializable
    {
        /// <summary>
        /// The names of the award types that this class can contain. All awards must be of the same type.
        /// </summary>
        private static readonly HashSet<string> SupportedAwardTypes = new HashSet<string>
        {
            typeof(FeatureAward).Name,
            typeof(AncillaryAward).Name,
            typeof(RiskAward).Name
        };

        #region Constructors

        /// <summary>
        /// Public parameter-less constructor for ICompactSerializable.
        /// </summary>
        public OutcomeListFeatureEntry()
        {
            awardsField = new OutcomeListFeatureEntryAwards();
        }

        /// <summary>
        /// Constructor.  Converts an IFeatureEntry into a OutcomeListFeatureEntry.
        /// </summary>
        /// <param name="featureEntry">
        /// An implementation of <see cref="IFeatureEntry"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureEntry"/> is null.
        /// </exception>
        public OutcomeListFeatureEntry(IFeatureEntry featureEntry)
        {
            if(featureEntry == null)
            {
                throw new ArgumentNullException(nameof(featureEntry));
            }

            var listRng = featureEntry.GetFeatureRngNumbers();
            var listFeatureAwards = featureEntry.GetAwards<IFeatureAward>();
            var listAncillaryAwards = featureEntry.GetAwards<IAncillaryAward>();
            var listRiskAwards = featureEntry.GetAwards<IRiskAward>();

            featureRngNumbersField = new OutcomeListFeatureEntryFeatureRngNumbers();
            awardsField = new OutcomeListFeatureEntryAwards();

            foreach(var entry in listRng)
            {
                featureRngNumbersField.Number.Add(entry);
            }

            foreach(var entry in listFeatureAwards)
            {
                var newEntry = new FeatureAward(entry);
                awardsField.Items.Add(newEntry);
            }

            foreach(var entry in listAncillaryAwards)
            {
                var newEntry = new AncillaryAward(entry);
                awardsField.Items.Add(newEntry);
            }

            foreach(var entry in listRiskAwards)
            {
                var newEntry = new RiskAward(entry);
                awardsField.Items.Add(newEntry);
            }

            FeatureIndex = featureEntry.FeatureIndex;
        }

        #endregion Constructors
        
        #region IFeatureEntry Members

        /// <inheritdoc />
        public bool ContainsFeatureAwardOutcome
        {
            get
            {
                var result = false;

                if(awardsField.Items.Any())
                {
                    // Check if the first award is an FeatureAward.
                    // The awards in the list are supposed to be of the same type.
                    result = awardsField.Items[0] is FeatureAward;
                }

                return result;
            }
        }

        /// <inheritdoc />
        public bool ContainsAncillaryAwardOutcome
        {
            get
            {
                var result = false;

                if(awardsField.Items.Any())
                {
                    // Check if the first award is an AncillaryAward.
                    // The awards in the list are supposed to be of the same type.
                    result = awardsField.Items[0] is AncillaryAward;
                }

                return result;
            }
        }

        /// <inheritdoc />
        public bool ContainsRiskAwardOutcome
        {
            get
            {
                var result = false;

                if(awardsField.Items.Any())
                {
                    // Check if the first award is a RiskAward.
                    // The awards in the list are supposed to be of the same type.
                    result = awardsField.Items[0] is RiskAward;
                }

                return result;
            }
        }

        /// <inheritdoc />
        public long GetTotalDisplayableAmount()
        {
            return awardsField.Items.Sum(award => award.GetDisplayableAmount());
        }

        /// <inheritdoc />
        public ReadOnlyCollection<int> GetFeatureRngNumbers()
        {
            return featureRngNumbersField.Number.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<T> GetAwards<T>()
        {
            return awardsField.Items.OfType<T>().ToList().AsReadOnly();
        }

        #endregion IFeatureEntry Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        /// <exception cref="CompactSerializationException">
        /// Thrown if the feature entry contains an unsupported award type.
        /// </exception>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, FeatureIndex);
            CompactSerializer.Write(stream, featureRngNumbersField);

            // Write the award type name.
            var awardType = GetAwardType();
            CompactSerializer.Write(stream, awardType);

            // Make sure that there is existing valid awards within this feature entry,
            // otherwise serialize nothing afterwards.
            if(!string.IsNullOrEmpty(awardType))
            {
                if(!SupportedAwardTypes.Contains(awardType))
                {
                    var message =
                        $"This feature entry contains an unsupported award type {awardType}, and cannot be serialized. Please " +
                        $"ensure to only use one of the following award types in feature entries: {string.Join(", ", SupportedAwardTypes.ToArray())}.";
                    throw new CompactSerializationException(message);
                }
                CompactSerializer.WriteList(stream, awardsField.Items);
            }
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            FeatureIndex = CompactSerializer.ReadUint(stream);
            featureRngNumbersField = CompactSerializer.ReadSerializable<OutcomeListFeatureEntryFeatureRngNumbers>(stream);

            // Read the award type name.
            var awardType = CompactSerializer.ReadString(stream);

            // Read the award list accordingly, for award type not supported, leave it empty.
            awardsField.Items.Clear();
            if(awardType == typeof(FeatureAward).Name)
            {
                var featureAwards = CompactSerializer.ReadListSerializable<FeatureAward>(stream);
                foreach(var featureAward in featureAwards)
                {
                    awardsField.Items.Add(featureAward);
                }
            }
            else if(awardType == typeof(AncillaryAward).Name)
            {
                var ancillaryAwards = CompactSerializer.ReadListSerializable<AncillaryAward>(stream);
                foreach(var ancillaryAward in ancillaryAwards)
                {
                    awardsField.Items.Add(ancillaryAward);
                }
            }
            else if(awardType == typeof(RiskAward).Name)
            {
                var riskAwards = CompactSerializer.ReadListSerializable<RiskAward>(stream);
                foreach(var riskAward in riskAwards)
                {
                    awardsField.Items.Add(riskAward);
                }
            }
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

            builder.AppendLine("FeatureEntry -");

            builder.AppendLine("\t  Feature Index = " + FeatureIndex);
            if(featureRngNumbersField != null)
            {
                builder.Append("\t  " + featureRngNumbersField);
            }

            var awardType = GetAwardType();

            builder.AppendFormat("\t  < {0} {1} >{2}", awardsField.Items.Count, awardType, Environment.NewLine);
            for(var i = 0; i < awardsField.Items.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", awardsField.Items[i]);
            }
            builder.AppendFormat("\t  < {0} End >{1}", awardType, Environment.NewLine);
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion ToString Override

        #region Private Functions

        /// <summary>
        /// Retrieves the type name string to indicate the awards type that this outcome list feature entry contains.
        /// </summary>
        /// <returns>The type name string to indicate the awards type, empty string for an empty award list.</returns>
        private string GetAwardType()
        {
            return awardsField.Items.Any() ? awardsField.Items[0].GetType().Name : string.Empty;
        }

        #endregion
    }
}
