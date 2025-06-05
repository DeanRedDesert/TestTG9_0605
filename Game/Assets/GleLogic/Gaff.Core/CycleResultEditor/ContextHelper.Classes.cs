using System.Collections.Generic;

namespace Gaff.Core.CycleResultEditor
{
	/// <summary>
	/// Base class to a hierarchy of sub context items. For use in interfaces on the GLE and Unity sides.
	/// Only two kinds of node allowed LeafItem or GroupItem.
	/// Leaf items with an empty Title are decisions for the root context of the group they are in.
	/// </summary>
	public abstract class ContextStructure
	{
		public string Title { get; }

		protected ContextStructure(string title) => Title = title;
	}

	/// <summary>
	/// A leaf item is where an actual decisions is made.
	/// </summary>
	public sealed class LeafItem : ContextStructure
	{
		public string Context { get; }
		public int CallIndex { get; }

		public LeafItem(string title, string context, int callIndex)
			: base(title)
		{
			Context = context;
			CallIndex = callIndex;
		}
	}

	/// <summary>
	/// A group item gathers leaf items together at particular level based on parsed sub contexts.
	/// </summary>
	public sealed class GroupItem : ContextStructure
	{
		public IReadOnlyList<ContextStructure> Children { get; }

		public GroupItem(string title, IReadOnlyList<ContextStructure> children)
			: base(title)
		{
			Children = children;
		}
	}
}