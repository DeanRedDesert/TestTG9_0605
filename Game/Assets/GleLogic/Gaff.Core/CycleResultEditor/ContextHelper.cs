using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Gaff.Core.CycleResultEditor
{
	public static class ContextHelper
	{
		private static readonly Dictionary<string, IReadOnlyList<string>> parsedContexts = new Dictionary<string, IReadOnlyList<string>>();

		/// <summary>
		/// Parse a full structure from a set of decision context strings.
		/// </summary>
		/// <param name="contexts">A collection of context strings.</param>
		/// <param name="getCallIndex">A function to get the call index.</param>
		/// <returns>A complete context structure created from the context strings.</returns>
		public static IReadOnlyList<ContextStructure> GenerateContextStructure(this IReadOnlyList<string> contexts, Func<string, int> getCallIndex)
		{
			return GenerateContextStructureInternal(contexts, string.Empty, 0, getCallIndex);
		}

		/// <summary>
		/// Process a context structure and groups that contain a single child element and collapse them into a single entry.
		/// </summary>
		/// <param name="contextStructure">The structure to condense.</param>
		/// <returns>A condensed context structure.</returns>
		/// <exception cref="Exception"></exception>
		public static ContextStructure Condense(this ContextStructure contextStructure)
		{
			switch (contextStructure)
			{
				case GroupItem groupItem:
				{
					var workingGroup = groupItem;

					while (workingGroup.Children.Count == 1 && workingGroup.Children[0] is GroupItem innerGroup)
						workingGroup = new GroupItem($"{workingGroup.Title}_{innerGroup.Title}", innerGroup.Children);

					if (workingGroup.Children.Count == 1 && workingGroup.Children[0] is LeafItem leafItem)
						return new LeafItem($"{workingGroup.Title}_{leafItem.Title}", leafItem.Context, leafItem.CallIndex);

					return new GroupItem(workingGroup.Title, workingGroup.Children.Select(Condense).ToList());
				}
				case LeafItem leafItem:
					return leafItem;
				default: throw new Exception();
			}
		}

		private static IReadOnlyList<ContextStructure> GenerateContextStructureInternal(this IReadOnlyList<string> contexts, string current, int index, Func<string, int> getCallIndex)
		{
			var result = new List<ContextStructure>();
			var data = GenerateContextStructureAtIndex(contexts, index);

			foreach (var tuple in data)
			{
				if (tuple.Contexts.Count == 1)
				{
					if (tuple.Contexts[0] == current || current == string.Empty)
						result.Add(new LeafItem(tuple.Contexts[0], tuple.Contexts[0], getCallIndex(tuple.Contexts[0])));
					else
						result.Add(new LeafItem(tuple.Contexts[0].Replace($"{current}_", ""), tuple.Contexts[0], getCallIndex(tuple.Contexts[0])));
				}
				else
				{
					var newItems = new List<ContextStructure>();
					var rootLevelContext = current == string.Empty ? tuple.SubContext : $"{current}_{tuple.SubContext}";

					if (tuple.Contexts.Contains(rootLevelContext))
						newItems.Add(new LeafItem(string.Empty, rootLevelContext, getCallIndex(rootLevelContext)));

					newItems.AddRange(GenerateContextStructureInternal(tuple.Contexts.Except(new[] { rootLevelContext }).ToList(), rootLevelContext, index + 1, getCallIndex));

					result.Add(new GroupItem(tuple.SubContext, newItems));
				}
			}

			return result;
		}

		private static IReadOnlyList<string> GetSubContexts(this string context)
		{
			if (!parsedContexts.TryGetValue(context, out var result))
			{
				var parsedContext = context.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

				parsedContexts.Add(context, parsedContext);
				result = parsedContext;
			}

			return result;
		}

		private static IReadOnlyList<(string SubContext, List<string> Contexts)> GenerateContextStructureAtIndex(IReadOnlyList<string> contexts, int index)
		{
			var data = new List<(string SubContext, List<string> Contexts)>();

			foreach (var context in contexts)
			{
				var subContext = context.GetSubContexts();

				if (index >= subContext.Count)
					continue;

				if (data.All(d => d.SubContext != subContext[index]))
					data.Add((subContext[index], new List<string> { context }));
				else
				{
					var localScopeIndex = data.IndexOf(d => d.SubContext == subContext[index]);
					data[localScopeIndex].Contexts.Add(context);
				}
			}

			return data;
		}
	}
}