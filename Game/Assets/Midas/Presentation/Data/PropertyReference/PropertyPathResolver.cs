using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;

namespace Midas.Presentation.Data.PropertyReference
{
	public static class PropertyPathResolver
	{
		private static IPropertyPathResolver[] resolvers;

		private static IPropertyPathResolver[] GetResolvers()
		{
			return resolvers ??= ReflectionUtil.GetInterfaceImplementations<IPropertyPathResolver>().Select(i => (IPropertyPathResolver)Activator.CreateInstance(i)).ToArray();
		}

		public static IReadOnlyList<(string Name, Type PropertyType)> CollectProperties(Type requiredType)
		{
			return GetResolvers().SelectMany(GetProps).ToArray();

			IEnumerable<(string, Type)> GetProps(IPropertyPathResolver pf)
			{
				return pf.CollectProperties()
					.SelectMany(v => CollectPropertiesRecursive(v.PropertyType, $"{pf.Name}.{v.Name}"))
					.Where(v => requiredType.IsAssignableFrom(v.PropertyType)) // Only select properties of specified type
					.OrderBy(v => v.Name);
			}
		}

		public static PropertyBinding ResolveProperty(string path)
		{
			var firstDot = path.IndexOf('.');
			var resolver = GetResolvers().FirstOrDefault(r => r.Name == path.Substring(0, firstDot));

			if (resolver == null)
				throw new ArgumentException($"Unable to find property path resolver for {path}");

			// path e.g: StatusDatabase.BankStatus.BankMeter
			var propertyPath = path.Substring(firstDot + 1);

			return resolver.Resolve(propertyPath);
		}

		private static IEnumerable<(string Name, Type PropertyType)> CollectPropertiesRecursive(Type propType, string propName)
		{
			yield return (propName, propType);

			// This does not recurse into the properties of Nullable<T> or anything that is enumerable.

			if (!typeof(IEnumerable).IsAssignableFrom(propType) &&
				!ReflectionUtil.IsNullableType(propType))
			{
				foreach (var valueTuple in propType.GetProperties()
							.Where(p => p.PropertyType != propType && p.GetIndexParameters().Length == 0) // Try to remove recursion and indexer properties
							.SelectMany(p => CollectPropertiesRecursive(p.PropertyType, $"{propName}.{p.Name}")) // Collect child properties
						)
				{
					yield return valueTuple;
				}
			}
		}
	}
}