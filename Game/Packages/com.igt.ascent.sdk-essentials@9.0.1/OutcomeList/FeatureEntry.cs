//-----------------------------------------------------------------------
// <copyright file = "FeatureEntry.cs" company = "IGT">
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
    /// Contains outcome/awards created by a theme's feature.
    /// </summary>
    [Serializable]
    public class FeatureEntry : IFeatureEntry, ICompactSerializable
    {
        private List<int> featureRngNumbers = new List<int>();
        private List<Award> awards = new List<Award>();

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
        public FeatureEntry()
        {
        }

        /// <summary>
        /// Constructor.  Converts an IFeatureEntry into a FeatureEntry.
        /// </summary>
        /// <param name="featureEntry">An implementation of <see cref="IFeatureEntry"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureEntry"/> is null.
        /// </exception>
        public FeatureEntry(IFeatureEntry featureEntry)
        {
            if(featureEntry == null)
            {
                throw new ArgumentNullException(nameof(featureEntry));
            }

            if(featureEntry.ContainsAncillaryAwardOutcome)
            {
                awards = featureEntry.GetAwards<IAncillaryAward>().Select(
                    entry => new AncillaryAward(entry) as Award).ToList();

            }
            else if(featureEntry.ContainsFeatureAwardOutcome)
            {
                awards = featureEntry.GetAwards<IFeatureAward>().Select(
                    entry => new FeatureAward(entry) as Award).ToList();
            }
            else if(featureEntry.ContainsRiskAwardOutcome)
            {
                awards = featureEntry.GetAwards<IRiskAward>().Select(
                    entry => new RiskAward(entry) as Award).ToList();
            }

            var featureNumbers = featureEntry.GetFeatureRngNumbers();

            if(featureNumbers?.Any() == true)
            {
                featureRngNumbers = featureNumbers.ToList();
            }

            FeatureIndex = featureEntry.FeatureIndex;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="FeatureEntry"/> using passed in arguments.
        /// </summary>
        /// <param name="featureIndex">The associated index for this feature.</param>
        /// <param name="featureRngNumbers">Optional. The RNG numbers.</param>
        /// <param name="awards">Optional. The <see cref="Award"/>s.</param>
        public FeatureEntry(uint featureIndex = 0, IEnumerable<int> featureRngNumbers = null, IEnumerable<Award> awards = null)
        {
            FeatureIndex = featureIndex;

            if(featureRngNumbers != null)
            {
                this.featureRngNumbers = featureRngNumbers.ToList();
            }

            var awardsList = awards?.ToList();
            if(awardsList?.Any() == true)
            {
                var initialType = awardsList[0].GetType().Name;
                this.awards =
                    awardsList.Select(award =>
                    {
                        var awardType = award.GetType().Name;
                        if(!SupportedAwardTypes.Contains(awardType) || awardType != initialType)
                        {
                            var message =
                                $"The award type {awardType} is not supported or not the same type as the others ({initialType}).";
                            throw new ArgumentException(message, nameof(awards));
                        }

                        return award;
                    }).ToList();
            }
        }

        #endregion Constructors

        /// <summary>
        /// Adds an <see cref="AncillaryAward"/>.
        /// </summary>
        /// <param name="award">The <see cref="AncillaryAward"/> to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="award"/> is null.</exception>
        /// <exception cref="ArgumentException"> Thrown if the type of <paramref name="award"/> is not supported in a feature entry.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Feature/Ancillary Award is added to a list that contains awards of different type.</exception>
        public void AddAward(AncillaryAward award)
        {
            AddAward(award, ContainsAncillaryAwardOutcome);
        }

        /// <summary>
        /// Adds an <see cref="AncillaryAward"/>.
        /// </summary>
        /// <param name="award">The <see cref="AncillaryAward"/> to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="award"/> is null.</exception>
        /// <exception cref="ArgumentException"> Thrown if the type of <paramref name="award"/> is not supported in a feature entry.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Feature/Ancillary Award is added to a list that contains awards of different type.</exception>
        public void AddAward(FeatureAward award)
        {
            AddAward(award, ContainsFeatureAwardOutcome);
        }

        /// <summary>
        /// Adds an <see cref="AncillaryAward"/>.
        /// </summary>
        /// <param name="award">The <see cref="AncillaryAward"/> to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="award"/> is null.</exception>
        /// <exception cref="ArgumentException"> Thrown if the type of <paramref name="award"/> is not supported in a feature entry.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a Feature/Ancillary Award is added to a list that contains awards of different type.</exception>
        public void AddAward(RiskAward award)
        {
            AddAward(award, ContainsRiskAwardOutcome);
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
        /// Adds a number to the FeatureRngNumbers list.
        /// </summary>
        /// <param name="number">The integer to add to the list.</param>
        public void AddFeatureRngNumber(int number)
        {
            featureRngNumbers.Add(number);
        }

        /// <summary>
        /// Adds numbers to the FeatureRngNumbers list.
        /// </summary>
        /// <param name="numbers">The integers to add to the list.</param>
        public void AddFeatureRngNumbers(IEnumerable<int> numbers)
        {
            featureRngNumbers.AddRange(numbers);
        }

        #region IFeatureEntry Members

        /// <inheritdoc />
        public uint FeatureIndex { get; private set; }

        /// <inheritdoc />
        public bool ContainsFeatureAwardOutcome
        {
            get
            {
                var result = false;

                if(awards.Any())
                {
                    // Check if the first award is an FeatureAward.
                    // The awards in the list are supposed to be of the same type.
                    result = awards[0] is FeatureAward;
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

                if(awards.Any())
                {
                    // Check if the first award is an AncillaryAward.
                    // The awards in the list are supposed to be of the same type.
                    result = awards[0] is AncillaryAward;
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

                if(awards.Any())
                {
                    // Check if the first award is a RiskAward.
                    // The awards in the list are supposed to be of the same type.
                    result = awards[0] is RiskAward;
                }

                return result;
            }
        }

        /// <inheritdoc />
        public long GetTotalDisplayableAmount()
        {
            return awards.Sum(award => award.GetDisplayableAmount());
        }

        /// <inheritdoc />
        public ReadOnlyCollection<int> GetFeatureRngNumbers()
        {
            return featureRngNumbers.AsReadOnly();
        }

        /// <inheritdoc />
        public ReadOnlyCollection<T> GetAwards<T>()
        {
            return awards.OfType<T>().ToList().AsReadOnly();
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
            CompactSerializer.WriteList(stream, featureRngNumbers);

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
                CompactSerializer.WriteList(stream, awards);
            }
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            FeatureIndex = CompactSerializer.ReadUint(stream);
            featureRngNumbers = CompactSerializer.ReadListInt(stream);

            // Read the award type name.
            var awardType = CompactSerializer.ReadString(stream);

            // Read the award list accordingly, for award type not supported, leave it empty.
            if(awardType == typeof(FeatureAward).Name)
            {
                awards = CompactSerializer.ReadListSerializable<FeatureAward>(stream).Cast<Award>().ToList();
            }
            else if(awardType == typeof(AncillaryAward).Name)
            {
                awards = CompactSerializer.ReadListSerializable<AncillaryAward>(stream).Cast<Award>().ToList();
            }
            else if(awardType == typeof(RiskAward).Name)
            {
                awards = CompactSerializer.ReadListSerializable<RiskAward>(stream).Cast<Award>().ToList();
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
            if(featureRngNumbers != null)
            {
                builder.Append("\t  " + featureRngNumbers);
            }

            var awardType = GetAwardType();

            builder.AppendFormat("\t  < {0} {1} >{2}", awards.Count, awardType, Environment.NewLine);
            for(var i = 0; i < awards.Count; i++)
            {
                builder.AppendFormat("\t  #{0}{1}", i + 1, Environment.NewLine);
                builder.AppendFormat("\t  {0}", awards[i]);
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
            return awards.Any() ? awards[0].GetType().Name : string.Empty;
        }

        /// <summary>
        /// Adds an Award to the Feature Entry.
        /// </summary>
        /// <param name="award">The Award to be added.</param>
        /// <param name="containsFlag">
        /// The flag indicating if the award type (Ancillary, Feature, Risk) is contained within this FeatureEntry.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="award"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the type of <paramref name="award"/> is not supported in a feature entry.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a Feature/Ancillary Award is added to a list that contains
        /// awards of different type.
        /// </exception>
        private void AddAward(Award award, bool containsFlag)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            if(!SupportedAwardTypes.Contains(award.GetType().Name))
            {
                var message = $"The award type {award.GetType().Name} is not supported.";
                throw new ArgumentException(message, nameof(award));
            }

            // If this FeatureEntry doesn't contain any awards, the containsFlag is false.
            // containsFlag is the bool output of ContainsXXXAwardOutcome
            if(!awards.Any() || containsFlag)
            {
                awards.Add(award);
            }
            else if(awards.Any())
            {
                throw new InvalidOperationException(
                    $"{award.GetType().Name} cannot be added. The award list already contains {GetAwardType()}.");
            }
        }

        #endregion
    }
}
