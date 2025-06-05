using System;

namespace Midas.Core.ExtensionMethods
{
	public static class NullableExtensionMethods
	{
		/// <summary>
		/// A nullable equals method using IEquatable that does not cause boxing.
		/// </summary>
		public static bool NullableEquals<T>(this T? self, T? other) where T : struct, IEquatable<T>
		{
			return self.HasValue == other.HasValue && (!other.HasValue || self.Value.Equals(other.Value));
		}
	}
}