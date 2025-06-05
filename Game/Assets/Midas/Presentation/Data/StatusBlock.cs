using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Midas.Core.General;

namespace Midas.Presentation.Data
{
	public delegate void PropertyChangedHandler<in T>(StatusBlock sender, string propertyName, T newValue, T oldValue);

	public delegate void PropertyChangedHandler(StatusBlock sender, string propertyName);

	/// <summary>
	/// Base for status blocks.
	/// </summary>
	public abstract class StatusBlock
	{
		#region Types

		private interface IPropertySubscriber
		{
#if DEBUG
			void CheckHangingPropertyChangedHandlers(string propertyName);
#endif
		}

		private sealed class PropertySubscriber<T> : IPropertySubscriber
		{
			public void Invoke(StatusBlock sender, string name, T newValue, T oldValue)
			{
				Handler?.Invoke(sender, name, newValue, oldValue);
			}

			public bool HasHandlers => Handler != null;

			public event PropertyChangedHandler<T> Handler;

#if DEBUG
			public void CheckHangingPropertyChangedHandlers(string propertyName)
			{
				if (Handler == null)
					return;

				foreach (var propertyHandler in Handler.GetInvocationList())
					Log.Instance.Fatal($"Property changed handlers is not unregistered from '{propertyName} '{propertyHandler.Target?.GetType().FullName}.{propertyHandler.Method.Name}'");
			}
#endif
		}

		private sealed class PropertySubscriber
		{
			public void Invoke(StatusBlock sender, string name)
			{
				Handler?.Invoke(sender, name);
			}

			public bool HasHandlers => Handler != null;

			public event PropertyChangedHandler Handler;

			[Conditional("DEBUG")]
			public void CheckHangingPropertyChangedHandlers(string propertyName)
			{
				if (Handler == null)
					return;

				foreach (var propertyHandler in Handler.GetInvocationList())
					Log.Instance.Fatal($"Property changed handlers is not unregistered from '{propertyName} '{propertyHandler.Target?.GetType().FullName}.{propertyHandler.Method.Name}'");
			}
		}

		#endregion

		#region Fields

		private readonly Dictionary<string, PropertySubscriber> propertyHandlers = new Dictionary<string, PropertySubscriber>();
		private readonly Dictionary<string, PropertySubscriber> oneShotPropertyHandlers = new Dictionary<string, PropertySubscriber>();
		private readonly Dictionary<string, IPropertySubscriber> propertyWithValueHandlers = new Dictionary<string, IPropertySubscriber>();
		private readonly List<IStatusProperty> properties = new List<IStatusProperty>();
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		#endregion

		#region Events

		/// <summary>
		/// Raised when any property changes.
		/// </summary>
		public virtual event PropertyChangedHandler AnyPropertyChanged;

		#endregion

		#region Properties

		/// <summary>
		/// The name of the status block.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a list of the property names in the status block.
		/// </summary>
		public IEnumerable<string> PropertyNames => properties.Select(item => item.Name);

		/// <summary>
		/// Gets the properties contained in the staus block.
		/// </summary>
		public IReadOnlyList<IStatusProperty> Properties => properties;

		#endregion

		#region Methods

		/// <summary>
		/// Initialise the property block.
		/// </summary>
		public virtual void Init()
		{
			RegisterForEvents(autoUnregisterHelper);
		}

		/// <summary>
		/// Deinit the property block.
		/// </summary>
		public virtual void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			CheckHangingPropertyChangedHandlers();
		}

		/// <summary>
		/// Apply all modifications to the status block.
		/// </summary>
		/// <returns>True if any property was modified.</returns>
		public virtual bool ApplyModifications()
		{
			var result = false;

			foreach (var item in properties)
				result |= item.ApplyModification();

			return result;
		}

		/// <summary>
		/// Sends change notifications for all changed properties.
		/// </summary>
		public virtual void SendChangedNotifications()
		{
			foreach (var item in properties)
				item.SendChangedNotifications();
		}

		public void AddPropertyChangedHandler<T>(string propertyName, PropertyChangedHandler<T> handler)
		{
			if (!propertyWithValueHandlers.TryGetValue(propertyName, out var subscriber))
			{
				subscriber = new PropertySubscriber<T>();
				propertyWithValueHandlers.Add(propertyName, subscriber);
			}

			if (subscriber is PropertySubscriber<T> typedSubscriber)
			{
				typedSubscriber.Handler += handler;
			}
			else
			{
				throw new ArgumentException($"Subscriber of '{propertyName}' are of wrong type: " +
					$"'{subscriber.GetType().GenericTypeArguments[0].FullName}' is not '{typeof(T).FullName}'");
			}
		}

		public void AddPropertyChangedHandler(string propertyName, PropertyChangedHandler handler)
		{
			if (!propertyHandlers.TryGetValue(propertyName, out var propertySubscriber))
			{
				propertySubscriber = new PropertySubscriber();
				propertyHandlers.Add(propertyName, propertySubscriber);
			}

			propertySubscriber.Handler += handler;
		}

		public void RemovePropertyChangedHandler<T>(string propertyName, PropertyChangedHandler<T> handler)
		{
			if (propertyWithValueHandlers.TryGetValue(propertyName, out var subscriber))
			{
				if (subscriber is PropertySubscriber<T> typedSubscriber)
				{
					typedSubscriber.Handler -= handler;
					if (!typedSubscriber.HasHandlers)
						propertyWithValueHandlers.Remove(propertyName);
				}
				else
				{
					throw new ArgumentException($"Subscriber of '{propertyName}' are of wrong type: " +
						$"'{subscriber.GetType().GenericTypeArguments[0].FullName}' is not '{typeof(T).FullName}'");
				}
			}
		}

		public IStatusProperty GetPropertyValue(string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				return null;
			}

			var property = properties.FirstOrDefault(x => x.Name == propertyName);
			return property;
		}

		public bool HasProperty(string propertyName)
		{
			return !string.IsNullOrWhiteSpace(propertyName) && properties.Any(x => x.Name == propertyName);
		}

		public void AddOneShotPropertyChangedHandler(string propertyName, PropertyChangedHandler handler)
		{
			if (!oneShotPropertyHandlers.TryGetValue(propertyName, out var propertySubscriber))
			{
				propertySubscriber = new PropertySubscriber();
				oneShotPropertyHandlers.Add(propertyName, propertySubscriber);
			}

			propertySubscriber.Handler += handler;
		}

		public void RemovePropertyChangedHandler(string propertyName, PropertyChangedHandler handler)
		{
			propertyHandlers[propertyName].Handler -= handler;
			if (!propertyHandlers[propertyName].HasHandlers)
			{
				propertyHandlers.Remove(propertyName);
			}
		}

		public virtual StatusBlock AddPropertyChangedHandlerRecursive(IReadOnlyList<string> path, PropertyChangedHandler handler)
		{
			if (path.Count != 1)
				throw new ArgumentException("path must have one element");

			AddPropertyChangedHandler(path[0], handler);
			return this;
		}

		public virtual StatusBlock AddPropertyChangedHandlerRecursive<T>(IReadOnlyList<string> path, PropertyChangedHandler<T> handler)
		{
			if (path.Count != 1)
				throw new ArgumentException("path must have one element");

			AddPropertyChangedHandler(path[0], handler);
			return this;
		}

		public void Reset()
		{
			properties.Clear();
			DoResetProperties();
		}

		public virtual void ResetForNewGame()
		{
		}

		protected void RaiseAnyPropertyChanged(string propertyName)
		{
			AnyPropertyChanged?.Invoke(this, propertyName);
		}

		protected StatusBlock(string name)
		{
			Name = name;
		}

		protected virtual void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
		}

		protected StatusProperty<T> AddProperty<T>(string name, T value, IEqualityComparer<T> comparer = null)
		{
			var item = new StatusProperty<T>(name, value, OnPropertyChanged, comparer);
			properties.Add(item);
			return item;
		}

		protected void OnPropertyChanged<T>(IStatusProperty status, T newValue, T oldValue)
		{
			RaisePropertyChangedWithValue(newValue, oldValue, status.Name);
			RaisePropertyChanged(status.Name);
			RaiseAnyPropertyChanged(status.Name);
		}

		private void RaisePropertyChangedWithValue<T>(T newValue, T oldValue, string propertyName)
		{
			if (propertyWithValueHandlers.TryGetValue(propertyName, out var subscriber))
			{
				if (subscriber is PropertySubscriber<T> subscriberT)
				{
					subscriberT.Invoke(this, propertyName, newValue, oldValue);
				}
				else
				{
					throw new ArgumentException($"Subscriber of '{propertyName}' are of wrong type: " +
						$"'{subscriber.GetType().GenericTypeArguments[0].FullName}' is not '{typeof(T).FullName}'. Subscriber is='{subscriber}'");
				}
			}
		}

		private void RaisePropertyChanged(string propertyName)
		{
			if (propertyHandlers.TryGetValue(propertyName, out var handler))
			{
				handler.Invoke(this, propertyName);
			}

			if (oneShotPropertyHandlers.TryGetValue(propertyName, out var oneShotHandler))
			{
				oneShotHandler.Invoke(this, propertyName);
				oneShotPropertyHandlers.Remove(propertyName);
			}
		}

		protected virtual void DoResetProperties()
		{
		}

		[Conditional("DEBUG")]
		private void CheckHangingPropertyChangedHandlers()
		{
			var anyPropertyChanged = AnyPropertyChanged?.GetInvocationList() ?? Array.Empty<Delegate>();
			for (var index = 0; index < anyPropertyChanged.Length; index++)
			{
				var anyPropertyChangedHandler = anyPropertyChanged[index];
				Log.Instance.Fatal($"Any Property changed handlers is not unregistered '{anyPropertyChangedHandler.Target?.GetType().FullName}.{anyPropertyChangedHandler.Method.Name}'");
			}

			foreach (var propertyHandler in propertyHandlers)
				propertyHandler.Value.CheckHangingPropertyChangedHandlers(propertyHandler.Key);

#if DEBUG
			foreach (var propertyWithValueHandler in propertyWithValueHandlers)
			{
				propertyWithValueHandler.Value.CheckHangingPropertyChangedHandlers(propertyWithValueHandler.Key);
			}
#endif
		}

		#endregion
	}
}