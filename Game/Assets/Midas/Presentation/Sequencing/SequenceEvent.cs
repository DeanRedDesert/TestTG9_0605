using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Used in components to reference a particular event inside a sequence.
	/// </summary>
	[Serializable]
	public class SequenceEvent : IEquatable<SequenceEvent>
	{
		#region Inspector Fields

		[SerializeField]
		private string sequenceName;

		[SerializeField]
		private string eventName;

		#endregion

		public string SequenceName => sequenceName;
		public string Event => eventName;

		public SequenceEvent(string sequenceName, string eventName)
		{
			this.sequenceName = sequenceName;
			this.eventName = eventName;
		}

		public override string ToString()
		{
			return sequenceName != null ? $"{sequenceName} [{eventName}]" : "undefined";
		}

		#region Equality

		[SuppressMessage("csharpsquid", "S2328")]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (sequenceName != null ? sequenceName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (eventName != null ? eventName.GetHashCode() : 0);
				return hashCode;
			}
		}

		public virtual bool Equals(SequenceEvent other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return sequenceName == other.sequenceName && eventName == other.eventName;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == GetType() && Equals((SequenceEvent)obj);
		}

		#endregion
	}
}