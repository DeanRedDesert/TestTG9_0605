using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Midas.Ascent.Cabinet.Lights.Visualiser.Devices
{
	public sealed class LedSchema : IEnumerator<Vector3>
	{
		/// <summary>
		/// Direction of the LED ellipse.
		/// </summary>
		public enum EllipseOrientation
		{
			Clockwise,
			CounterClockwise
		}

		public enum StripOrientation
		{
			/// <summary>
			/// The strip is horizontal and LEDs count from left to right.
			/// </summary>
			LeftToRight,

			/// <summary>
			/// The strip is horizontal and LEDs count from right to left.
			/// </summary>
			RightToLeft,

			/// <summary>
			/// The strip is vertical and LEDs count from top to bottom.
			/// </summary>
			TopToBottom,

			/// <summary>
			/// The strip is vertical and LEDs count from bottom to top.
			/// </summary>
			BottomToTop
		}

		private sealed class Section
		{
			public readonly Operator @Operator;
			public readonly int Count;

			private readonly float value;
			private readonly Vector3? point;
			private readonly Direction direction;

			public Vector3 Vector2
			{
				get
				{
					if (point.HasValue)
						return point.Value;

					var scale = Operator == Operator.Shift ? value : 1f;
					switch (direction)
					{
						case Direction.Down: return -Vector3.up * scale;
						case Direction.Right: return Vector3.right * scale;
						case Direction.Left: return -Vector3.right * scale;
						case Direction.Up: return Vector3.up * scale;
					}

					return Vector3.zero;
				}
			}

			public Section(Operator @operator, Direction direction, float value)
			{
				Operator = @operator;
				this.direction = direction;
				this.value = value;
				Count = Mathf.RoundToInt(value);
				point = null;
			}

			/// <summary>
			/// x - X position
			/// y - Y position
			/// z - Z rotation
			/// </summary>
			/// <param name="point"></param>
			public Section(Vector3 point)
			{
				Operator = Operator.None;
				direction = Direction.None;
				value = -1f;
				Count = 1;
				this.point = point;
			}
		}

		private enum Operator
		{
			None,
			Add,
			Shift
		}

		private enum Direction
		{
			None,
			Down,
			Left,
			Right,
			Up
		}

		private readonly LinkedList<Section> sections;
		private LinkedListNode<Section> currentSection;
		private int currentIndex;

		public LedSchema()
		{
			sections = new LinkedList<Section>();
			Reset();
		}

		public void AddUp(int numberOfLeds)
		{
			sections.AddLast(new Section(Operator.Add, Direction.Up, numberOfLeds));
		}

		public void ShiftUp(float unitsToShiftBy)
		{
			sections.AddLast(new Section(Operator.Shift, Direction.Up, unitsToShiftBy));
		}

		public void AddLeft(int numberOfLeds)
		{
			sections.AddLast(new Section(Operator.Add, Direction.Left, numberOfLeds));
		}

		public void ShiftLeft(float unitsToShiftBy)
		{
			sections.AddLast(new Section(Operator.Shift, Direction.Left, unitsToShiftBy));
		}

		public void AddDown(int numberOfLeds)
		{
			sections.AddLast(new Section(Operator.Add, Direction.Down, numberOfLeds));
		}

		public void ShiftDown(float unitsToShiftBy)
		{
			sections.AddLast(new Section(Operator.Shift, Direction.Down, unitsToShiftBy));
		}

		public void AddRight(int numberOfLeds)
		{
			sections.AddLast(new Section(Operator.Add, Direction.Right, numberOfLeds));
		}

		public void ShiftRight(float unitsToShiftBy)
		{
			sections.AddLast(new Section(Operator.Shift, Direction.Right, unitsToShiftBy));
		}

		public void LedStrip(int startingIndex, int endingIndex, StripOrientation ledOrientation, float startX = 0f,
			float startY = 0f, float angle = 0f)
		{
			if (endingIndex < startingIndex)
			{
				throw new InvalidOperationException("EndingIndex must be greater than or equal to StartingIndex");
			}

			var angleR = angle / 180f * (float)Math.PI;
			var range = (float)(endingIndex - startingIndex + 1);
			for (float i = 0; i < range; i++)
			{
				float x;
				float y;

				switch (ledOrientation)
				{
					case StripOrientation.BottomToTop: x = i * Mathf.Sin(angleR); y = i * Mathf.Cos(angleR); break;
					case StripOrientation.LeftToRight: x = i * Mathf.Cos(angleR); y = i * Mathf.Sin(angleR); break;
					case StripOrientation.TopToBottom: x = i * -Mathf.Sin(angleR); y = i * -Mathf.Cos(angleR); break;
					case StripOrientation.RightToLeft: x = i * -Mathf.Cos(angleR); y = i * -Mathf.Sin(angleR); break;
					default: throw new ArgumentOutOfRangeException(nameof(ledOrientation), ledOrientation, null);
				}

				sections.AddLast(new Section(new Vector3(x + startX, y + startY, -angle)));
			}
		}

		public void Ellipse(int startingIndex, int endingIndex, float angleOffset, EllipseOrientation ellipseOrientation = EllipseOrientation.Clockwise, float ellipseWidth = 2f, float ellipseHeight = 2f)
		{
			var ledCount = endingIndex - startingIndex + 1;

			// Divide circle by number of LEDs.
			var angleStep = 2 * Math.PI / ledCount;

			// Convert AngleOffset to radians.
			var angle = -(angleOffset / 180 * Math.PI);

			for (var ledIndex = startingIndex; ledIndex <= endingIndex; ++ledIndex)
			{
				var x = ellipseWidth * Math.Cos(angle);
				var y = ellipseHeight * Math.Sin(angle);

				// Offset by Radius so center is no longer at the origin.
				x += ellipseWidth;
				y += ellipseHeight;

				sections.AddLast(new Section(new Vector3((float)x, (float)y)));

				// Step through circle.
				switch (ellipseOrientation)
				{
					case EllipseOrientation.Clockwise: angle += -angleStep; break;
					case EllipseOrientation.CounterClockwise: angle += angleStep; break;
				}
			}
		}

		public Vector3 Current { get; private set; }

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			// Terminate if sections are not defined
			if (sections == null || sections.Count == 0)
				return false;

			// Get next section
			if (currentSection == null)
			{
				// First MoveNext() call right after Reset();

				if (sections.First == null)
					return false;

				currentIndex = -1;
				currentSection = sections.First;
			}
			else
			{
				// Standard next section after first MoveNext();

				// If we are not still iterating
				currentIndex = ++currentIndex;
				if (currentIndex >= currentSection.Value.Count)
				{
					// Try to get next section
					if (currentSection.Next == null)
						return false;

					currentIndex = 0;
					currentSection = currentSection.Next;
				}
			}

			// Iterate through no operating sections until we hit a different operation
			if (currentSection.Value.Operator == Operator.None)
			{
				// Set the current vector
				Current = currentSection.Value.Vector2;

				// If index is less than zero then claim this as it's first iteration
				currentIndex = currentIndex < 0 ? 0 : currentIndex;

				// Return true because we moved successfully
				return true;
			}

			// Iterate through shifting sections until we hit a different operation
			while (currentSection.Value.Operator == Operator.Shift)
			{
				// Shift the current vector
				Current += currentSection.Value.Vector2;

				// Try to get next section
				if (currentSection.Next == null)
					return false;

				currentIndex = -1;
				currentSection = currentSection.Next;
			}

			// Iterate through adding sections until we hit a different operation
			while (currentSection.Value.Operator == Operator.Add)
			{
				var index = currentIndex;
				var count = currentSection.Value.Count;

				// Append to the current vector as we build it but not on the first time
				if (index >= 0)
				{
					Current += currentSection.Value.Vector2;
				}

				// If index is less than zero then claim this as it's first iteration
				currentIndex = currentIndex < 0 ? 0 : currentIndex;

				// If the current adding section is finished iterating and needs to get the next section
				var moveNext = index >= count;

				// If we need to move to the next section
				if (moveNext)
				{
					// Try to get next section
					if (currentSection.Next == null)
						return false;

					currentSection = currentSection.Next;
				}
				else
				{
					// If we successfully added a point then return and wait until we want to get the next point.
					return true;
				}
			}

			return currentSection.Next != null;
		}

		public void Reset()
		{
			currentSection = null;
			Current = Vector3.zero;
			currentIndex = -1;
		}

		public void Dispose()
		{
			// No unmanaged resources allocated.
		}
	}
}