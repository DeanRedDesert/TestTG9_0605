using System;

namespace Midas.Presentation.ButtonHandling
{
	public readonly struct PhysicalButton : IEquatable<PhysicalButton>
	{
		public static PhysicalButton Undefined = new PhysicalButton(-1);

		public int Id { get; }

		public PhysicalButton(int id = -1)
		{
			Id = id;
		}

		public bool Equals(PhysicalButton other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return obj is PhysicalButton other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Id;
		}

		public override string ToString()
		{
			return Id.ToString();
		}

		public static bool operator ==(PhysicalButton lhs, PhysicalButton rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(PhysicalButton lhs, PhysicalButton rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}