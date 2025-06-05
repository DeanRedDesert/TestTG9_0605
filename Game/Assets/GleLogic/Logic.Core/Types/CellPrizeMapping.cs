namespace Logic.Core.Types
{
	/// <summary>
	/// A mapping to associate a <see cref="CellPrizeResult"/> with an <see cref="object"/>.
	/// </summary>
	public sealed class CellPrizeMapping
	{
		/// <summary>
		/// The <see cref="CellPrizeResult"/> component of this mapping.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public CellPrizeResult Prize { get; }

		/// <summary>
		/// An arbitrary <see cref="object"/> associated with the prize.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public object Object { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="prize">The cell prize result.</param>
		/// <param name="obj">An arbitrary object associated with the prize result.</param>
		public CellPrizeMapping(CellPrizeResult prize, object obj)
		{
			Prize = prize;
			Object = obj;
		}
	}
}