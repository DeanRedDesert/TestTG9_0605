using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Midas.Presentation.ButtonHandling
{
	[Serializable, ButtonFunctions]
	public class ButtonFunction : IEquatable<ButtonFunction>
	{
		public const int UndefinedButtonFunctionId = 0;

		[SerializeField]
		private int id;

		[SerializeField]
		private string name;

		public static ButtonFunction Undefined { get; } = Create(UndefinedButtonFunctionId);

		public int Id => id;
		public string Name => name;

		public ButtonFunction(ButtonFunction function) : this(function.id, function.name) { }

		private ButtonFunction(int id, string name)
		{
			this.id = id;
			this.name = string.IsNullOrEmpty(name) ? ButtonHelpers.GetButtonFunctionNameOfId(id) : name;
		}

		public static ButtonFunction Create(ButtonFunctions id, [CallerMemberName] string name = null)
		{
			return new ButtonFunction((int)id, name);
		}

		public override string ToString() => $"{id}, {name}";

		#region Equality (Note: only uses Id)

		public bool Equals(ButtonFunction other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == GetType() && Equals((ButtonFunction)obj);
		}

		public override int GetHashCode()
		{
			return Id;
		}

		#endregion
	}
}