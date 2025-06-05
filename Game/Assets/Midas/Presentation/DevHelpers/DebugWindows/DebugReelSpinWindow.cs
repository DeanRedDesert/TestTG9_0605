using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Core;
using Midas.Presentation.Data;
using Midas.Presentation.Reels;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugReelSpinWindow : DebugWindow
	{
		private SuspenseReelSpin[] spinControllers;
		private SuspenseReelSpin activeController;
		private DefaultReelSpinTimings timings;
		private readonly Data[][] savedItems = new Data[3][];
		private Data[] origItems = Array.Empty<Data>();
		private Rect originalSize;
		private bool isHidden;
		private bool lastIsHidden;

		private Data[] items =
		{
			new Data("Min Spin Time", 1f, 0f, 5f),
			new Data("Min Stop Time", 0.2f, 0f, 2f),
			new Data("Max Stop Time", 0.4f, 0f, 2f),
			new Data("Spin Speed", 12.5f, 1f, 30f),
			new Data("Overshoot Distance", 0f, 0f, 2f),
			new Data("Recovery Time", 0f, 0f, 2f),
			new Data("Spin Frame Rate", 0f, 0f, 60f, 1f),
		};

		public DebugReelSpinWindow()
		{
			originalSize = WindowRect;
		}

		public void Update()
		{
			var shouldHide = StatusDatabase.GameStatus.GameIsActive || isHidden;

			if (lastIsHidden == shouldHide)
				return;

			if (shouldHide)
				originalSize = Resize(new Rect(WindowRect.x, WindowRect.y, WindowRect.width, 80));
			else
				Resize(originalSize);
			lastIsHidden = shouldHide;

			Rect Resize(Rect rect)
			{
				var wr = WindowRect;
				WindowRect = rect;
				return wr;
			}
		}

		protected override void RenderWindowContent()
		{
			if (StatusDatabase.GameStatus.GameMode != FoundationGameMode.Play)
				return;

			if (StatusDatabase.GameStatus.GameIsActive)
				return;

			if (isHidden)
			{
				if (GUILayout.Button("Show", ButtonStyle, GUILayout.Width(100)))
					isHidden = false;
				return;
			}

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Hide", ButtonStyle, GUILayout.Width(100)))
				isHidden = true;

			if (GUILayout.Button("Orig", ButtonStyle, GUILayout.Width(100)))
				items = Copy(origItems);

			for (var i = 0; i < 3; i++)
			{
				if (GUILayout.Button($"Save {i + 1}", ButtonStyle, GUILayout.Width(100)))
					savedItems[i] = Copy(items.ToArray());

				if (GUILayout.Button($"Load {i + 1}", ButtonStyle, GUILayout.Width(100)))
					items = Copy(savedItems[i]);
			}

			GUILayout.EndHorizontal();

			foreach (var item in items)
				RenderSlider(item);

			UpdateData();
		}

		private void UpdateData()
		{
			if (timings == null)
				return;

			timings.UpdateTimings(items[0].Value, items[1].Value, items[2].Value, items[3].Value, 0, 0, items[4].Value, items[5].Value, (int)items[6].Value);
		}

		private void RenderSlider(Data data)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(data.Label, LabelStyle, GUILayout.Width(250));
			GUILayout.Label($"{data.Value:F1}", LabelStyle, GUILayout.Width(80));
			if (GUILayout.Button("-", ButtonStyle, GUILayout.Width(20)))
			{
				if (data.Value - data.Delta >= data.MinValue)
					data.Value -= data.Delta;
				else
					data.Value = data.MinValue;
			}

			data.Value = GUILayout.HorizontalSlider(data.Value, data.MinValue, data.MaxValue, GUILayout.Width(500));
			data.Value = Mathf.Round(data.Value / 0.1f) * 0.1f;

			if (GUILayout.Button("+", ButtonStyle, GUILayout.Width(20)))
			{
				if (data.Value + data.Delta <= data.MaxValue)
					data.Value += data.Delta;
				else
					data.Value = data.MaxValue;
			}

			GUILayout.EndHorizontal();
		}

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Reel Spin";
			FontSize = 24;
			WindowRect = new Rect(1000, 115, 700, 1000);
			BackgroundColor = new Color(0, 0.7f, 1);
		}

		private void OnEnable()
		{
			if (activeController != null)
				return;

			Initialise();
		}

		private void Initialise()
		{
			spinControllers = FindObjectsOfType<SuspenseReelSpin>(true);
			activeController = spinControllers.FirstOrDefault(sc => sc.gameObject.activeInHierarchy);

			if (activeController == null)
				return;

			var timingsField = activeController.GetType().GetField("timings", BindingFlags.Instance | BindingFlags.NonPublic);

			if (timingsField == null)
				return;
			var t = (MultiReelSpinTimings)timingsField.GetValue(activeController);

			var anzTimingsField = typeof(MultiReelSpinTimings).GetField("AnzSpinTimings", BindingFlags.Instance | BindingFlags.NonPublic);
			if (anzTimingsField == null)
				return;

			timings = (DefaultReelSpinTimings)anzTimingsField.GetValue(t);

			items[0].Value = GetFloatValue(timings, "minSpinTime");
			items[1].Value = GetFloatValue(timings, "minStopInterval");
			items[2].Value = GetFloatValue(timings, "maxStopInterval");
			items[3].Value = GetFloatValue(timings, "spinSpeed");
			items[4].Value = GetFloatValue(timings, "overshootDistance");
			items[5].Value = GetFloatValue(timings, "recoveryTime");
			items[6].Value = GetIntValue(timings, "spinFrameRate");

			origItems = Copy(items);

			float GetFloatValue(object o, string fieldName)
			{
				var field = typeof(DefaultReelSpinTimings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				if (field == null)
					return 0f;
				return (float)field.GetValue(o);
			}

			int GetIntValue(object o, string fieldName)
			{
				var field = typeof(DefaultReelSpinTimings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				if (field == null)
					return 0;
				return (int)field.GetValue(o);
			}
		}

		private static Data[] Copy(IReadOnlyList<Data> data)
		{
			var ret = new Data[data.Count];
			for (var i = 0; i < data.Count; i++)
				ret[i] = new Data(data[i]);

			return ret;
		}

		private void OnDestroy()
		{
			if (!Application.isEditor || timings == null)
				return;

			items = Copy(origItems);
			UpdateData();
		}

		private sealed class Data
		{
			public readonly string Label;
			public float Value;
			public readonly float MinValue;
			public readonly float MaxValue;
			public readonly float Delta;

			public Data(string label, float value, float minValue, float maxValue, float delta = 0.1f)
			{
				Label = label;
				Value = value;
				MinValue = minValue;
				MaxValue = maxValue;
				Delta = delta;
			}

			public Data(Data data)
			{
				Label = data.Label;
				Value = data.Value;
				MinValue = data.MinValue;
				MaxValue = data.MaxValue;
				Delta = data.Delta;
			}
		}
	}
}