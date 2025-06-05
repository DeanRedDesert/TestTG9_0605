//-----------------------------------------------------------------------
// <copyright file = "OutcomeListFeatureEntry.cs" company = "IGT">
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
    /// of the automatically generated type OutcomeListFeatureEntry.
    /// </summary>
    public partial class OutcomeListFeatureEntry : IFeatureEntry, ICompactSerializable
    {
        /// <summary>
        /// Provides an empty collection as return value so we can avoid creating new objects more than once.
        /// </summary>
        private static readonly ReadOnlyCollection<int> EmptyReadonlyList = new ReadOnlyCollection<int>(new List<int>());

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
        /// Constructor.  Converts an IFeatureEntry into a FeatureEntry.
        /// </summary>
        /// <param name="featureEntry">An implementation of <see cref="IFeatureEntry"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureEntry"/> is null.
        /// </exception>
        public OutcomeListFeatureEntry(IFeatureEntry featureEntry)
        {
            if(featureEntry == null)
            {
                throw new ArgumentNullException("featureEntry");
            }

            awardsField = new OutcomeListFeatureEntryAwards();

            if(featureEntry.ContainsAncillaryAwardOutcome)
            {
                awardsField.Items = featureEntry.GetAwards<IAncillaryAward>().Select(
                    entry => new AncillaryAward(entry) as Award).ToList();

            }
            else if(featureEntry.ContainsFeatureAwardOutcome)
            {
                awardsField.Items = featureEntry.GetAwards<IFeatureAward>().Select(
                    entry => new FeatureAward(entry) as Award).ToList();
            }
            else if(featureEntry.ContainsRiskAwardOutcome)
            {
                awardsField.Items = featureEntry.GetAwards<IRiskAward>().Select(
                    entry => new RiskAward(entry) as Award).ToList();
            }

            var featureRngs = featureEntry.GetFeatureRngNumbers().ToList();
            featureRngNumbersField = featureRngs.Any() ? new OutcomeListFeatureEntryFeatureRngNumbers { Number = featureRngs } : null;

            feature_indexField = featureEntry.FeatureIndex;
        }

        #endregion Constructors

        #region public methods

        /// <summary>
        /// Add an Award to the Feature Entry.
        /// </summary>
        /// <param name="award">The Award to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="award"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the type of <paramref name="award"/> is not supported in a feature entry.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a Feature/Ancillary Award is added to a list that contains
        /// awards of different type.
        /// </exception>
        public void AddAward(Award award)
        {
            if(award == null)
            {
                throw new ArgumentNullException("award");
            }
            if(!SupportedAwardTypes.Contains(award.GetType().Name))
            {
                var message = string.Format("The award type {0} is not supported.", award.GetType().Name);
                throw new ArgumentException(message, "award");
            }
            if(Awards.Items.Any() && award.GetType().Name != GetAwardType())
            {
                throw new InvalidOperationException(
                    string.Format("{0} cannot be added. The award list already contains {1}.",
                        award.GetType().Name, GetAwardType()));
            }

            Awards.Items.Add(award);
        }

        #endregion public methods

        #region IFeatureEntry Members

        /// <inheritdoc />
        public uint FeatureIndex
        {
            get { return feature_indexField; }
        }

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
            checked
            {
                return awardsField.Items.Sum(award => award.GetDisplayableAmount());
            }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<int> GetFeatureRngNumbers()
        {
            return featureRngNumbersField == null ? EmptyReadonlyList : featureRngNumbersField.Number.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<T> GetAwards<T>()
        {
            return awardsField.Items.OfType<T>().ToList().AsReadOnly();
        }

        #endregion IFeatureEntry Members

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeListFeatureEntry -");

            builder.AppendLine("\t  Feature Index = " + feature_index);
            if(FeatureRngNumbers != null)
            {
                builder.Append("\t  " + FeatureRngNumbers);
            }

            var awardType = GetAwardType();

            builder.AppendFormat("\t  < {0} {1} >{2}", Awards.Items.Count, awardType, Environment.NewLine);
            for(var i = 0; i < Awards.Items.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", Awards.Items[i]);
            }
            builder.AppendFormat("\t  < {0} End >{1}", awardType, Environment.NewLine);
            builder.AppendLine();

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        /// <exception cref="CompactSerializationException">
        /// Thrown if the feature entry contains an unsupported award type.
        /// </exception>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, feature_index);

            // Write a FeatureRngNumbers indicator.
            CompactSerializer.Write(stream, FeatureRngNumbers != null);

            if(FeatureRngNumbers != null)
            {
                CompactSerializer.Write(stream, FeatureRngNumbers);
            }

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
                        string.Format(
                            "This feature entry contains an unsupported award type {0}, and cannot be serialized. Please " +
                            "ensure to only use one of the following award types in feature entries: {1}.",
                            awardType,
                            string.Join(", ", SupportedAwardTypes.ToArray()));
                    throw new CompactSerializationException(message);
                }
                CompactSerializer.WriteList(stream, Awards.Items);
            }
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            feature_index = CompactSerializer.ReadUint(stream);

            // Read the FeatureRngNumbers indicator first.
            if(CompactSerializer.ReadBool(stream))
            {
                FeatureRngNumbers = CompactSerializer.ReadSerializable<OutcomeListFeatureEntryFeatureRngNumbers>(stream);
            }

            // Read the award type name.
            var awardType = CompactSerializer.ReadString(stream);

            // Read the award list accordingly, for award type not supported, leave it empty.
            Awards.Items.Clear();
            if(awardType == typeof(FeatureAward).Name)
            {
                var featureAwards = CompactSerializer.ReadListSerializable<FeatureAward>(stream);
                foreach(var featureAward in featureAwards)
                {
                    Awards.Items.Add(featureAward);
                }
            }
            else if(awardType == typeof(AncillaryAward).Name)
            {
                var ancillaryAwards = CompactSerializer.ReadListSerializable<AncillaryAward>(stream);
                foreach(var ancillaryAward in ancillaryAwards)
                {
                    Awards.Items.Add(ancillaryAward);
                }
            }
            else if(awardType == typeof(RiskAward).Name)
            {
                var riskAwards = CompactSerializer.ReadListSerializable<RiskAward>(stream);
                foreach(var riskAward in riskAwards)
                {
                    Awards.Items.Add(riskAward);
                }
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Retrieve the type name string to indicate the awards type that this outcome list feature entry contains.
        /// </summary>
        /// <returns>The type name string to indicate the awards type, empty string for an empty award list.</returns>
        private string GetAwardType()
        {
            return Awards.Items.Any() ? Awards.Items[0].GetType().Name : string.Empty;
        }

        #endregion
    }
}
