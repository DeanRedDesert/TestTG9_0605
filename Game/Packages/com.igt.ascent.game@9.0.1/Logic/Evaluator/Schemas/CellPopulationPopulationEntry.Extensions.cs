//-----------------------------------------------------------------------
// <copyright file = "CellPopulationPopulationEntry.Extensions.cs" company = "IGT">
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
    /// Extensions to the generated CellPopulationPopulationEntry class.
    /// </summary>
    public partial class CellPopulationPopulationEntry : ICompactSerializable, IEquatable<CellPopulationPopulationEntry>
    {
        #region Constructors

        /// <summary>
        /// Create a default population entry.
        /// </summary>
        public CellPopulationPopulationEntry()
        {
        }

        /// <summary>
        /// Construct a population entry based on the passed entry.
        /// </summary>
        /// <param name="entryToCopy">Entry to copy.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="entryToCopy"/> does not have at least 1 cell, its name is <see langword="null"/>,
        /// or its stripName is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="entryToCopy"/> is null.</exception>
        public CellPopulationPopulationEntry(CellPopulationPopulationEntry entryToCopy)
        {
            if(entryToCopy == null)
            {
                throw new ArgumentNullException(nameof(entryToCopy));
            }

            if(entryToCopy.Cells?.Any() != true)
            {
                throw new ArgumentException("Population entries must contain at least 1 cell.", nameof(entryToCopy));
            }

            name = entryToCopy.name ?? throw new ArgumentException("A CellPopulationPopulationEntry must not have a null name.", nameof(entryToCopy));
            startPosition = entryToCopy.startPosition;
            stripName = entryToCopy.stripName ?? throw new ArgumentException("A CellPopulationPopulationEntry must not have a null stripName.",
                            nameof(entryToCopy));
            stripOffsetIndex = entryToCopy.stripOffsetIndex;

            foreach (var cell in entryToCopy.Cells)
            {
                Cells.Add(cell);
            }
        }

        #endregion

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = stripOffsetIndexField.GetHashCode()
                ^ startPositionField.GetHashCode();

            if(cellsField != null)
            {
                hashCode ^= cellsField.GetHashCode();
            }

            if(nameField != null)
            {
                hashCode ^= nameField.GetHashCode();
            }

            if(stripNameField != null)
            {
                hashCode ^= stripNameField.GetHashCode();
            }

            return hashCode;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var equals = false;

            if(obj != null)
            {
                if(obj is CellPopulationPopulationEntry cellPopulationEntryObj)
                {
                    equals = Equals(cellPopulationEntryObj);
                }
            }

            return equals;
        }

        /// <summary>
        /// Determines if this instance is equal to another.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        /// <returns>True if both instances are equal.</returns>
        public bool Equals(CellPopulationPopulationEntry other)
        {
            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    return true;
                }

                var equals = nameField == other.nameField
                             && stripOffsetIndexField == other.stripOffsetIndexField
                             && startPositionField == other.startPositionField
                             && stripNameField == other.stripNameField;

                if(cellsField != null && other.cellsField != null)
                {
                    equals = equals && cellsField.SequenceEqual(other.cellsField);
                }
                else if(!(cellsField == null && other.cellsField == null))
                {
                    equals = false;
                }

                return equals;
            }

            return false;
        }

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, cellsField);
            CompactSerializer.Write(stream, nameField);
            CompactSerializer.Write(stream, stripOffsetIndexField);
            CompactSerializer.Write(stream, startPositionField);
            CompactSerializer.Write(stream, stripNameField);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            cellsField = CompactSerializer.ReadListSerializable<Cell>(stream);
            nameField = CompactSerializer.ReadString(stream);
            stripOffsetIndexField = CompactSerializer.ReadInt(stream);
            startPositionField = CompactSerializer.ReadUint(stream);
            stripNameField = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
