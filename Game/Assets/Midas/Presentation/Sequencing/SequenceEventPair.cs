using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	[Serializable]
	public sealed class SequenceEventPair : IEquatable<SequenceEventPair>
	{
		[SerializeField]
		private string sequenceName;

		[SerializeField]
		private string eventNameA;

		[SerializeField]
		private string eventNameB;

		public string SequenceName => sequenceName;
		public string EventA => eventNameA;
		public string EventB => eventNameB;

		public SequenceEventPair(string sequenceName, string eventA, string eventB)
		{
			this.sequenceName = sequenceName;
			eventNameA = eventA;
			eventNameB = eventB;
		}

		public override string ToString()
		{
			return sequenceName != null ? $"{sequenceName} [{eventNameA}] ... [{eventNameB}]" : "undefined";
		}

		#region Equality

		[SuppressMessage("csharpsquid", "S2328")]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = sequenceName != null ? sequenceName.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (eventNameA != null ? eventNameA.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (eventNameB != null ? eventNameB.GetHashCode() : 0);
				return hashCode;
			}
		}

		public bool Equals(SequenceEventPair other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return sequenceName == other.sequenceName &&
				eventNameA == other.eventNameA &&
				eventNameB == other.eventNameB;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == GetType() && Equals((SequenceEventPair)obj);
		}

		#endregion
	}
}