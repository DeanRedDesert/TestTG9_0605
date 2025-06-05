using System;

namespace Midas.Ascent.Cabinet
{
	/// <summary>
	/// The data structure defines a size of a rectangle.
	/// </summary>
	internal readonly struct SizeRect
	{
		/// <summary>
		/// Gets the width of the rectangle.
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Gets the height of the rectangle.
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// Initialize a new instance of <see cref="SizeRect" />.
		/// </summary>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when either parameter is negative.
		/// </exception>
		public SizeRect(int width, int height)
		{
			Width = width;
			Height = height;

			if (width < 0)
			{
				throw new ArgumentException("Width cannot be negative.", nameof(width));
			}

			if (height < 0)
			{
				throw new ArgumentException("Height cannot be negative.", nameof(height));
			}
		}
	}
}