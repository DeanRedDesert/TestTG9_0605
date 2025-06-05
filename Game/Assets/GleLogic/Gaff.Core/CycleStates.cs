using System;

// ReSharper disable MemberCanBePrivate.Global

namespace Gaff.Core
{
	[Flags]
	public enum CycleStates
	{
		/// <summary>
		/// No states selected.
		/// </summary>
		// ReSharper disable once UnusedMember.Global
		None = 0,

		/// <summary>
		/// A base game trigger that is about to be played on the next cycle.
		/// </summary>
		InitialTrigger = 1,

		/// <summary>
		/// A trigger to a new stage and cycle id within the feature.
		/// </summary>
		Trigger = 2,

		/// <summary>
		/// A re trigger.
		/// </summary>
		ReTrigger = 4,

		/// <summary>
		/// A non base game without any triggers.
		/// </summary>
		FeatureGame = 8,

		/// <summary>
		/// Cycles contains a feature that is not being actively processed.
		/// </summary>
		PendingFeature = 16,

		/// <summary>
		/// This is the last game before returning to the base game after a feature.
		/// </summary>
		FeatureComplete = 32,

		/// <summary>
		/// A base game without any triggers.
		/// </summary>
		NonTriggeringBase = 64,

		/// <summary>
		/// Is about to move to a new feature.
		/// </summary>
		NextSubFeature = 128,

		/// <summary>
		/// Is about to move to a new feature and the current feature is complete.
		/// </summary>
		EndSubFeature = 256
	}
}