using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	[Serializable]
	public sealed class SoundId : IEquatable<SoundId>
	{
		[SerializeField]
		private string id;

		[SerializeField]
		private string definitionsName;

		public string Id => id;

		public string DefinitionName => definitionsName;

		public bool IsValid => id.Length > 0 && definitionsName.Length > 0;

		public SoundId(string id = "", string definitionName = "")
		{
			this.id = id;
			definitionsName = definitionName;
		}

		public override string ToString()
		{
			return IsValid ? $"{definitionsName}/{id}" : "";
		}

		#region Equality

		[SuppressMessage("csharpsquid", "S2328")]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked
			{
				return ((id != null ? id.GetHashCode() : 0) * 397) ^ (definitionsName != null ? definitionsName.GetHashCode() : 0);
			}
		}

		public bool Equals(SoundId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.id == id && other.definitionsName == definitionsName;
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is SoundId other && Equals(other);

		#endregion
	}
}