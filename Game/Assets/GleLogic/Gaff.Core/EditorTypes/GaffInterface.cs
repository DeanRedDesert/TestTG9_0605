using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core.DecisionMakers;

namespace Gaff.Core.EditorTypes
{
	/// <summary>
	/// A definition of an interface for editing objects derived from <see cref="DecisionMaker"/>, <see cref="ResultCondition"/> and <see cref="StepCondition"/>.
	/// The general structure is of a collection of rows.  With each row containing a left aligned sequence of items placed one after the other.
	/// </summary>
	public sealed class GaffInterface
	{
		/// <summary>
		/// The rows of interface items to display.
		/// </summary>
		public IReadOnlyList<GaffEditorRow> Rows { get; }

		public GaffInterface(IReadOnlyList<GaffEditorRow> rows)
		{
			Rows = rows;
		}
	}

	/// <summary>
	/// The row data in a <see cref="GaffInterface"/>.
	/// </summary>
	public sealed class GaffEditorRow
	{
		/// <summary>
		/// The margin (in pixels) reserved above this row.
		/// </summary>
		public int TopGap { get; }

		/// <summary>
		/// The margin (in pixels) reserved below this row.
		/// </summary>
		public int BottomGap { get; }

		/// <summary>
		/// The interface items to be displayed in this row.
		/// </summary>
		public IReadOnlyList<GaffEditorItem> Items { get; }

		public GaffEditorRow(IReadOnlyList<GaffEditorItem> items, int topGap = 0, int bottomGap = 0)
		{
			Items = items;
			TopGap = topGap;
			BottomGap = bottomGap;
		}
	}

	/// <summary>
	/// An item in the <see cref="GaffInterface"/>.  These are read only text areas, editable text and number fields, check boxes etc.
	/// </summary>
	public abstract class GaffEditorItem
	{
		/// <summary>
		/// The margin (in pixels) reserved to the left of this item.
		/// </summary>
		public int LeftGap { get; }

		/// <summary>
		/// The margin (in pixels) reserved to the right of this item.
		/// </summary>
		public int RightGap { get; }

		protected GaffEditorItem(int leftGap, int rightGap)
		{
			LeftGap = leftGap;
			RightGap = rightGap;
		}
	}
}