using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Data;
using Midas.Presentation.Data.PropertyReference;
using Midas.Tools.Editor;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public sealed class FindStatusItem : FindReferencesBase
	{
		#region Fields

		private readonly List<string> nameList = new List<string>();

		#endregion

		#region FindReferencesBase

		protected override string[] PopupNameList => nameList.ToArray();

		protected override string HelpBoxMessage => "Find all Status Items that are in use.";

		protected override string SearchFieldLabel => "Status Item";

		#endregion

		#region Private Methods

		private void OnEnable()
		{
			var statusItemProperties = PropertyPathResolver.CollectProperties(typeof(object));

			nameList.Clear();
			nameList.AddRange(StatusDatabase.IsInitialised
				? statusItemProperties.Select(v => v.Name.Replace('.', '/')).ToArray()
				: new[] { "--StatusDatabase unavailable--" });
		}

		[MenuItem("Midas/Find/Find Status Item")]
		public static void ShowWindow() => GetWindow<FindStatusItem>("Midas-Find Status Item");

		protected override IEnumerable<(Object Object, string Path)> Find() => new ResourceFinderWithProgress<Object, (Object, string)>().Find(InspectBehaviour);

		private IEnumerable<(Object, string)> InspectBehaviour(Object o)
		{
			var selection = SelectionName.Replace("/", ".");

			return o.GetAllSerializedFieldsRecursive<PropertyReference>()
				.Where(r => r.Path.IndexOf(selection, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
				.Select(r => (o, r.Path));
		}

		#endregion
	}
}