using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;

namespace Midas.Ascent.Cabinet.Lights.Visualiser
{
	public static class LightSequenceSeeker
	{
		#region Public Methods

		public static SeekData Seek(this ILightSequence lightSequence, float t)
		{
			var loop = lightSequence.Loop;
			var elapsedTime = 0f;
			var length = lightSequence.DisplayTime / 1000f;

			if (!loop && t > length)
			{
				return null;
			}

			var tSafe = Roll(t, length);

			for (var i = 0; i < lightSequence.Segments.Count; i++)
			{
				var lightSequenceSegment = lightSequence.Segments[i];

				for (var j = 0; j < lightSequenceSegment.Loop + 1; j++)
				{
					for (var k = 0; k < lightSequenceSegment.Frames.Count; k++)
					{
						var frame = lightSequenceSegment.Frames[k];
						var lastFrame = frame;

						if (loop)
							elapsedTime += Roll(frame.TotalDisplayTime / 1000f, length);
						else
							elapsedTime += frame.TotalDisplayTime / 1000f;

						if (elapsedTime > tSafe)
						{
							return new SeekData
							{
								Frames = new[] { lastFrame }
							};
						}
					}
				}
			}

			return null;
		}

		#endregion

		#region Private Methods

		private static float Roll(float a, float count)
		{
			return a % count;
		}

		#endregion

		#region Sub Classes

		public sealed class SeekData
		{
			#region Properties

			public Frame[] Frames { get; set; }

			#endregion
		}

		public sealed class StreamingSeek
		{
			#region Fields

			/// <summary>
			/// Stores how many times a segment loops
			/// </summary>
			private readonly List<ushort> segmentLoops = new List<ushort>();

			/// <summary>
			/// The time of which the current frame starts at in the sequence
			/// It is the current position in time for the frame we are on
			/// </summary>
			private float position;

			#endregion

			#region Properties

			/// <summary>
			/// The light sequence being used
			/// </summary>
			private ILightSequence LightSequence { get; }

			/// <summary>
			/// Does the light sequence loop?
			/// </summary>
			private bool Loop { get; }

			/// <summary>
			/// How long the light sequence
			/// </summary>
			private float Duration { get; }

			/// <summary>
			/// The current index of the segment we are on
			/// </summary>
			private int CurrentSegmentIndex { get; set; }

			/// <summary>
			/// The current index of the frame of the current segment we are on
			/// </summary>
			private int CurrentFrameIndex { get; set; }

			#endregion

			#region Constructors

			public StreamingSeek(ILightSequence lightSequence)
			{
				LightSequence = lightSequence;
				Loop = lightSequence.Loop;
				Duration = lightSequence.DisplayTime / 1000f;

				CurrentFrameIndex = CurrentSegmentIndex = 0;

				segmentLoops.Clear();
				segmentLoops.AddRange(LightSequence.Segments.Select(x => x.Loop));
			}

			#endregion

			#region Public Methods

			public SeekData Seek(float t)
			{
				var frames = new List<Frame>();
				var dt = Roll(t, Duration) - position;

				var currentFrame = GetCurrentFrame();
				var currentFrameDuration = currentFrame.TotalDisplayTime / 1000f;
				frames.Add(currentFrame);

				// Position from 0 inclusive to 1 exclusive in the current frame
				var framePosition = (int)(dt / currentFrameDuration * 100000f) / 100000f;
				while (framePosition < 0f || framePosition >= 1f)
				{
					// Get the direction the frame is going
					var direction = framePosition < 0f ? -1 : framePosition >= 1f ? 1 : 0;

					// Increment the position as we are going to the next frame
					if (direction > 0)
						position += currentFrameDuration;

					// Get the next/previous frame
					currentFrame = GetFrame(direction);
					currentFrameDuration = currentFrame.TotalDisplayTime / 1000f;
					frames.Add(currentFrame);

					// Increment the position as we are going to the previous frame
					if (direction < 0)
						position -= currentFrameDuration;

					// Update the frame position
					dt = Roll(t, Duration) - position;
					framePosition = (int)(dt / currentFrameDuration * 100000f) / 100000f;
				}

				return new SeekData
				{
					Frames = frames.ToArray()
				};
			}

			#endregion

			#region Private Methods

			private bool DecrementSegment()
			{
				var currentSegmentLoop = LightSequence.Segments[CurrentSegmentIndex].Loop;
				var currentSegmentLoopCount = segmentLoops[CurrentSegmentIndex];

				if (currentSegmentLoop == currentSegmentLoopCount)
				{
					// Go to previous segment
					if (--CurrentSegmentIndex < 0)
					{
						if (Loop)
						{
							CurrentSegmentIndex = LightSequence.Segments.Count - 1;
						}
						else
						{
							return false;
						}
					}

					// Reset the segment's loop count
					LightSequence.Segments[CurrentSegmentIndex].Loop = 0;
				}
				else
				{
					++LightSequence.Segments[CurrentSegmentIndex].Loop;
				}

				return true;
			}

			private bool IncrementSegment()
			{
				var currentSegmentLoopCount = CurrentSegmentIndex < 0 || CurrentSegmentIndex >= LightSequence.Segments.Count
					? (ushort)0
					: LightSequence.Segments[CurrentSegmentIndex].Loop;

				if (currentSegmentLoopCount == 0)
				{
					// Reset the segment's loop count
					LightSequence.Segments[CurrentSegmentIndex].Loop = segmentLoops[CurrentSegmentIndex];

					if (++CurrentSegmentIndex >= LightSequence.Segments.Count)
					{
						if (Loop)
						{
							CurrentSegmentIndex = 0;
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					--LightSequence.Segments[CurrentSegmentIndex].Loop;
				}

				return true;
			}

			private Segment GetCurrentSegment()
			{
				if (CurrentSegmentIndex < 0)
				{
					var loopCount = segmentLoops[0];

					if (LightSequence.Segments[0].Loop == loopCount)
					{
						if (Loop)
						{
							CurrentSegmentIndex = LightSequence.Segments.Count - 1;
						}
						else
						{
							return null;
						}
					}
					else
					{
						++LightSequence.Segments[0].Loop;
					}
				}
				else if (CurrentSegmentIndex >= LightSequence.Segments.Count)
				{
					var lastSegment = LightSequence.Segments[LightSequence.Segments.Count - 1];
					if (lastSegment.Loop > 0)
					{
						--lastSegment.Loop;
					}
					else
					{
						// Reset the segment's loop count
						lastSegment.Loop = segmentLoops[LightSequence.Segments.Count - 1];

						if (Loop)
						{
							CurrentSegmentIndex = 0;
						}
						else
						{
							return null;
						}
					}
				}

				return LightSequence.Segments[CurrentSegmentIndex];
			}

			private Frame GetCurrentFrame()
			{
				var segment = GetCurrentSegment();
				if (segment == null)
					return null;

				if (CurrentFrameIndex < 0)
				{
					if (DecrementSegment())
					{
						CurrentFrameIndex = LightSequence.Segments[CurrentSegmentIndex].Frames.Count - 1;
					}
					else
					{
						return null;
					}
				}
				else if (CurrentFrameIndex >= segment.Frames.Count)
				{
					if (IncrementSegment())
					{
						CurrentFrameIndex = 0;
					}
					else
					{
						return null;
					}
				}

				segment = GetCurrentSegment();
				return segment.Frames[CurrentFrameIndex];
			}

			private Frame NextFrame()
			{
				++CurrentFrameIndex;
				return GetCurrentFrame();
			}

			private Frame PreviousFrame()
			{
				--CurrentFrameIndex;
				return GetCurrentFrame();
			}

			private Frame GetFrame(int direction)
			{
				switch (direction)
				{
					case -1:
						return PreviousFrame();

					case 0:
						return LightSequence.Segments[CurrentSegmentIndex].Frames[CurrentFrameIndex];

					case 1:
						return NextFrame();
				}

				throw new ArgumentOutOfRangeException(nameof(direction), "Direction not valid! Must be (-1) for previous frame, 0 for current frame or 1 for next frame.");
			}

			#endregion
		}

		#endregion
	}
}