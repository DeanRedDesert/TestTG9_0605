using System.Collections.Generic;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	public interface ICyclesModifier : IToString
	{
		/// <summary>
		/// Modify and return the Cycles object based on whatever logic is required when the exit is taken.
		/// </summary>
		Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage);
	}

	public static class CyclesModifierExtensionMethods
	{
		public static IReadOnlyList<DesiredExit> ToDesiredExits(this ICyclesModifier exitBehaviour, string exitName)
			=> new[] { new DesiredExit(exitName, exitBehaviour) };

		public static IReadOnlyList<DesiredExit> ToDesiredExits(this IReadOnlyList<ICyclesModifier> exitBehaviours, string exitName)
		{
			var result = new DesiredExit[exitBehaviours.Count];

			for (var i = 0; i < exitBehaviours.Count; i++)
				result[i] = new DesiredExit(exitName, exitBehaviours[i]);

			return result;
		}
	}
}