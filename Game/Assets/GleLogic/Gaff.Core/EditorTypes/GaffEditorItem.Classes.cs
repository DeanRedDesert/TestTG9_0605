using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Gaff.Core.EditorTypes
{
	/// <summary>
	/// A readonly text block.
	/// </summary>
	public sealed class GeText : GaffEditorItem
	{
		/// <summary>
		/// The text to display.
		/// </summary>
		public string Text { get; }

		public GeText(string text, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap) => Text = text;
	}

	/// <summary>
	/// An editable text block.
	/// </summary>
	public sealed class GeTextEdit : GaffEditorItem
	{
		/// <summary>
		/// The editable text.
		/// </summary>
		public string Text { get; }

		public GeTextEdit(string text, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap) => Text = text;
	}

	/// <summary>
	/// An editable number.
	/// </summary>
	public sealed class GeNumber : GaffEditorItem
	{
		/// <summary>
		/// The editable number.
		/// </summary>
		public int Value { get; }

		public GeNumber(int value, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap) => Value = value;
	}

	/// <summary>
	/// A combo of selectable string values.  Single selection only.
	/// </summary>
	public sealed class GeCombo : GaffEditorItem
	{
		/// <summary>
		/// The collection of strings tpo select from.
		/// </summary>
		public IReadOnlyList<string> Items { get; }

		/// <summary>
		/// The index of the currently selected string item.
		/// </summary>
		public int SelectedIndex { get; }

		public GeCombo(IReadOnlyList<string> items, int selectedIndex, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap)
		{
			Items = items;
			SelectedIndex = selectedIndex;
		}
	}

	/// <summary>
	/// A combo of selectable string values.  Multiple selection allowed.
	/// </summary>
	public sealed class GeMultiCombo : GaffEditorItem
	{
		/// <summary>
		/// The collection of strings to select from.
		/// </summary>
		public IReadOnlyList<string> Items { get; }

		/// <summary>
		/// The indexes of the currently selected string items.
		/// </summary>
		public IReadOnlyList<int> SelectedIndexes { get; }

		public bool IsExpanded { get; }

		public GeMultiCombo(IReadOnlyList<string> items, IReadOnlyList<int> selectedIndexes, bool isExpanded, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap)
		{
			Items = items;
			SelectedIndexes = selectedIndexes;
			IsExpanded = isExpanded;
		}
	}

	/// <summary>
	/// A checkbox for boolean values.
	/// </summary>
	public sealed class GeCheckBox : GaffEditorItem
	{
		/// <summary>
		/// The checked state.
		/// </summary>
		public bool IsChecked { get; }

		public GeCheckBox(bool isChecked, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap) => IsChecked = isChecked;
	}

	/// <summary>
	/// A slider for selecting a number within in a range.
	/// </summary>
	public sealed class GeSlider : GaffEditorItem
	{
		/// <summary>
		/// The minimum selectable value.
		/// </summary>
		public int Min { get; }

		/// <summary>
		/// The maximum selectable value.
		/// </summary>
		public int Max { get; }

		/// <summary>
		/// The minimum selectable value.
		/// </summary>
		public int Current { get; }

		public GeSlider(int min, int max, int current, int leftGap = 0, int rightGap = 0) : base(leftGap, rightGap)
		{
			Min = min;
			Max = max;
			Current = current;
		}
	}
}