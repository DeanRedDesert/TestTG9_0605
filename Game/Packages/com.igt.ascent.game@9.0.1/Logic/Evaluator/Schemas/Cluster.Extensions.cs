//-----------------------------------------------------------------------
// <copyright file = "Cluster.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated Cluster class.
    /// </summary>
    public partial class Cluster : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// A regular expression for parsing <see cref="Cluster"/> objects from a string.
        /// </summary>
        /// <remarks>
        /// A group of clusters is formatted as [Cluster1][Cluster2]...[ClusterN].  Each cluster is a list of cells.
        /// </remarks>
        public const string ClusterPattern = @"\[([^\[^\]]*)\]";

        /// <summary>
        /// The format for a cluster.
        /// </summary>
        private const string ClusterFormat = "[{0}]";

        /// <summary>
        /// The separator for cells in a cluster.
        /// </summary>
        private const string CellSeparator = " ";

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Cluster()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class from the given string.
        /// </summary>
        /// <param name="formattedCluster">A string containing a formatted cluster.</param>
        public Cluster(string formattedCluster) : this(Regex.Match(formattedCluster, ClusterPattern))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class using the given
        /// regex <see cref="Match"/> object.
        /// </summary>
        /// <param name="clusterMatch">A <see cref="Match"/> for the cluster pattern <see cref="ClusterPattern"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <see cref="Group.Success"/> is false.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="clusterMatch"/> is null.
        /// </exception>
        internal Cluster(Match clusterMatch)
        {
            if(ReferenceEquals(clusterMatch, null))
            {
                throw new ArgumentNullException("clusterMatch");
            }
            if(!clusterMatch.Success)
            {
                throw new ArgumentException("Clusters must be formatted properly: \"[Cell1 Cell2 .. CellN]");
            }
            var cellRegex = new Regex(Cell.CellPattern, RegexOptions.Compiled);
            Cells.AddRange(GetCells(clusterMatch.Groups[1].Value, cellRegex));
        }

        /// <summary>
        /// Formats this cluster as a string.
        /// </summary>
        /// <returns>A string containing the formatted cluster.</returns>
        public override string ToString()
        {
            var formattedCells =
                Cells.Select(cell => cell.ToString());
            return string.Format(
                CultureInfo.InvariantCulture, ClusterFormat, string.Join(CellSeparator, formattedCells.ToArray()));
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.WriteList(stream, Cells);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            Cells = CompactSerializer.ReadListSerializable<Cell>(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new Cluster
                       {
                           name = name,
                           Cells = NullableClone.DeepCloneList(Cells)
                       };
            return copy;
        }

        #endregion

        /// <summary>
        /// Gets a collection of cells from the formatted string.
        /// </summary>
        /// <param name="formattedCells">A string containing one or more formatted cells.</param>
        /// <param name="cellRegex">A <see cref="Regex"/> that recognizes cells.</param>
        /// <returns>The collection of cells represented by the string.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the string does not contain properly formatted cells or if any of the cells
        /// do not have at least 2 indices specified.
        /// </exception>
        private IEnumerable<Cell> GetCells(string formattedCells, Regex cellRegex)
        {
            if(string.IsNullOrEmpty(formattedCells))
            {
                throw new ArgumentException("Argument cannot be null or empty.", "formattedCells");
            }
            var cellMatches = cellRegex.Matches(formattedCells);
            if(cellMatches.Count == 0)
            {
                throw new ArgumentException("Cells must be formatted properly: \"(L<layer>R<row>C<column>)\".", "formattedCells");
            }
            return cellMatches.Cast<Match>().Select(cellMatch => new Cell(cellMatch)).ToList();
        }

    }
}
