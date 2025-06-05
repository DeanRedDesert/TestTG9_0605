using System.Collections.Generic;

namespace Logic.Core.Types
{
	/// <summary>
	/// A pattern is a abstract representation what older platforms call a PayLine.
	/// </summary>
	public sealed class Pattern
	{
		/// <summary>
		/// The name of the pattern.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The clusters that make up the pattern.
		/// </summary>
		public IReadOnlyList<Cluster> Clusters { get; }

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		/// <param name="name">The name of the pattern.</param>
		/// <param name="clusters">The clusters that make up the pattern.</param>
		public Pattern(string name, IReadOnlyList<Cluster> clusters)
		{
			Name = name;
			Clusters = clusters;
		}
	}
}