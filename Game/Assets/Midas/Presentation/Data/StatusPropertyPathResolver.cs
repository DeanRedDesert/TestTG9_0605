using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Presentation.Data.PropertyReference;

namespace Midas.Presentation.Data
{
	// ReSharper disable once UnusedType.Global - Instantiated via reflection
	public sealed class StatusPropertyPathResolver : IPropertyPathResolver
	{
		private sealed class Binding : PropertyBinding
		{
			private readonly StatusBlock rootObject;
			private readonly string propName;

			public Binding(StatusBlock rootObject, string path)
				: base(rootObject, path)
			{
				this.rootObject = rootObject;
				var firstDot = path.IndexOf('.');
				propName = firstDot == -1
					? path
					: path.Substring(0, firstDot);
			}

			private void OnPropertyChanged(StatusBlock sender, string propertyname)
			{
				RaiseValueChanged();
			}

			protected override void RegisterChangeEvent() => rootObject.AddPropertyChangedHandler(propName, OnPropertyChanged);
			protected override void UnregisterChangeEvent() => rootObject.RemovePropertyChangedHandler(propName, OnPropertyChanged);
		}

		public string Name => "StatusDatabase";

		public IReadOnlyList<(string Name, Type PropertyType)> CollectProperties()
		{
			return StatusDatabase.StatusBlocksInstance == null
				? Array.Empty<(string, Type)>()
				: CollectPropertiesRecursive(StatusDatabase.StatusBlocksInstance?.StatusBlocks, "").ToArray();

			IEnumerable<(string Name, Type PropertyType)> CollectPropertiesRecursive(IReadOnlyList<StatusBlock> statusBlocks, string prefix)
			{
				return statusBlocks
					.SelectMany(s => s.Properties.Select(sp => ($"{prefix}{s.Name}.{sp.Name}", sp.PropertyType)))
					.Concat(statusBlocks.OfType<StatusBlockCompound>().SelectMany(sc => CollectPropertiesRecursive(sc.StatusBlocks, $"{prefix}{sc.Name}.")));
			}
		}

		public PropertyBinding Resolve(string path)
		{
			var firstDot = path.IndexOf('.');
			var statusBlockName = path.Substring(0, firstDot);

			var instance = StatusDatabase.StatusBlocksInstance.StatusBlocks.FirstOrDefault(i => i.Name.Equals(statusBlockName));
			if (instance == null)
			{
				throw new ArgumentException($"Unable to find status item at path '{path}'");
			}

			path = path.Substring(firstDot + 1);

			while (instance is StatusBlockCompound sbc)
			{
				firstDot = path.IndexOf('.');
				statusBlockName = path.Substring(0, firstDot);
				var nextInstance = sbc.StatusBlocks.FirstOrDefault(i => i.Name.Equals(statusBlockName));

				if (nextInstance == default)
					break;

				instance = nextInstance;
				path = path.Substring(firstDot + 1);
			}

			return new Binding(instance, path);
		}
	}
}