using System;
using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A Cluster is a group of cells.
	/// </summary>
	public sealed class Cluster : IToCode
	{
		/// <summary>
		/// The list of cells for this cluster.
		/// </summary>
		public IReadOnlyList<Cell> Cells { get; }

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		public Cluster(IReadOnlyList<Cell> cells)
		{
			Cells = cells;
		}

		public static IReadOnlyList<Cluster> CreateList(string str)
		{
			if (StringConverter.TryFromString(str, "SL", typeof(IReadOnlyList<string>), out var clusters))
			{
				var clusterList = new List<Cluster>();

				foreach (var s in (IReadOnlyList<string>)clusters)
				{
					if (StringConverter.TryFromString(s, "SL", typeof(IReadOnlyList<Cell>), out var c))
						clusterList.Add(new Cluster((IReadOnlyList<Cell>)c));
					else
						throw new Exception($"Bad Cluster: {s}");
				}

				return clusterList;
			}

			throw new Exception($"Cannot convert to list of clusters: {str}");
		}

		#region IToCode Members

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args) => new NotSupported();

		/// <summary>Implementation of IToCode.ListToCode(CodeGenArgs, object)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToCode(CodeGenArgs args, object list)
		{
			var clusters = new List<string>();

			foreach (var cluster in (IReadOnlyList<Cluster>)list)
			{
				if (!StringConverter.TryToString(cluster.Cells, "SL", out var cs))
					return new Error(cs);

				clusters.Add(cs);
			}

			return StringConverter.TryToString(clusters, "SL", out var s)
				? $"{CodeConverter.ToCode<Cluster>(args)}.{nameof(CreateList)}(\"{s}\")".ToSuccess()
				: new Error(s);
		}

		#endregion
	}
}