//-----------------------------------------------------------------------
// <copyright file = "CellPopulation.Extensions.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.IO;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated CellPopulation class.
    /// </summary>
    public partial class CellPopulation : ICompactSerializable, IEquatable<CellPopulation>
    {
        #region Constructors

        /// <summary>
        /// Create a default cell population.
        /// </summary>
        public CellPopulation()
        {
        }

        /// <summary>
        /// Construct a cell population containing a copy of the passed population.
        /// </summary>
        /// <param name="populationToCopy">The population to copy.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="populationToCopy"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the population does not contain at least 1 population entry or if the name of the population
        /// is <see langword="null"/>.
        /// </exception>
        public CellPopulation(CellPopulation populationToCopy)
        {
            if(populationToCopy == null)
            {
                throw new ArgumentNullException(nameof(populationToCopy));
            }

            if(populationToCopy.PopulationEntry?.Any() != true)
            {
                throw new ArgumentException("CellPopulations must contain at least 1 population entry.", nameof(populationToCopy));
            }

            name = populationToCopy.name ?? throw new ArgumentException("A CellPopulation cannot have a null name.", nameof(populationToCopy));

            foreach (var populationEntry in populationToCopy.PopulationEntry)
            {
                var newEntry = new CellPopulationPopulationEntry(populationEntry);
                PopulationEntry.Add(newEntry);
            }
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var equals = false;

            if(obj != null)
            {
                if(obj is CellPopulation cellPopulationObj)
                {
                    equals = Equals(cellPopulationObj);
                }
            }

            return equals;
        }

        /// <summary>
        /// Determines if this instance is equal to another.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        /// <returns>True if both instances are equal.</returns>
        public bool Equals(CellPopulation other)
        {
            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    return true;
                }

                var equals = nameField == other.nameField;

                if(populationEntryField != null && other.populationEntryField != null)
                {
                    equals = equals && populationEntryField.SequenceEqual(other.populationEntryField);
                }
                else if(!(populationEntryField == null && other.populationEntryField == null))
                {
                    equals = false;
                }

                return equals;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = 0;
            if(nameField != null)
            {
                hashCode = nameField.GetHashCode();
            }

            if(populationEntryField != null)
            {
                hashCode ^= populationEntryField.GetHashCode();
            }

            return hashCode;
        }

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, populationEntryField);
            CompactSerializer.Write(stream, nameField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            populationEntryField = CompactSerializer.ReadListSerializable<CellPopulationPopulationEntry>(stream);
            nameField = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
