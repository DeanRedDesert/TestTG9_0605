using System;
using System.Reflection;
using UnityEngine;

namespace Midas.Presentation.Data.PropertyReference
{
	public sealed class PropertyPathAttribute : PropertyAttribute
	{
		private readonly string requiredTypeProperty;

		public PropertyPathAttribute(string requiredTypeProperty = null)
		{
			this.requiredTypeProperty = requiredTypeProperty;
		}

		public Type GetRequiredType(object o)
		{
			if (requiredTypeProperty == null)
				return typeof(object);

			var t = o.GetType();
			var prop = t.GetProperty(requiredTypeProperty, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (prop != null)
				return (Type)prop.GetValue(o);

			prop = t.GetProperty(requiredTypeProperty, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			if (prop == null)
				throw new InvalidOperationException($"Unable to find property {requiredTypeProperty}");

			return (Type)prop.GetValue(null);
		}
	}
}