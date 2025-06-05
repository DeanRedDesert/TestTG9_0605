using Midas.Core.General;

namespace Midas.Core
{
	/// <summary>
	/// This class contains the data for a progressive level.
	/// </summary>
	public sealed class ProgressiveLevel
	{
		#region Properties

		/// <summary>
		/// The id of the progressive level.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// The display name of the progressive level.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The increment/contribution of the progressive level.
		/// </summary>
		public double IncrementPercentage { get; }

		/// <summary>
		/// The hidden increment/contribution of the progressive level.
		/// </summary>
		public double HiddenIncrementPercentage { get; }

		/// <summary>
		/// The startup value of the progressive level.
		/// </summary>
		public Money Startup { get; }

		/// <summary>
		/// The ceiling of the progressive level.
		/// </summary>
		public Money Ceiling { get; }

		/// <summary>
		/// Is the progressive level SAP or LP.
		/// </summary>
		public bool IsStandalone { get; }

		public bool IsTriggered { get; }

		/// <summary>
		/// The RTP as calculated by the foundation.
		/// </summary>
		public double Rtp { get; }

		/// <summary>
		/// The triggered probability as defined by the game.
		/// </summary>
		public double TriggerProbability { get; }

		#endregion

		#region Constructor

		public ProgressiveLevel(string id, string name, double incrementPercentage, double hiddenIncrementPercentage, Money startup, Money ceiling, bool isStandalone, bool isTriggered, double rtp, double triggerProbability)
		{
			Id = id;
			Name = name;
			IncrementPercentage = incrementPercentage;
			HiddenIncrementPercentage = hiddenIncrementPercentage;
			Startup = startup;
			Ceiling = ceiling;
			IsStandalone = isStandalone;
			IsTriggered = isTriggered;
			Rtp = rtp;
			TriggerProbability = triggerProbability;
		}

		#endregion
	}
}