//-----------------------------------------------------------------------
// <copyright file = "PatternListPattern.Extensions.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extensions to the generated <see cref="PatternListPattern"/> class.
    /// </summary>
    public partial class PatternListPattern
    {
        /// <summary>
        /// The format for a name.
        /// </summary>
        private const string NameFormat = "{0}";

        /// <summary>
        /// The separator which is placed between the pattern name and the clusters.
        /// </summary>
        private const string NameClusterSeparator = ":";
        
        /// <summary>
        /// Creates a new uninitialized <see cref="PatternListPattern"/>.
        /// </summary>
        public PatternListPattern()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PatternListPattern"/> class with the given formatted
        /// pattern string.
        /// </summary>
        /// <param name="formattedPattern">A string conforms to the format for patterns defined by this class.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the formatted pattern does not contain a string of the format <![CDATA["<Name>:<Clusters>"]]> or
        /// if it is null or empty.
        /// </exception>
        /// <example>
        /// For a 3 x 3 symbol window, Line 1, which goes across the middle row of the window would be:
        /// 
        /// "Line 1:[(L0R1C0)][(L0R1C1)][(L0R1C2)]" or optionally "Line 1:[(R1C0)][(R1C1)][(R1C2)]" if the cells are
        /// on layer 0.
        /// 
        /// For the same symbol window, a scatter pattern containing cells grouped by column, would be:
        /// 
        /// "Scatter:[(L0R0C0)(L0R1C0)(L0R2C0)][(L0R0C1)(L0R1C1)(L0R2C1)][(L0R0C2)(L0R1C2)(L0R2C2)]" or
        /// "Scatter:[(R0C0)(R1C0)(R2C0)][(R0C1)(R1C1)(R2C1)][(R0C2)(R1C2)(R2C2)]" for layer 0.
        /// </example>
        public PatternListPattern(string formattedPattern)
        {
            if(string.IsNullOrEmpty(formattedPattern))
            {
                throw new ArgumentException("Argument cannot be null or empty.", nameof(formattedPattern));
            }
            var patternParts = formattedPattern.Split(':');
            if(patternParts.Length != 2)
            {
                throw new ArgumentException("Pattern must be of format <Name>:<Clusters>.");
            }
            name = patternParts[0];

            var clusterRegex = new Regex(Schemas.Cluster.ClusterPattern, RegexOptions.Compiled);
            Cluster.AddRange(GetClusters(patternParts[1], clusterRegex));
        }

        /// <summary>
        /// Gets a collection of clusters from the formatted string.
        /// </summary>
        /// <param name="formattedClusters">A string containing one or more formatted clusters.</param>
        /// <param name="clusterRegex">A <see cref="Regex"/> that recognizes clusters.</param>
        /// <returns>The collection of clusters represented by the formatted string.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="formattedClusters"/> does not contain any properly formatted clusters.
        /// </exception>
        private IEnumerable<Cluster> GetClusters(string formattedClusters, Regex clusterRegex)
        {
            if(!clusterRegex.IsMatch(formattedClusters))
            {
                throw new ArgumentException("No clusters were found.");
            }
            // TODO: http://cspjira.gtk.gtech.com:8080/jira/browse/AS-8712
            // ReSharper disable once CollectionNeverUpdated.Local
            var clusters = new List<Cluster>();
            var clusterMatches = clusterRegex.Matches(formattedClusters);
            foreach(var clusterMatch in clusterMatches.Cast<Match>())
            {
                Cluster.Add(new Cluster(clusterMatch));
            }
            return clusters;
        }

        /// <summary>
        /// Creates a string representation of this pattern.
        /// </summary>
        /// <returns>A string that represents the pattern.</returns>
        /// <remarks>
        /// This implementation represents the "official" way to format patterns, and should not be changed
        /// without updating the <see cref="PatternListPattern(string)"/> method and any tools that are
        /// relying on this implementation (such as the Game Data Editor.)
        /// </remarks>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, NameFormat, name);
            stringBuilder.Append(NameClusterSeparator);
            foreach(var cluster in Cluster)
            {
                stringBuilder.Append(cluster);
            }
            return stringBuilder.ToString();
        }
    }
}
