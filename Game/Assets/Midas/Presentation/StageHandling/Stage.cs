using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Midas.Presentation.StageHandling
{
	/// <summary>
	///     class wraps an integer for type safety
	/// </summary>
	[Serializable]
	public sealed class Stage : IEquatable<Stage>
	{
		[SerializeField]
		private int id = -1;

		/// <summary>
		///     This field is only used to make debugging easy
		/// </summary>
		[SerializeField, HideInInspector]
		private string name = "Undefined";

		/// <summary>
		///     integer id of the stage
		/// </summary>
		public int Id => id;

		/// <summary>
		///     name of the stage, should only be use for debugging or in editor
		/// </summary>
		public string Name => name;

		public static Stage Undefined { get; } = new Stage();

		public Stage() { }

		public Stage(int id, [CallerMemberName] string name = "")
		{
			this.id = id;
			this.name = name;
		}

		public override string ToString()
		{
			return $"'{name}', {id}";
		}

		#region Equality

		[SuppressMessage("csharpsquid", "S2328")]
		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked
			{
				return (id * 397) ^ (name != null ? name.GetHashCode() : 0);
			}
		}

		public bool Equals(Stage other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return id == other.id && name == other.name;
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Stage other && Equals(other);

		public static bool operator ==(Stage obj1, Stage obj2)
		{
			if (obj1 is null)
				return obj2 is null;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(Stage obj1, Stage obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}