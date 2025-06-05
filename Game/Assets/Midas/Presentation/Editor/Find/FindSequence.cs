using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public sealed class FindSequence : FindReferencesBase
	{
		private SequenceFinder sequenceFinder;
		private string[] popupNameList;

		public SequenceFinder SequenceFinder => sequenceFinder ??= GameBase.GameInstance.GetPresentationController<SequenceFinder>();

		[MenuItem("Midas/Find/Find Sequence")]
		public static void ShowWindow()
		{
			GetWindow<FindSequence>("Midas-Find Sequence");
		}

		protected override IEnumerable<(Object Object, string Path)> Find()
		{
			return new ResourceFinderWithProgress<SequenceActivatorBase, (Object GameObject, string Path)>().Find(InspectSequenceActivator);

			IEnumerable<(Object, string)> InspectSequenceActivator(SequenceActivatorBase sa)
			{
				foreach (var sn in sa.SequenceNames)
				{
					if (sn.IndexOf(SelectionName, StringComparison.OrdinalIgnoreCase) >= 0)
						yield return (sa, sn);
				}
			}
		}

		protected override string[] PopupNameList => popupNameList ??= CreateNameList();

		protected override string HelpBoxMessage => "Find all Sequences that are in use.";

		protected override string SearchFieldLabel => "Sequence";

		private string[] CreateNameList()
		{
			return SequenceFinder.Sequences.ToArray()
				.Select(n => n.Name)
				.ToArray();
		}
	}
}