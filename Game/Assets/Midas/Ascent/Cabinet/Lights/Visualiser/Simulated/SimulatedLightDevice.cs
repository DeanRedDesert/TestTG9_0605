// Copyright (C) 2019, IGT Australia Pty. Ltd.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;
using UnityEngine;

namespace Midas.Ascent.Cabinet.Lights.Visualiser.Simulated
{
	public sealed class SimulatedLightDevice : MonoBehaviour
	{
		#region Fields

		private StreamingLightHardware streamingLightHardware;

		/// <summary>
		/// The LEDs arranged in order on the light strip
		/// </summary>
		private SimulatedLed[] leds;

		/// <summary>
		/// The default intensity level for the leds in this device
		/// </summary>
		private byte intensity = 66;

		/// <summary>
		/// The queue to get the next light sequence from
		/// </summary>
		private readonly Queue<ILightSequence> lightSequenceQueue = new Queue<ILightSequence>();

		/// <summary>
		/// Current playing light sequence
		/// </summary>
		private ILightSequence currentLightSequence;

		/// <summary>
		/// Streaming Seeker used to access any frame with a given time in the light sequence duration.
		/// </summary>
		private LightSequenceSeeker.StreamingSeek streamingSeek;

		/// <summary>
		/// The current elapsed time of the light sequence playback.
		/// </summary>
		private float time;

		#endregion

		#region Properties

		public SimulatedLed[] Leds
		{
			get => IsPopulatedLeds() ? leds : null;
			set => leds = value;
		}

		public byte SupportedLightVersion => 2;

		#endregion

		#region Unity Methods

		private void Awake()
		{
			IsPopulatedLeds();
		}

		private void Update()
		{
			if (currentLightSequence == null)
			{
				if (lightSequenceQueue.Count > 0)
				{
					currentLightSequence = lightSequenceQueue.Dequeue();
				}
			}

			if (currentLightSequence != null)
			{
				streamingSeek ??= new LightSequenceSeeker.StreamingSeek(currentLightSequence);

				var seekData = streamingSeek.Seek(time);
				if (seekData?.Frames != null)
				{
					foreach (var seekDataFrame in seekData.Frames)
					{
						SetFrame(seekDataFrame);
					}
				}

				time += Time.deltaTime;
			}
		}

		#endregion

		#region Public Methods

		public static GameObject Construct(Transform parent, Vector3 localPosition, StreamingLightHardware streamingLightHardware)
		{
			var name = $"{streamingLightHardware}";
			var gobj = new GameObject
			{
				name = name,
				layer = LayerMask.NameToLayer("UI")
			};

			var simulatedLightDevice = gobj.AddComponent<SimulatedLightDevice>();
			simulatedLightDevice.streamingLightHardware = streamingLightHardware;

			gobj.transform.SetParent(parent);
			gobj.transform.localPosition = localPosition;
			gobj.transform.localRotation = Quaternion.identity;
			gobj.transform.localScale = Vector3.one;

			return gobj;
		}

		// ReSharper disable once MemberCanBeMadeStatic.Global currently not implemented
		[SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "Currently not implemented")]
		public void BreakLoop(byte groupNumber)
		{
		}

		public byte GetIntensity()
		{
			return intensity;
		}

		public StreamingLightHardware GetStreamingLightHardware()
		{
			return streamingLightHardware;
		}

		// ReSharper disable once MemberCanBeMadeStatic.Global currently not implemented
		[SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "Currently not implemented")]
		public void SendFrameChunk(byte groupNumber, uint frameCount, byte[] frameData, StreamingLightsPlayMode playMode)
		{
		}

		public void SetIntensity(byte intensityValue)
		{
			intensity = intensityValue;
		}

		// ReSharper disable once MemberCanBeMadeStatic.Global currently not implemented
		[SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "Currently not implemented")]
		public void StartSequenceFile(byte groupNumber, string filePath, StreamingLightsPlayMode playMode)
		{
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "Currently not implemented")]
		public void StartSequenceFile(byte groupNumber, string sequenceName, byte[] sequenceFile, StreamingLightsPlayMode playMode)
		{
			// Set starting times
			PlayLightSequence(new LightSequence(new MemoryStream(Convert.FromBase64String(Encoding.ASCII.GetString(sequenceFile)))), playMode);
		}

		#endregion

		#region Private Methods

		private void PlayLightSequence(ILightSequence lightSequence, StreamingLightsPlayMode playMode)
		{
			if (streamingLightHardware != lightSequence.LightDevice)
				return;

			switch (playMode)
			{
				case StreamingLightsPlayMode.Restart:
					lightSequenceQueue.Clear();
					lightSequenceQueue.Enqueue(lightSequence);
					currentLightSequence = null;
					streamingSeek = null;
					time = 0f;
					break;
				case StreamingLightsPlayMode.Continue:
					if (currentLightSequence.Segments.Count == lightSequence.Segments.Count && currentLightSequence.Segments.Sum(x => x.Frames.Count) == lightSequence.Segments.Sum(x => x.Frames.Count))
					{
						lightSequenceQueue.Clear();
						lightSequenceQueue.Enqueue(lightSequence);
						currentLightSequence = null;
						streamingSeek = null;
					}

					break;
				case StreamingLightsPlayMode.Queue:
					lightSequenceQueue.Enqueue(lightSequence);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(playMode), playMode, null);
			}
		}

		private void SetFrame(Frame frame)
		{
			var colorFrameData = frame.ColorFrameData;

			if (Leds != null)
			{
				for (var i = 0; i < colorFrameData.Count; i++)
				{
					if (i >= Leds.Length)
						return;

					Leds[i].SetColor(colorFrameData[i], 255);
				}
			}
		}

		private bool IsPopulatedLeds()
		{
			var isPopulated = leds != null && leds.Length > 0;

			if (!isPopulated)
			{
				leds = GetComponentsInChildren<SimulatedLed>();
				isPopulated = leds != null && leds.Length > 0;
			}

			return isPopulated;
		}

		#endregion
	}
}