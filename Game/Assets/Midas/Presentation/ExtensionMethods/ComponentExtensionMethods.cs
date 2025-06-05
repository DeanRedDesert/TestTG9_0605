using System.Linq;
using UnityEngine;

namespace Midas.Presentation.ExtensionMethods
{
	public static class ComponentExtensionMethods
	{
		public static string GetPath(this Component component)
		{
			return string.Join("/", component.GetComponentsInParent<Transform>(true).Select(t => t.name).Reverse());
		}
	}
}