using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public sealed class RequiredIntensity : IEquatable<RequiredIntensity>
	{
		public static readonly RequiredIntensity NoIntensity = new RequiredIntensity(-1);

		[SerializeField]
		private int intensity;

		public RequiredIntensity(int intensity)
		{
			this.intensity = intensity;
		}

		public bool HasIntensity(int value)
		{
			return intensity == value || intensity == -1;
		}

		#region Equality

		[SuppressMessage("csharpsquid", "S2328")]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			return intensity;
		}

		public override string ToString()
		{
			return intensity == -1 ? "all" : $"{intensity}";
		}

		public bool Equals(RequiredIntensity other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return intensity == other.intensity;
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is RequiredIntensity other && Equals(other);

		#endregion
	}
}