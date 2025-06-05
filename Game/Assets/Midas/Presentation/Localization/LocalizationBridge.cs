using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Midas.Presentation.Localization
{
	/// <summary>
	/// Dive into the internals of Localization to inject our own properly implemented tracked objects.
	/// </summary>
	internal static class LocalizationBridge
	{
		private static MethodInfo addressablesInterfaceSafeReleaseMethod;

		public static void AddressablesInterfaceSafeRelease(AsyncOperationHandle handle)
		{
			if (addressablesInterfaceSafeReleaseMethod == null)
			{
				var asm = typeof(LocaleIdentifier).Assembly;
				var t = asm.GetTypes().Where(t => t.IsClass && t.Name == "AddressablesInterface")!.Single();
				addressablesInterfaceSafeReleaseMethod = t.GetMethod("SafeRelease", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			}

			addressablesInterfaceSafeReleaseMethod!.Invoke(null, new object[] { handle });
		}

		public static bool IsForceSynchronous(this LocalizedReference reference)
		{
			var type = reference.GetType();
			var prop = type.GetProperty("ForceSynchronous", BindingFlags.Instance | BindingFlags.NonPublic);
			return (bool)prop!.GetValue(reference);
		}

#if UNITY_EDITOR

		private static MethodInfo registerPropertyMethod;

		public static void VariantsPropertyDriverRegisterProperty(Object target, string propertyPath)
		{
			if (registerPropertyMethod == null)
			{
				var asm = typeof(LocaleIdentifier).Assembly;
				var t = asm.GetTypes().Where(t => t.IsClass && t.Name == "VariantsPropertyDriver")!.Single();
				registerPropertyMethod = t.GetMethod("RegisterProperty", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			}

			registerPropertyMethod!.Invoke(null, new object[] { target, propertyPath });
		}

#endif
	}
}