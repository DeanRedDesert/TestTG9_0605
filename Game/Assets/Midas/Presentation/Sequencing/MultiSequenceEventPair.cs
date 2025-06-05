using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Connects an event pair to multiple sequences (provided the sequences share event IDs)
	/// </summary>
	/// <remarks>
	/// The way this works is the sequence activators are started when eventA fires, and the
	/// sequence waits in eventB until the component indicates that it is finished.
	/// </remarks>
	[Serializable]
	public sealed class MultiSequenceEventPair : IEquatable<MultiSequenceEventPair>
	{
		#region Inspector Fields

		[SerializeField]
		private List<string> sequenceNames;

		[SerializeField]
		private string eventNameA;

		[SerializeField]
		private string eventNameB;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the names of all sequences.
		/// </summary>
		public IReadOnlyList<string> SequenceNames => sequenceNames;

		/// <summary>
		/// Details about the first event.
		/// </summary>
		public string EventA => eventNameA;

		/// <summary>
		/// Details about the second event.
		/// </summary>
		public string EventB => eventNameB;

		#endregion

		#region Methods

		public MultiSequenceEventPair(List<string> sequenceNames, string eventA, string eventB)
		{
			this.sequenceNames = sequenceNames;
			eventNameA = eventA;
			eventNameB = eventB;
		}

		public override string ToString() => sequenceNames != null ? $"{string.Join(",", sequenceNames)} [{eventNameA} ... {eventNameB}]" : "undefined";

		#region Equality

		[SuppressMessage("csharpsquid", "S2328")]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (sequenceNames != null ? sequenceNames.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (eventNameA != null ? eventNameA.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (eventNameB != null ? eventNameB.GetHashCode() : 0);
				return hashCode;
			}
		}

		public bool Equals(MultiSequenceEventPair other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(sequenceNames, other.sequenceNames) && eventNameA == other.eventNameA && eventNameB == other.eventNameB;
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is MultiSequenceEventPair other && Equals(other);

		#endregion

		#endregion
	}
}