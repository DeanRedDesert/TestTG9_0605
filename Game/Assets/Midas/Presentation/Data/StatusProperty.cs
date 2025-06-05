using System;
using System.Collections.Generic;

namespace Midas.Presentation.Data
{
	/// <summary>
	/// Status property implementation.
	/// </summary>
	/// <typeparam name="T">The property type.</typeparam>
	public sealed class StatusProperty<T> : IStatusProperty<T>
	{
		private T value;
		private T lastValueWhenNotificationWasSent;
		private readonly StatusPropertyChangedHandler<T> changed;
		private readonly IEqualityComparer<T> comparer;
		private bool notificationRequired;
		private bool modified;

		public StatusProperty(string name, T value, StatusPropertyChangedHandler<T> changed, IEqualityComparer<T> comparer = null)
		{
			Name = name;
			this.value = lastValueWhenNotificationWasSent = value;
			this.changed = changed;
			this.comparer = comparer ?? EqualityComparer<T>.Default;
			notificationRequired = true;
		}

		#region IStatusProperty implementation

		public string Name { get; }

		Type IStatusProperty.PropertyType => typeof(T);

		object IStatusProperty.Value => value;

		public bool LogChangedValue { get; set; } = false;

		public bool ApplyModification()
		{
			var result = false;
			if (modified && (notificationRequired || !comparer.Equals(value, lastValueWhenNotificationWasSent)))
			{
				notificationRequired = true;
				result = true;
			}

			modified = false;
			return result;
		}

		public void SendChangedNotifications()
		{
			if (notificationRequired)
			{
				notificationRequired = false;

#if DEBUG
				if (LogChangedValue)
					Log.Instance.DebugFormat("Value '{0}' changed from '{1}' to '{2}'", Name, lastValueWhenNotificationWasSent, value);
#endif

				var tempLast = value;
				changed.Invoke(this, value, lastValueWhenNotificationWasSent);
				lastValueWhenNotificationWasSent = tempLast;
			}
		}

		#endregion

		#region IStatusProperty<T> implementation

		public T Value
		{
			get => value;
			set
			{
				this.value = value;
				modified = true;
			}
		}

		public bool SetValue(T newValue)
		{
			value = newValue;
			modified = true;
			return !comparer.Equals(this.value, lastValueWhenNotificationWasSent);
		}

		#endregion
	}
}