// Copyright (c) 2021 IGT

namespace Midas.Core.General
{
	//Usage: When need to store a ref as a member, which does not work otherwise (ref keyword is not allowed on fields)
	public sealed class ValueRef<T>
	{
		#region Public

		public ValueRef(T value)
		{
			Value = value;
		}

		public T Value { get; set; }

		#endregion
	}
}