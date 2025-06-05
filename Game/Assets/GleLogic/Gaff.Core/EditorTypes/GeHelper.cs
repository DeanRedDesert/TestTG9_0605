using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaff.Core.EditorTypes
{
	public static class GeHelper
	{
		/// <summary>
		/// Create an empty interface definition to be used with the <see cref="GaffInterface"/> fluent functions.
		/// </summary>
		/// <returns>An empty interface definition.</returns>
		public static GaffInterface CreateInterface() => new GaffInterface(Array.Empty<GaffEditorRow>());

		/// <summary>
		/// Create an empty row definition to be used with the <see cref="GaffInterface"/> fluent functions.
		/// </summary>
		/// <param name="topGap">The top margin in pixels.</param>
		/// <param name="bottomGap">The bottom margin in pixels.</param>
		/// <returns>An empty row definition.</returns>
		public static GaffEditorRow CreateRow(int topGap = 0, int bottomGap = 0) => new GaffEditorRow(Array.Empty<GaffEditorItem>(), topGap, bottomGap);

		/// <summary>
		/// Create a readonly text field and add it into the row.
		/// </summary>
		/// <param name="row">The row to add the Text item to.</param>
		/// <param name="text">The text to display.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		public static GaffEditorRow AddText(this GaffEditorRow row, string text, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeText(text, leftGap, rightGap));

		/// <summary>
		/// Create an editable text field and add it into the row.
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="text">The initial text to display and edit.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		public static GaffEditorRow AddTextEdit(this GaffEditorRow row, string text, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeTextEdit(text, leftGap, rightGap));

		/// <summary>
		/// Create a single selection combobox with a collection of strings and add it into the row.
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="items">The strings to select from.</param>
		/// <param name="selectedIndex">The index of the initial selected item.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		// ReSharper disable once UnusedMember.Global - Helper method
		public static GaffEditorRow AddComboEdit(this GaffEditorRow row, IReadOnlyList<string> items, int selectedIndex, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeCombo(items, selectedIndex, leftGap, rightGap));

		/// <summary>
		/// Create a multi selection combobox with a collection of strings and add it into the row.
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="items">The strings to select from.</param>
		/// <param name="selectedIndexes">The indexes of the initial selected items.</param>
		/// <param name="startExpanded">Start the combo expanded?</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		// ReSharper disable once UnusedMember.Global - Helper method
		public static GaffEditorRow AddMultiCombo(this GaffEditorRow row, IReadOnlyList<string> items, IReadOnlyList<int> selectedIndexes, bool startExpanded, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeMultiCombo(items, selectedIndexes, startExpanded, leftGap, rightGap));

		/// <summary>
		/// Create a check box and add it into the row.
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="isChecked">The initial checked state.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		public static GaffEditorRow AddCheckBox(this GaffEditorRow row, bool isChecked, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeCheckBox(isChecked, leftGap, rightGap));

		/// <summary>
		/// Create an editable number field and add it into the row.
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="number">The initial number to display and edit.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		public static GaffEditorRow AddNumberEdit(this GaffEditorRow row, int number, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeNumber(number, leftGap, rightGap));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="min">The minimum selectable number.</param>
		/// <param name="max">The maximum selectable number.</param>
		/// <param name="current">The initial selected number.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <returns>The updated row.</returns>
		// ReSharper disable once UnusedMember.Global - Helper method
		public static GaffEditorRow AddSlider(this GaffEditorRow row, int min, int max, int current, int leftGap = 0, int rightGap = 0) => row.AddItem(new GeSlider(min, max, current, leftGap, rightGap));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gi">The gaff interface.</param>
		/// <param name="row">The row index.</param>
		/// <param name="column">The column index.</param>
		/// <typeparam name="T">The specific type of <see cref="GaffEditorItem"/>.</typeparam>
		/// <returns>The item at the specified row and colum.</returns>
		/// <exception cref="Exception">If the items is not of type T.</exception>
		public static T GetItemAt<T>(this GaffInterface gi, int row, int column) where T : GaffEditorItem
		{
			if (!(gi.Rows[row].Items[column] is T item))
				throw new Exception($"Interface item at Row: {row} Col: {column} is not of type {typeof(T).Name}");
			return item;
		}

		/// <summary>
		/// Add an interface item to the row.
		/// </summary>
		/// <param name="row">The row to add the interface item to.</param>
		/// <param name="item">The item to add.</param>
		/// <typeparam name="T">The specific type of <see cref="GaffEditorItem"/>.</typeparam>
		/// <returns>The updated row.</returns>
		public static GaffEditorRow AddItem<T>(this GaffEditorRow row, T item) where T : GaffEditorItem
		{
			return new GaffEditorRow(new List<GaffEditorItem>(row.Items) { item }, row.TopGap, row.BottomGap);
		}

		/// <summary>
		/// Add an interface row to the <see cref="GaffInterface"/>
		/// </summary>
		/// <param name="gi">The gaff interface.</param>
		/// <param name="row">The <see cref="GaffEditorRow"/> to add.</param>
		/// <returns>An updated <see cref="GaffInterface"/>.</returns>
		public static GaffInterface AddRow(this GaffInterface gi, GaffEditorRow row)
		{
			return new GaffInterface(new List<GaffEditorRow>(gi.Rows) { row });
		}

		/// <summary>
		/// Take an enum value and create a <see cref="GeCombo"/> for it.  The list will be populated with all enum values and the selected index will be the specified value.
		/// </summary>
		/// <param name="selectedValue">The enum value to select after the combo is constructed.</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <typeparam name="T">The specific enum to use.</typeparam>
		/// <returns>A <see cref="GeCombo"/> populated with the range of enum values and the specified value selected.</returns>
		public static GeCombo ToEnumCombo<T>(this T selectedValue, int leftGap = 0, int rightGap = 0) where T : Enum
		{
			var names = new List<string>();
			foreach (var n in Enum.GetNames(typeof(T)))
				names.Add(n);

			return new GeCombo(names, names.IndexOf(selectedValue.ToString()), leftGap, rightGap);
		}

		/// <summary>
		/// Get the enum value from a <see cref="GeCombo"/> that was created with the <see cref="ToEnumCombo{T}"/> function.
		/// </summary>
		/// <param name="enumCombo">The <see cref="GeCombo"/> to use.</param>
		/// <typeparam name="T">The specific enum to use.</typeparam>
		/// <returns>The enum value the <see cref="GeCombo"/> had selected.</returns>
		public static T GetEnumValue<T>(this GeCombo enumCombo) where T : Enum
		{
			var enumText = enumCombo.Items[enumCombo.SelectedIndex];
			var enumValue = (T)Enum.Parse(typeof(T), enumText);
			return enumValue;
		}

		/// <summary>
		/// Take an enum value and create a <see cref="GeMultiCombo"/> for it.  The list will be populated with all enum values and the selected index will be the specified value.
		/// </summary>
		/// <param name="selectedValue">The enum value to select after the combo is constructed.</param>
		/// <param name="startExpanded">Start the combo expanded?</param>
		/// <param name="leftGap">The left margin in pixels.</param>
		/// <param name="rightGap">The right margin in pixels.</param>
		/// <typeparam name="T">The specific enum to use.</typeparam>
		/// <returns>A <see cref="GeMultiCombo"/> populated with the range of enum values and the specified value selected.</returns>
		public static GeMultiCombo ToMultiEnumCombo<T>(this T selectedValue, bool startExpanded, int leftGap = 0, int rightGap = 0) where T : Enum
		{
			var names = Enum.GetNames(typeof(T)).Skip(1).ToArray();
			var indexes = new List<int>();
			var selectedValueAsInt = Convert.ToInt32(selectedValue);

			for (var i = 0; i < names.Length; i++)
			{
				if ((selectedValueAsInt & 1 << i) == 1 << i)
					indexes.Add(i);
			}

			return new GeMultiCombo(names, indexes, startExpanded, leftGap, rightGap);
		}

		/// <summary>
		/// Get the enum value from a <see cref="GeMultiCombo"/> that was created with the <see cref="ToEnumCombo{T}"/> function.
		/// </summary>
		/// <param name="enumCombo">The <see cref="GeMultiCombo"/> to use.</param>
		/// <typeparam name="T">The specific enum to use.</typeparam>
		/// <returns>The enum value the <see cref="GeMultiCombo"/> had selected.</returns>
		public static T GetMultiEnumValue<T>(this GeMultiCombo enumCombo) where T : Enum
		{
			var enumValues = new List<string>();

			foreach (var selectedIndex in enumCombo.SelectedIndexes)
				enumValues.Add(enumCombo.Items[selectedIndex]);

			return (T)Enum.Parse(typeof(T), string.Join(", ", enumValues));
		}
	}
}