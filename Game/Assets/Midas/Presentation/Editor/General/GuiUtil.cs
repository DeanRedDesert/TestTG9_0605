using System;
using Midas.Core.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.General
{
	public static class GuiUtil
	{
		#region Public

		public static int CheckedPopup(Rect position, string selectedOption, string[] options)
		{
			var index = options.FindIndex(x => x.Equals(selectedOption));
			if (index == -1 && selectedOption.Length > 0)
			{
				using (new ChangeGuiColorScope(Color.red))
				{
					string[] tempOptions = new string[options.Length + 1];
					Array.Copy(options, tempOptions, options.Length);
					tempOptions[tempOptions.Length - 1] = "<missing> " + selectedOption;

					index = EditorGUI.Popup(position, tempOptions.Length - 1, tempOptions);
					return index == tempOptions.Length - 1 ? -1 : index;
				}
			}

			index = EditorGUI.Popup(position, index, options);
			return index;
		}

		#endregion
	}
}