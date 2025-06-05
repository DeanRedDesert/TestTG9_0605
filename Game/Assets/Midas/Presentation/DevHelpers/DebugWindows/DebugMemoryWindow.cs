using System;
using System.Collections.Generic;
using Midas.Presentation.Audio;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugMemoryWindow : DebugWindow
	{
		private static string PlayingAudioClip => "Playing AudioClip";
		private static string PlayingVideoClip => "Playing VideoClip";

		private bool more = false;
		private int updateIndex = 0;
		private (string Name, Func<int> CountingFunc)[] objectCounterCalculator;
		private int[] objectCounter;

		private long lastGcMemory = 0;
		private long maxGcMemory = 0;

		private int currentWasterIdx = 0;

		private readonly int[] memoryWaster = new[]
		{
			0, 10, 100, 1000
		};

		private readonly string[] memoryWasteStrings;

		private readonly List<Func<(string, string)>> customValues = new List<Func<(string, string)>>();

		public DebugMemoryWindow()
		{
			memoryWasteStrings = new string[memoryWaster.Length];
			for (int i = 0; i < memoryWaster.Length; i++)
			{
				int amount = memoryWaster[i];
				if (amount == 0)
				{
					memoryWasteStrings[i] = "Extra memory waste: off";
				}
				else
				{
					memoryWasteStrings[i] = $"Extra memory waste: {amount}KB/Frame";
				}
			}
		}

		public void AddCustomValue(Func<(string, string)> f)
		{
			customValues.Add(f);
		}

		public void RemoveCustomValue(Func<(string, string)> f)
		{
			customValues.Remove(f);
		}

		protected override void RenderWindowContent()
		{
			UpdateValues();

			GUILayout.Label($"GC Mode                : {GarbageCollector.GCMode}", LabelStyle);
			GUILayout.Label($"Incremental GC enabled : {GarbageCollector.isIncremental}", LabelStyle);
			GUILayout.Label($"Incremental GC interval: {GarbageCollector.incrementalTimeSliceNanoseconds / 1000000.0} ms", LabelStyle);
			GUILayout.Space(10);

			GUILayout.Label($"Total GCs              : {GC.CollectionCount(0)}", LabelStyle);
			GUILayout.Label($"Total Memory           : {lastGcMemory / 1024} (max: {maxGcMemory / 1024})", LabelStyle);
			GUILayout.Space(10);

			AddMoreStatistics();
			AddWasteButton();
		}

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Memory";
			FontSize = 24;
			WindowRect = new Rect(1000, 115, 700, 1000);
			BackgroundColor = new Color(0, 0.7f, 1);
		}

		private void OnEnable()
		{
			objectCounterCalculator = new (string Name, Func<int> CountingFunc)[]
			{
				("Objects", () => Resources.FindObjectsOfTypeAll(typeof(Object)).Length),
				("", null),

				("GameObjects", () => Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length),
				("MonoBehaviours", () => Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour)).Length),
				("Components", () => Resources.FindObjectsOfTypeAll(typeof(Component)).Length),
				("Materials", () => Resources.FindObjectsOfTypeAll(typeof(Material)).Length),
				("Textures", () => Resources.FindObjectsOfTypeAll(typeof(Texture)).Length),
				("Sprites", () => Resources.FindObjectsOfTypeAll(typeof(Sprite)).Length),
				("", null),

				("ISound", () => AudioService.SoundCount),
				("AudioSourcesCreated", () => AudioService.TotalAudioSourcesCreated),
				("AudioSourcesAcquired", () => AudioService.NumAudioSourcesCurrentlyAcquired),
			};

			objectCounter = new int[objectCounterCalculator.Length];
		}

		private void AddMoreStatistics()
		{
			more = GUILayout.Toggle(more, "More Statistics", ButtonStyle);
			if (more)
			{
				for (int idx = 0; idx < objectCounter.Length; ++idx)
				{
					if (objectCounterCalculator[idx].CountingFunc != null)
					{
						GUILayout.Label($"{objectCounterCalculator[idx].Name,40} : {objectCounter[idx],6}", LabelStyle);
					}
					else
					{
						GUILayout.Space(10);
					}
				}

				//Audio
				var audioSources = Resources.FindObjectsOfTypeAll(typeof(AudioSource));
				foreach (var audioSource in audioSources)
				{
					var s = (AudioSource)audioSource;
					if (s.clip != null && s.isPlaying)
					{
						GUILayout.Label($"{PlayingAudioClip,40} : {s.clip.name}", LabelStyle);
					}
				}

				//Video
				var videoPlayers = Resources.FindObjectsOfTypeAll(typeof(VideoPlayer));
				foreach (var videoPlayer in videoPlayers)
				{
					var v = (VideoPlayer)videoPlayer;
					if (v.clip != null && v.isPlaying)
					{
						GUILayout.Label($"{PlayingVideoClip,40} : {v.clip.name}", LabelStyle);
					}
				}

				foreach (var custom in customValues)
				{
					var result = custom();
					GUILayout.Label($"{result.Item1,40} : {result.Item2}", LabelStyle);
				}
			}
		}

		private void AddWasteButton()
		{
			if (GUILayout.Button(memoryWasteStrings[currentWasterIdx], ButtonStyle))
			{
				if (currentWasterIdx < memoryWasteStrings.Length - 1)
				{
					currentWasterIdx++;
				}
				else
				{
					currentWasterIdx = 0;
				}
			}
		}

		private void UpdateValues()
		{
			lastGcMemory = GC.GetTotalMemory(false);
			if (lastGcMemory > maxGcMemory)
			{
				maxGcMemory = lastGcMemory;
			}

			if (more)
			{
				while (objectCounterCalculator[updateIndex].CountingFunc == null)
				{
					updateIndex = ++updateIndex % objectCounter.Length;
				}

				objectCounter[updateIndex] = objectCounterCalculator[updateIndex].CountingFunc();
				updateIndex = ++updateIndex % objectCounter.Length;
			}

			int amount = memoryWaster[currentWasterIdx];
			if (amount > 0)
			{
				int parts = Time.frameCount % 4; //sometimes big chunks, sometimes many small chunks
				int[][] wasted = new int[parts][];
				for (int part = 0; part < parts; part++)
				{
					wasted[part] = new int[amount / parts * 1024];
				}
			}
		}
	}
}