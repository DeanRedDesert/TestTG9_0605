using System;
using System.Collections.Generic;
using System.Linq;

namespace Midas.Presentation.Data
{
	/// <summary>
	/// Handles a collection of status blocks.
	/// </summary>
	public class StatusBlockCompound : StatusBlock
	{
		private int anyPropertyChangedRegistrationCount;
		private readonly List<StatusBlock> statusBlocks = new List<StatusBlock>();
		private bool isInitialised;

		/// <summary>
		/// Gets the status blocks contained in this compound.
		/// </summary>
		public IReadOnlyList<StatusBlock> StatusBlocks => statusBlocks;

		/// <summary>
		/// Constructor.
		/// </summary>
		public StatusBlockCompound(string name)
			: base(name)
		{
		}

		public override void Init()
		{
			base.Init();
			foreach (var statusBlock in statusBlocks)
				statusBlock.Init();

			isInitialised = true;
		}

		public override void DeInit()
		{
			foreach (var statusBlock in statusBlocks)
				statusBlock.DeInit();

			isInitialised = false;
			base.DeInit();
		}

		/// <summary>
		/// Adds a child status block. Intended to only be called from derived class or StatusDatabase (treat it as if it was protected).
		/// </summary>
		public void AddStatusBlock(StatusBlock statusBlock)
		{
			if (statusBlock != null)
			{
				if (isInitialised)
					statusBlock.Init();

				if (anyPropertyChangedRegistrationCount != 0)
				{
					statusBlock.AnyPropertyChanged += OnPropertyChanged;
				}

				statusBlocks.Add(statusBlock);
				statusBlock.Reset();
			}
		}

		/// <summary>
		/// Remove a child status block. Intended to only be called from derived class or StatusDatabase (treat it as if it was protected).
		/// </summary>
		public void RemoveStatusBlock(StatusBlock statusBlock)
		{
			if (statusBlock != null)
			{
				if (anyPropertyChangedRegistrationCount != 0)
				{
					statusBlock.AnyPropertyChanged -= OnPropertyChanged;
				}

				statusBlocks.Remove(statusBlock);
				statusBlock.DeInit();
			}
		}

		public T QueryStatusBlock<T>() where T : StatusBlock
		{
			foreach (var sb in statusBlocks)
			{
				if (sb is T result)
					return result;

				if (sb is StatusBlockCompound comp)
				{
					result = comp.QueryStatusBlock<T>();
					if (result != null)
						return result;
				}
			}

			return null;
		}

		/// <summary>
		/// Pulls all status properties out of the compound.
		/// </summary>
		public IEnumerable<(string path, IStatusProperty property)> GetAllStatusProperties()
		{
			var items = new List<(string, IStatusProperty)>();
			AddStatusItemValuesRecursive(this, "", items);
			return items;
		}

		private void RegisterAnyPropertyChanged()
		{
			if (anyPropertyChangedRegistrationCount == 0)
				statusBlocks.ForEach(s => s.AnyPropertyChanged += OnPropertyChanged);

			anyPropertyChangedRegistrationCount++;
		}

		private void UnregisterAnyPropertyChanged()
		{
			anyPropertyChangedRegistrationCount--;

			if (anyPropertyChangedRegistrationCount == 0)
				statusBlocks.ForEach(s => s.AnyPropertyChanged -= OnPropertyChanged);
		}

		private static void AddStatusItemValuesRecursive(StatusBlockCompound statusBlockCompound, string path, ICollection<(string Path, IStatusProperty Value)> items)
		{
			foreach (var statusItem in statusBlockCompound.StatusBlocks)
			{
				var itemPath = path + "." + statusItem.Name;
				foreach (var itemName in statusItem.PropertyNames)
					items.Add((itemPath + "." + itemName, statusItem.GetPropertyValue(itemName)));

				if (statusItem is StatusBlockCompound compound)
					AddStatusItemValuesRecursive(compound, itemPath, items);
			}
		}

		private void OnPropertyChanged(StatusBlock sender, string propertyName)
		{
			RaiseAnyPropertyChanged($"{sender.Name}.{propertyName}");
		}

		#region StatusBlock overrides

		public override event PropertyChangedHandler AnyPropertyChanged
		{
			add
			{
				RegisterAnyPropertyChanged();
				base.AnyPropertyChanged += value;
			}
			remove
			{
				base.AnyPropertyChanged -= value;
				UnregisterAnyPropertyChanged();
			}
		}

		public override bool ApplyModifications()
		{
			var result = base.ApplyModifications();

			foreach (var statusBlock in statusBlocks)
				result |= statusBlock.ApplyModifications();

			return result;
		}

		public override void SendChangedNotifications()
		{
			base.SendChangedNotifications();

			foreach (var statusItem in statusBlocks)
				statusItem.SendChangedNotifications();
		}

		public override StatusBlock AddPropertyChangedHandlerRecursive(IReadOnlyList<string> path, PropertyChangedHandler handler)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			switch (path.Count)
			{
				case 0:
					throw new ArgumentException("path must have at least one element");
				case 1:
					AddPropertyChangedHandler(path[0], handler);
					return this;
			}

			var compoundName = path[0];
			var remainingPath = path.Skip(1).ToList();
			var nextLevel = StatusBlocks.SingleOrDefault(i => i.Name == path[0]);

			if (nextLevel == null)
				throw new Exception($"Unable to find compound status block '{compoundName}' inside '{Name}'.");

			return nextLevel.AddPropertyChangedHandlerRecursive(remainingPath, handler);
		}

		public override StatusBlock AddPropertyChangedHandlerRecursive<T>(IReadOnlyList<string> path, PropertyChangedHandler<T> handler)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			switch (path.Count)
			{
				case 0:
					throw new ArgumentException("path must have at least one element");
				case 1:
					AddPropertyChangedHandler(path[0], handler);
					return this;
			}

			var compoundName = path[0];
			var remainingPath = path.Skip(1).ToList();
			var nextLevel = StatusBlocks.SingleOrDefault(i => i.Name == path[0]);

			if (nextLevel == null)
				throw new Exception($"Unable to find compound status block '{compoundName}' inside '{Name}'.");

			return nextLevel.AddPropertyChangedHandlerRecursive(remainingPath, handler);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			foreach (var statusBlock in statusBlocks)
				statusBlock.Reset();
		}

		public override void ResetForNewGame()
		{
			foreach (var block in statusBlocks)
				block.ResetForNewGame();
		}

		#endregion
	}
}