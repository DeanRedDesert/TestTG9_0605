using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Utility;
using Midas.Gle.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.DevHelpers;
using UnityEngine;

namespace Midas.Gle.Presentation
{
	public sealed class GleResultsView : TreeViewControl
	{
		private readonly Font font;
		private GUIStyle style;

		public GleResultsView(Font font) => this.font = font;

		#region TreeViewControl Overrides

		protected override IEnumerable<object> GetChildren(object node)
		{
			while (true)
			{
				switch (node)
				{
					case GleResult gleResult:
					{
						var inputObjects = GetInputObjects(gleResult);
						var srObjects = GetStageResultObjects(gleResult);
						return new object[] { new Data("Inputs", inputObjects), new Data("Stage Results", srObjects) };
					}
					case Data data:
						return data.Children;
					case null:
						return StatusDatabase.StatusBlocksInstance != null
							? StatusDatabase.QueryStatusBlock<GleStatus>(false)?.GameResults ?? Enumerable.Empty<object>()
							: Enumerable.Empty<object>();
					default:
						return Enumerable.Empty<object>();
				}
			}
		}

		protected override GUIStyle GetLabelStyle(object item)
		{
			if (style != null)
				return style;

			style = new GUIStyle(LabelStyle)
			{
				font = font,
				fontSize = LabelStyle.fontSize + 1
			};
			return style;
		}

		private static List<object> GetStageResultObjects(GleResult gleResult)
		{
			var objects = new List<object>();

			foreach (var sr in gleResult.Current.StageResults)
			{
				var name = $"{sr.Name} (PI-{sr.ProcessorIndex})";
				if (sr.Value == null)
				{
					objects.Add($"{name}: null");
					continue;
				}

				if (StringConverter.TryToString(sr.Value, "SL", out var str1))
				{
					objects.Add($"{name}: {str1}");
					continue;
				}

				if (StringConverter.TryToString(sr.Value, "ML", out var str2))
				{
					if (string.IsNullOrEmpty(str2) || str2 == "empty")
					{
						objects.Add($"{name}: empty");
						continue;
					}

					var lines = str2.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
					if (lines.Length > 0)
						objects.Add(new Data($"{name}:", lines));
					else
						objects.Add($"{name}: empty");
					continue;
				}

				objects.Add($"{name}: unable to display type {sr.Value.GetType().ToDisplayString()}");
			}

			return objects;
		}

		private static List<object> GetInputObjects(GleResult gleResult)
		{
			var objects = new List<object>();
			foreach (var input in gleResult.Current.Inputs)
			{
				if (input.Value is Cycles c)
				{
					StringConverter.TryToString(c, "ML", out var cl);
					objects.Add(new Data(input.Name, cl.ToLines(false, false)));
					continue;
				}

				if (StringConverter.TryToString(input.Value, "SL", out var sl))
				{
					objects.Add($"{input.Name}: {sl}");
					continue;
				}

				if (StringConverter.TryToString(input.Value, "ML", out var ml))
				{
					var sp = ml.ToLines(false, false);

					if (sp.Count == 1)
					{
						objects.Add($"{input.Name}: {sp[0]}");
						continue;
					}

					objects.Add(new Data(input.Name, sp));
				}
			}

			return objects;
		}

		protected override bool HasChildren(object node) =>
			node switch
			{
				GleResults gleResults => gleResults.Count > 0,
				Data _ => true,
				GleResult _ => true,
				_ => false
			};

		protected override string GetValueAsString(object item) =>
			item switch
			{
				GleResult gleResult => $"{gleResult.CurrentCycle.Stage}{(gleResult.CurrentCycle.CycleId == null ? "" : $"({gleResult.CurrentCycle.CycleId})")}: {gleResult.Current.AwardedPrize.ToStringOrThrow("SL")} ({gleResult.Current.TotalAwardedPrize.ToStringOrThrow("SL")})",
				Data d => d.Name,
				StageResults _ => "Stage Results",
				null => "(null)",
				string s => s,
				_ => $"{item}"
			};

		#endregion

		private sealed class Data
		{
			public string Name { get; }

			public IReadOnlyList<object> Children { get; }

			public Data(string name, IReadOnlyList<object> children)
			{
				Name = name;
				Children = children;
			}
		}
	}
}