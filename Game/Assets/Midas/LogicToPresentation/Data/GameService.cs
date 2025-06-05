using System.Collections.Generic;
using System.Diagnostics;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data
{
	/// <summary>
	/// Implementation of a basic game service. Handles the handshake between the logic and presentation state machine.
	/// </summary>
	/// <typeparam name="T">The service type.</typeparam>
	public sealed class GameService<T> : IGameService
	{
		#region Nested Type: GameServiceVariable

		private sealed class GameServiceVariable : IGameServiceConsumer<T>
		{
			private readonly GameService<T> owner;
			private readonly List<IGameServiceEventListener<T>> onChangeListeners = new List<IGameServiceEventListener<T>>();

			public GameServiceVariable(GameService<T> owner)
			{
				this.owner = owner;
			}

			public void RegisterChangeListener(IGameServiceEventListener<T> onChangeHandler)
			{
				if (!onChangeListeners.Contains(onChangeHandler))
					onChangeListeners.Add(onChangeHandler);
			}

			public void UnregisterChangeListener(IGameServiceEventListener<T> onChangeHandler)
			{
				onChangeListeners.Remove(onChangeHandler);
			}

			public void Clear()
			{
				CheckHangingPropertyChangedHandlers();
				onChangeListeners.Clear();
			}

			public void DeliverChange(T value)
			{
				for (var i = 0; i < onChangeListeners.Count; i++)
					onChangeListeners[i].OnEventRaised(value);
			}

			[Conditional("DEBUG")]
			private void CheckHangingPropertyChangedHandlers()
			{
				foreach (var changeListener in onChangeListeners)
					Log.Instance.Fatal($"Change listener is not unregistered from '{owner.Name}' '{changeListener.GetType().FullName}'");
			}
		}

		#endregion

		#region Fields

		private readonly IEqualityComparer<T> comparer;
		private readonly HistorySnapshotType historySnapshotType;
		private readonly GameServiceVariable variable;

		private T presentationValue;

		#endregion

		#region Properties

		public T Value { get; private set; }

		public IGameServiceConsumer<T> Variable => variable;

		#endregion

		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="historySnapshotType">Indicates how the service needs to be saved in history.</param>
		/// <param name="comparer">An equality comparer if comparing complex types is required. If null then EqualityComparer{T}.Default will be used.</param>
		public GameService(HistorySnapshotType historySnapshotType, IEqualityComparer<T> comparer = null)
		{
			this.comparer = comparer ?? EqualityComparer<T>.Default;
			this.historySnapshotType = historySnapshotType;
			variable = new GameServiceVariable(this);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets a new value and immediately sends the change to the presentation.
		/// </summary>
		public bool SetValue(T newValue)
		{
			if (!comparer.Equals(Value, newValue))
			{
				Value = newValue;
				GameServices.ServiceChanged(this, newValue);
				return true;
			}

			return false;
		}

		#endregion

		#region Implementation of IGameService

		public string Name { get; private set; }

		void IGameService.Init(string name)
		{
			Name = name;
		}

		void IGameService.DeInit()
		{
			variable.Clear();
		}

		bool IGameService.IsHistoryRequired(HistorySnapshotType snapshotType)
		{
			return historySnapshotType == snapshotType;
		}

		/// <summary>
		/// Notify any listeners that a change has occurred. This happens on the presentation thread.
		/// </summary>
		void IGameService.DeliverChange(object notifyValue)
		{
			presentationValue = (T)notifyValue;
			variable.DeliverChange(presentationValue);
		}

		void IGameService.Refresh()
		{
			GameServices.ServiceChanged(this, Value);
		}

		/// <summary>
		/// Get the data to save into history.
		/// </summary>
		object IGameService.GetHistoryData(HistorySnapshotType snapshotType)
		{
			return Value;
		}

		/// <summary>
		/// Restore the game service from snapshot data.
		/// </summary>
		void IGameService.RestoreHistoryData(HistorySnapshotType snapshotType, object o)
		{
			SetValue((T)o);
		}

		#endregion
	}
}