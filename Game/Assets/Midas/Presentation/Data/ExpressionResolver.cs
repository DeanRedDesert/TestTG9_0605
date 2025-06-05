using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Presentation.Data.PropertyReference;

namespace Midas.Presentation.Data
{
	// ReSharper disable once UnusedType.Global - instantiated via reflection
	public sealed class ExpressionResolver : IPropertyPathResolver
	{
		private sealed class Binding : PropertyBinding
		{
			private readonly PropertyInfo property;

			public Binding(PropertyInfo property, string path) : base(property.DeclaringType, path)
			{
				this.property = property;
			}

			protected override void RegisterChangeEvent()
			{
				ExpressionManager.RegisterChangeEvent(property, OnPropertyChanged);
			}

			protected override void UnregisterChangeEvent()
			{
				ExpressionManager.UnRegisterChangeEvent(property, OnPropertyChanged);
			}

			private void OnPropertyChanged()
			{
				RaiseValueChanged();
			}
		}

		public string Name => "Expressions";

		public IReadOnlyList<(string Name, Type PropertyType)> CollectProperties()
		{
			return ExpressionManager.GetExpressions()
				.SelectMany(kvp => kvp.Value.Select(v => ($"{kvp.Key}.{v.Name}", v.PropertyType)))
				.ToArray();
		}

		public PropertyBinding Resolve(string path)
		{
			var firstDot = path.IndexOf('.');
			var categoryName = path.Substring(0, firstDot);
			path = path.Substring(firstDot + 1);

			firstDot = path.IndexOf('.');
			var propName = firstDot == -1
				? path
				: path.Substring(0, firstDot);

			var property = ExpressionManager.GetProperty(categoryName, propName);
			if (property == null)
			{
				throw new ArgumentException($"Unable to find expression at path '{path}'");
			}

			return new Binding(property, path);
		}
	}
}