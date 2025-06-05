using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	/// <summary>
	/// Stores the generated desired exit results.
	/// </summary>
	public sealed class DesiredExit : IToString
	{
		#region Properties

		/// <summary>
		/// The exit name that has been requested.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The cycles modifier to run when the exit is taken.
		/// </summary>
		public ICyclesModifier CyclesModifier { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">The name of the requested stage exit.</param>
		/// <param name="cyclesModifier">The cycles modifier to run when the exit is taken.</param>
		public DesiredExit(string name, ICyclesModifier cyclesModifier)
		{
			Name = name;
			CyclesModifier = cyclesModifier;
		}

		#endregion

		#region IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => new NotSupported();

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<DesiredExit> l)
			{
				return l.Select(d => $"{d.Name} - {d.CyclesModifier.ToStringOrThrow("SL")}")
					.Join()
					.ToSuccess();
			}

			return new NotSupported();
		}

		#endregion
	}
}