using System;

namespace Midas.Presentation.Data
{
	public interface IStatusProperty
	{
		string Name { get; }
		Type PropertyType { get; }
		object Value { get; }
		bool LogChangedValue { get; set; }

		bool ApplyModification();
		void SendChangedNotifications();
	}

	public delegate void StatusPropertyChangedHandler<in T>(IStatusProperty statusProperty, T newValue, T oldValue);

	public interface IStatusProperty<T> : IStatusProperty
	{
		new T Value { get; set; }

		/// <summary>
		/// Update the value and return whether it actually changed.
		/// </summary>
		/// <remarks>Call only if you really want the return value, otherwise use .Value=value instead</remarks>
		/// <returns>true if modified.</returns>
		bool SetValue(T newValue);
	}
}