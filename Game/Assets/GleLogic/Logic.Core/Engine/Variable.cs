namespace Logic.Core.Engine
{
	/// <summary>
	/// A variable input for passing data from cycle to cycle with a given lifespan.
	/// </summary>
	public sealed class Variable : Input
	{
		/// <summary>
		/// The lifespan of the Variable, determines when an input will be added and removed from the cycle inputs
		///  OneCycle: Is kept in the Inputs for ONE cycle, then removed before the next cycle
		///  OneGame: Is kept in the Inputs for ONE game, then removed before the next game, can be updated during the game
		///  Permanent: Once added to the Inputs it is never removed, but can be updated
		/// </summary>
		public Lifespan Lifespan { get; }

		/// <summary>
		/// Initialises a new instance of the <see cref="Input"/> class.
		/// </summary>
		/// <param name="name">The name of this Variable.</param>
		/// <param name="value">The value of this Variable.</param>
		/// <param name="lifespan">The lifespan of this Variable.</param>
		public Variable(string name, object value, Lifespan lifespan) : base(name, value)
		{
			Lifespan = lifespan;
		}

		/// <summary>
		/// Duplicate and return this Variable with the new value.
		/// </summary>
		public override Input WithValue(object value) => new Variable(Name, value, Lifespan);
	}
}