using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Midas.Presentation.Data
{
	public static partial class StatusDatabase
	{
		#region Types

		public delegate void MultiplePropertyChangedHandler(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> changedProperties);

		private sealed class MultiplePropertyChangedSubscriber
		{
			private IReadOnlyList<(StatusBlock statusBlock, string propertyName)> properties;
			private MultiplePropertyChangedHandler handler;
			private readonly List<(StatusBlock statusBlock, string propertyName)> changedItems = new List<(StatusBlock statusBlock, string propertyName)>();

			public MultiplePropertyChangedSubscriber(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> properties, MultiplePropertyChangedHandler handler)
			{
				this.properties = properties;
				this.handler = handler;

				foreach (var (statusBlock, propertyName) in this.properties)
					statusBlock.AddPropertyChangedHandler(propertyName, OnPropertyChanged);
			}

			public bool Matches(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> matchItems, MultiplePropertyChangedHandler matchHandler)
			{
				return handler.Equals(matchHandler) && properties.SequenceEqual(matchItems);
			}

			public void Unregister()
			{
				foreach (var p in properties)
					p.statusBlock.RemovePropertyChangedHandler(p.propertyName, OnPropertyChanged);

				properties = null;
				handler = null;
				changedItems.Clear();
			}

			public void InvokeIfRequired()
			{
				if (changedItems.Count > 0)
				{
					handler?.Invoke(changedItems);
					changedItems.Clear();
				}
			}

			private void OnPropertyChanged(StatusBlock sender, string propertyName)
			{
				changedItems.Add((sender, propertyName));
			}

			[Conditional("DEBUG")]
			public void LogRegisteredHandlers()
			{
				Log.Instance.Fatal($"Property changed handlers is not unregistered from '{handler.Target?.GetType().FullName}.{handler.Method.Name}'");
			}
		}

		#endregion

		private static readonly List<MultiplePropertyChangedSubscriber> multiplePropertyChangedSubscriber = new List<MultiplePropertyChangedSubscriber>();

		public static void RegisterMultiplePropertyChangedHandler(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> items, MultiplePropertyChangedHandler handler)
		{
			foreach (var subscriber in multiplePropertyChangedSubscriber)
			{
				if (subscriber.Matches(items, handler))
				{
					Log.Instance.Error($"Can not register AnyPropertyChangedHandler: {handler.Target?.GetType().FullName}.{handler.Method.Name} is already registered");
					return;
				}
			}

			var newSubscriber = new MultiplePropertyChangedSubscriber(items, handler);
			multiplePropertyChangedSubscriber.Add(newSubscriber);
		}

		public static void UnregisterMultiplePropertyChangedHandler(IReadOnlyList<(StatusBlock statusBlock, string propertyName)> items, MultiplePropertyChangedHandler handler)
		{
			foreach (var subscriber in multiplePropertyChangedSubscriber)
			{
				if (subscriber.Matches(items, handler))
				{
					subscriber.Unregister();
					multiplePropertyChangedSubscriber.Remove(subscriber);
					return;
				}
			}

			Log.Instance.Error($"Can not unregister AnyPropertyChangedHandler: {handler.Target?.GetType().FullName}.{handler.Method.Name} as it is not registered");
		}

		[Conditional("DEBUG")]
		private static void CheckHangingMultiplePropertyChangeHandlers()
		{
			foreach (var subscriber in multiplePropertyChangedSubscriber)
			{
				subscriber.LogRegisteredHandlers();
			}
		}
	}
}