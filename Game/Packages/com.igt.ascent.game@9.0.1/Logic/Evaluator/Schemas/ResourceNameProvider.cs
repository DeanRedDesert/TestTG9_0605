//-----------------------------------------------------------------------
// <copyright file = "ResourceNameProvider.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A provider that retrieves all of the matching resource names from the given <see cref="Assembly"/>.
    /// </summary>
    public static class ResourceNameProvider
    {
        /// <summary>
        /// Gets all of the resource names in the given assembly that match the given regular expression pattern.
        /// </summary>
        /// <param name="resourceAssembly">The <see cref="Assembly"/> that contains the embedded resources.</param>
        /// <param name="resourceNamePattern">A valid regular expression describing the resource names.</param>
        /// <returns>
        /// An enumerable collection of all the resource names in the assembly that match the regular expression pattern.
        /// </returns>
        public static IEnumerable<string> GetMatchingResourceNames(this Assembly resourceAssembly, string resourceNamePattern)
        {
            var resourceNameRegex = new Regex(resourceNamePattern, RegexOptions.Compiled);
            return resourceAssembly.GetManifestResourceNames().Where(name => resourceNameRegex.IsMatch(name));
        }
    }
}
