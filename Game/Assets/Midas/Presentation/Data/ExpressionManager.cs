using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Midas.Core;

namespace Midas.Presentation.Data
{
	public static class ExpressionManager
	{
		private interface IPropMonitor
		{
			event Action ValueChanged;
			PropertyInfo Property { get; }
			bool NobodyListening { get; }
			void Check();
		}

		private sealed class PropMonitor<T> : IPropMonitor
		{
			public PropertyInfo Property { get; }
			private readonly Func<T> getValue;
			private T value;

			public event Action ValueChanged;

			public bool NobodyListening => ValueChanged == null;

			public PropMonitor(PropertyInfo property)
			{
				Property = property;
				var memExp = Expression.MakeMemberAccess(null, property);
				getValue = Expression.Lambda<Func<T>>(memExp).Compile();
			}

			public void Check()
			{
				var val = getValue();
				if (!EqualityComparer<T>.Default.Equals(value, val))
				{
					value = val;
					ValueChanged?.Invoke();
				}
			}
		}

		private static Dictionary<string, IReadOnlyList<PropertyInfo>> allExpressions;
		private static readonly List<IPropMonitor> monitors = new List<IPropMonitor>();

		public static void Update()
		{
			for (var index = 0; index < monitors.Count;)
			{
				var mon = monitors[index];
				mon.Check();
				if (mon.NobodyListening)
					monitors.RemoveAt(index);
				else
					index++;
			}
		}

		public static IReadOnlyDictionary<string, IReadOnlyList<PropertyInfo>> GetExpressions()
		{
			if (allExpressions == null)
			{
				var newExp = new Dictionary<string, IReadOnlyList<PropertyInfo>>();
				var types = ReflectionUtil.GetAllTypes();

				foreach (var type in types)
				{
					foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public))
					{
						var expAttr = prop.GetCustomAttribute<ExpressionAttribute>();
						if (expAttr == null)
							continue;

						if (!newExp.TryGetValue(expAttr.Category, out var catData))
						{
							catData = new List<PropertyInfo>();
							newExp.Add(expAttr.Category, catData);
						}

						((List<PropertyInfo>)catData).Add(prop);
					}
				}

				allExpressions = newExp;
			}

			return allExpressions;
		}

		public static PropertyInfo GetProperty(string categoryName, string propName)
		{
			return GetExpressions()[categoryName].FirstOrDefault(p => p.Name == propName);
		}

		public static void RegisterChangeEvent(PropertyInfo property, Action onPropertyChanged)
		{
			if (!property.GetMethod.IsStatic)
				throw new ArgumentException("Property must be static to work with the expression manager", nameof(property));

			var mon = monitors.FirstOrDefault(m => m.Property == property);
			if (mon == null)
			{
				var t = typeof(PropMonitor<>).MakeGenericType(property.PropertyType);
				mon = (IPropMonitor)Activator.CreateInstance(t, property);
				monitors.Add(mon);
			}

			mon.ValueChanged += onPropertyChanged;
		}

		public static void UnRegisterChangeEvent(PropertyInfo property, Action onPropertyChanged)
		{
			var mon = monitors.FirstOrDefault(m => m.Property == property);

			if (mon == null)
				return;

			mon.ValueChanged -= onPropertyChanged;
		}
	}
}