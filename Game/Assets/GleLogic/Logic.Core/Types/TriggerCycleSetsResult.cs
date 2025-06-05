using System.Collections.Generic;
using Logic.Core.Types.Exits;
using Logic.Core.Utility;

// ReSharper disable UnusedAutoPropertyAccessor.Global - Required for serialisation
// ReSharper disable MemberCanBePrivate.Global - Required for serialisation

namespace Logic.Core.Types
{
	/// <summary>
	/// The results of a trigger evaluation see function WinCheckFunctions.CheckForPrizeTriggers.
	/// </summary>
	// ReSharper disable once UnusedType.Global
	public sealed class TriggerCycleSetsResult
	{
		private readonly IReadOnlyList<CellPrizeMapping> prizeMappings;

		/// <summary>
		/// Get the triggers that occurred.
		/// </summary>
		public IReadOnlyList<ICyclesModifier> Triggers { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="triggers">The triggers that occurred.</param>
		/// <param name="mappings">The prize mappings for the triggers.</param>
		public TriggerCycleSetsResult(IReadOnlyList<ICyclesModifier> triggers, IReadOnlyList<CellPrizeMapping> mappings)
		{
			Triggers = triggers;
			prizeMappings = mappings;
		}

		/// <summary>
		/// The prize mappings for the triggers.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used by presentation
		public IReadOnlyList<CellPrizeMapping> GetPrizeMappings()
		{
			return prizeMappings;
		}
	}
}