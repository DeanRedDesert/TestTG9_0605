using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using UnityEngine;

namespace Midas.Presentation.DevHelpers
{
	public sealed class SequencesView : TreeViewControl
	{
		private SequenceFinder sequenceFinder;
		private GUIStyle activeSequenceLabelStyle;
		private GUIStyle activeEventLabelStyle;
		private GUIStyle waitingEventCallbackLabelStyle;
		private GUIStyle breadCrumbStyle;

		public GUIStyle ActiveSequenceLabelStyle
		{
			get
			{
				if (activeSequenceLabelStyle == null)
				{
					activeSequenceLabelStyle = new GUIStyle(LabelStyle);
					activeSequenceLabelStyle.normal.textColor = Color.green;
				}

				return activeSequenceLabelStyle;
			}
			set => activeSequenceLabelStyle = value;
		}

		public GUIStyle WaitingEventCallbackLabelStyle
		{
			get
			{
				if (waitingEventCallbackLabelStyle == null)
				{
					waitingEventCallbackLabelStyle = new GUIStyle(LabelStyle);
					waitingEventCallbackLabelStyle.normal.textColor = Color.red;
				}

				return waitingEventCallbackLabelStyle;
			}
			set => waitingEventCallbackLabelStyle = value;
		}

		public GUIStyle ActiveEventLabelStyle
		{
			get
			{
				if (activeEventLabelStyle == null)
				{
					activeEventLabelStyle = new GUIStyle(LabelStyle);
					activeEventLabelStyle.normal.textColor = Color.cyan;
				}

				return activeEventLabelStyle;
			}
			set => activeEventLabelStyle = value;
		}

		protected override IEnumerable<object> GetChildren(object node)
		{
			switch (node)
			{
				case null:
				{
					var finder = SequenceFinder;
					return finder == null ? Enumerable.Empty<object>() : finder.SequenceInfo;
				}
				case SequenceInfo sequenceInfo:
					return sequenceInfo.Events.Select(e => (sequenceInfo, e)).Cast<object>();
				case ValueTuple<SequenceInfo, (string, int)> s:
					return s.Item1.Sequence is ISequenceDebug sequenceDebug ? sequenceDebug.GetSequenceEventCallbacks(s.Item2.Item2).Cast<object>() : Enumerable.Empty<object>();
				default:
					return Enumerable.Empty<object>();
			}
		}

		protected override bool HasChildren(object node)
		{
			switch (node)
			{
				case SequenceInfo sequenceInfo:
					return sequenceInfo.Events != null;
				case ValueTuple<SequenceInfo, (string, int)> s:
					return s.Item1.Sequence is ISequenceDebug sequenceDebug && sequenceDebug.GetSequenceEventCallbacks(s.Item2.Item2).Any();
				case null:
					return true;
			}

			return false;
		}

		protected override GUIStyle GetLabelStyle(object item)
		{
			switch (item)
			{
				case SequenceInfo sequenceInfo:
					return sequenceInfo.Sequence.IsActive ? ActiveSequenceLabelStyle : base.GetLabelStyle(item);
				case ValueTuple<SequenceInfo, (string, int)> tuple:
					return tuple.Item1.Sequence is ISequenceDebug sequenceDebug && sequenceDebug.IsInEvent(tuple.Item2.Item2) ? ActiveEventLabelStyle : base.GetLabelStyle(item);
				case ValueTuple<SequenceEventHandler, bool> sequenceEventCallbacksAndWaiting:
					return sequenceEventCallbacksAndWaiting.Item2 ? WaitingEventCallbackLabelStyle : base.GetLabelStyle(item);
			}

			return base.GetLabelStyle(item);
		}

		protected override string GetValueAsString(object item)
		{
			switch (item)
			{
				case SequenceInfo sequenceInfo:
					return $"{sequenceInfo.Sequence.Name} running:{sequenceInfo.Sequence.IsActive}";
				case ValueTuple<SequenceInfo, (string, int)> s:
					return $"{s.Item2.Item1}";
				case ValueTuple<SequenceEventHandler, bool> sequenceEventCallbacksAndWaiting:
					return sequenceEventCallbacksAndWaiting.Item2
						? $"Waiting: {FormatOwner(sequenceEventCallbacksAndWaiting.Item1.Owner)}"
						: $"{FormatOwner(sequenceEventCallbacksAndWaiting.Item1.Owner)}";
			}

			return string.Empty;
		}

		protected override void DrawItem(object item)
		{
#if UNITY_EDITOR
			if (item is ValueTuple<SequenceEventHandler, bool> valueTuple && valueTuple.Item1.Owner is MonoBehaviour mono)
			{
				var s = GetValueAsString(item);

				if (GUILayout.Button(s, GetBreadcrumbStyle()))
				{
					UnityEditor.EditorGUIUtility.PingObject(mono);
					UnityEditor.Selection.activeGameObject = mono.gameObject;
				}

				return;
			}
#endif

			base.DrawItem(item);
		}

		private static string FormatOwner(object owner)
		{
			if (owner is MonoBehaviour mono)
			{
				return $"{mono.GetPath()}({mono.GetType().Name})";
			}

			return owner.ToString();
		}

		private SequenceFinder SequenceFinder
		{
			get
			{
				if (sequenceFinder == null)
				{
					var gameInstance = GameBase.GameInstance;
					if (gameInstance != null)
					{
						sequenceFinder = gameInstance.GetPresentationController<SequenceFinder>();
					}
				}

				return sequenceFinder;
			}
		}

		private GUIStyle GetBreadcrumbStyle()
		{
			if (breadCrumbStyle == null)
			{
				var style = GUI.skin.FindStyle("GUIEditor.Breadcrumbleft");
				if (style != null)
				{
					breadCrumbStyle = new GUIStyle(style);
					breadCrumbStyle.hover.textColor = Color.white;
				}

				breadCrumbStyle ??= GUI.skin.label;
			}

			return breadCrumbStyle;
		}
	}
}