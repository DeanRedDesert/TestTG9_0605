//-----------------------------------------------------------------------
// <copyright file = "Cell.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated Cell class.
    /// </summary>
    public partial class Cell : ICompactSerializable, IEquatable<Cell>, IDeepCloneable
    {
        /// <summary>
        /// The format for a cell.
        /// </summary>
        private const string CellFormat = "(L{0}R{1}C{2})";

        /// <summary>
        /// A regular expression for parsing <see cref="Cell"/> objects from a string.
        /// </summary>
        /// <remarks>
        /// A group of cells is formatted as (Cell1)(Cell2)...(CellN).  Each cell is formatted as LxRyCz, where
        /// x, y and z are natural numbers.
        /// </remarks>
        public const string CellPattern = @"\(L?(?<layer>\d*)R(?<row>\d+)C(?<column>\d+)\)";

        /// <summary>
        /// The default value to use if a cell does not specify a third coordinate, which is 0.
        /// </summary>
        private const int DefaultLayerValue = 0;

        /// <summary>
        /// The string used to index <see cref="Match.Groups"/> to access the row coordinate.
        /// </summary>
        private const string RowGroup = "row";

        /// <summary>
        /// The string used to index <see cref="Match.Groups"/> to access the column coordinate.
        /// </summary>
        private const string ColumnGroup = "column";

        /// <summary>
        /// The string used to index <see cref="Match.Groups"/> to access the layer coordinate.
        /// </summary>
        private const string LayerGroup = "layer";

        /// <summary>
        /// Construct a cell with the given parameters.
        /// </summary>
        /// <param name="layer">Layer index of the cell.</param>
        /// <param name="row">Row index of the cell.</param>
        /// <param name="column">Column index of the cell.</param>
        public Cell(uint layer, uint row, uint column)
        {
            this.layer = layer;
            this.row = row;
            this.column = column;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class by parsing the given string.
        /// </summary>
        /// <param name="formattedCell">A string containing a formatted cell.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="formattedCell"/> is empty.
        /// </exception>
        public Cell(string formattedCell) : this(Regex.Match(formattedCell, CellPattern))
        {
            if(string.IsNullOrEmpty(formattedCell))
            {
                throw new ArgumentException("String cannot be empty.", "formattedCell");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class with the data contained in the given
        /// regular expression match.
        /// </summary>
        /// <param name="cellMatch">A <see cref="Match"/> for the cell pattern <see cref="CellPattern"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the match has less than 4 groups or if <see cref="Group.Success"/> is false.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="cellMatch"/> is null.
        /// </exception>
        internal Cell(Match cellMatch)
        {
            if(ReferenceEquals(cellMatch, null))
            {
                throw new ArgumentNullException("cellMatch");
            }
            if(!cellMatch.Success)
            {
                throw new ArgumentException("Cells must be formatted properly: \"(L<layer>R<row>C<column>)\".");
            }
            if(cellMatch.Groups.Count < 4)
            {
                throw new ArgumentException("Match must contain 4 groups.");
            }
            row = uint.Parse(cellMatch.Groups[RowGroup].Value);
            column = uint.Parse(cellMatch.Groups[ColumnGroup].Value);
            layer = !string.IsNullOrEmpty(cellMatch.Groups[LayerGroup].Value) ? uint.Parse(cellMatch.Groups[LayerGroup].Value) : DefaultLayerValue;
        }

        /// <summary>
        /// Parameter-less constructor.
        /// </summary>
        public Cell()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copy">The instance to copy.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="copy"/> is null.
        /// </exception>
        public Cell(Cell copy)
        {
            if(copy == null)
            {
                throw new ArgumentNullException("copy");
            }

            row = copy.row;
            column = copy.column;
            layer = copy.layer;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Cell);
        }

        /// <inheritdoc />
        public bool Equals(Cell other)
        {
            if(other == null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || (other.layer == layer && other.row == row && other.column == column);
        }

        /// <summary>
        /// Override of the hash code operator to follow equality rules.
        /// </summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            var layerShift = layer << 20;
            var rowShift = row << 10;
            return (int)(layerShift + rowShift + column);
        }

        /// <summary>
        /// Write the content of the cell to a string.
        /// </summary>
        /// <returns>A string representation of the cell.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, CellFormat, layer, row, column);
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, layer);
            CompactSerializer.Write(stream, column);
            CompactSerializer.Write(stream, row);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            layer = CompactSerializer.ReadUint(stream);
            column = CompactSerializer.ReadUint(stream);
            row = CompactSerializer.ReadUint(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            return new Cell(this);
        }

        #endregion
    }
}
