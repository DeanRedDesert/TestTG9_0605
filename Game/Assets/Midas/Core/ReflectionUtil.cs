using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace Midas.Core
{
	public static class ReflectionUtil
	{
		#region Fields

		private static readonly char[] stringDelimiters = { '.', '[', ']' };

		[ThreadStatic]
		private static bool isGetTypeRunningOnThisThread;

		private static readonly List<string> ignoreAssemblies = new List<string>
		{
			"MonoBleedingEdge", "UnityEngine",
			"IGT.Game.Core", "Core\\IGT", "Core\\InterfaceExtensions", "SDKBuild\\IGT.Game.",
			"nunit.framework.", "VisualStudio.Unity.",
			"Roslyn\\Microsoft.CodeAnalysis", "Roslyn\\System.",
			"NSubstitute", "Unity.Legacy.NRefactory", "Managed\\ExCSS.Unity",
			"ZeroFormatter", "Ionic.Zip.Reduced", "protobuf-net.Core", "Unity.Cecil.",
			"Unity.PackageManager.", "IGT.Mpt.Core",
			"Unity.Plastic.", "GleLogic", "Logic\\Logic"
		};

		private static Assembly[] filteredAssemblyCache;
		private static Type[] typeCache;

		#endregion

		#region Public Methods

		public static string ToDescription(this Type type)
		{
			var csc = new CSharpCodeProvider();
			var r = new CodeTypeReference(type);
			return csc.GetTypeOutput(r);
		}

		public static ((PropertyInfo propertyInfo, int index)[] infos, Type returnType) GetPropertyInfos(Type type, string propertyPath)
		{
			var propertyNames = propertyPath.Split(stringDelimiters, StringSplitOptions.RemoveEmptyEntries);

			if (type == null)
				throw new ArgumentException($"Property path='{propertyPath}' is not correct. Type is null");

			var propertyInfos = new (PropertyInfo propertyInfo, int index)[propertyNames.Length];

			object instance = null;

			for (var i = 0; i < propertyNames.Length; i++)
			{
				var isNumeric = int.TryParse(propertyNames[i], out var number);

				if (type == null)
					throw new ArgumentException($"Property path='{propertyPath}' and {propertyNames[i]} is not correct. Type is null");

				if (isNumeric)
				{
					type = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
					propertyInfos[i - 1] = (null, number);
					instance = null;
					continue;
				}

				var p = type.GetProperty(propertyNames[i]);

				if (p != null)
				{
					propertyInfos[i] = (p, -1);
					// try to get instance of the returned object
					if (p.GetMethod.IsStatic)
					{
						instance = p.GetValue(p);
					}
					else if (instance != null)
					{
						instance = p.GetValue(instance);
					}

					type = instance != null ? instance.GetType() : p.PropertyType;
				}
				else
				{
					throw new ArgumentException($"Property path='{propertyPath}' is not correct");
				}
			}

			return (propertyInfos, type);
		}

		/// <summary>
		///     Load type using <see cref="Type.GetType(string)" />, and if fails,
		///     attempt to load same type from an assembly by assembly name,
		///     without specifying assembly version or any other part of the signature
		/// </summary>
		/// <param name="typeName">
		///     The assembly-qualified name of the type to get.
		///     See System.Type.AssemblyQualifiedName.
		///     If the type is in the currently executing assembly or in Mscorlib.dll, it
		///     is sufficient to supply the type name qualified by its namespace.
		/// </param>
		public static Type GetTypeFromAnyAssemblyVersion(string typeName)
		{
			// If we were unable to resolve type object
			//    - possibly because of the version change
			// Try to load using just the assembly name,
			// without any version/culture/public key info
			ResolveEventHandler assemblyResolve = OnAssemblyResolve;

			try
			{
				// Attach our custom assembly name resolver,
				// attempt to resolve again, and detach it
				AppDomain.CurrentDomain.AssemblyResolve += assemblyResolve;
				isGetTypeRunningOnThisThread = true;
				return Type.GetType(typeName);
			}
			finally
			{
				isGetTypeRunningOnThisThread = false;
				AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolve;
			}
		}

		public static Type GetType(Func<Type, bool> filter)
		{
			return GetAllTypes().FirstOrDefault(filter);
		}

		public static IEnumerable<Type> GetAllTypes(Func<Type, bool> filter)
		{
			return GetAllTypes().Where(filter);
		}

		public static IEnumerable<Type> GetAllTypes()
		{
			if (typeCache == null)
			{
				try
				{
					typeCache = GetFilteredAssemblies()
						.SelectMany(a => a.GetTypes())
						.ToArray();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}

			return typeCache;
		}

		public static IEnumerable<Type> GetAllTypeWithAttribute(Type attribute) =>
			GetAllTypes()
				.Where(t => Attribute.GetCustomAttributes(t, attribute).Length > 0);

		public static IEnumerable<string> GetAllQualifiedTypeNamesWithAttribute(Type attribute) =>
			GetAllTypeWithAttribute(attribute)
				.Select(type => type.AssemblyQualifiedName);

		public static IEnumerable<Action> CollectStaticActionFromTypesWithAttribute(Type attribute, string methodName) =>
			GetAllTypeWithAttribute(attribute)
				.SelectMany(t => t.GetMethods(), (_, m) => m)
				.Where(m => m.Name == methodName)
				.Select(m => (Action)m.CreateDelegate(typeof(Action)));

		/// <summary>
		///     Returns the FieldInfo matching 'name' from either type 'type' itself or its most-derived
		///     base type (unlike 'System.Type.GetField'). Returns null if no match is found.
		/// </summary>
		public static FieldInfo GetPrivateField(this Type type, string name, bool searchHierarchy = true)
		{
			const BindingFlags flags = BindingFlags.Instance |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			FieldInfo field;
			do
			{
				field = type.GetField(name, flags);
				type = type.BaseType;
			} while (field == null && searchHierarchy && type != null);

			return field;
		}

		public static IEnumerable<Type> GetInterfaceImplementations<TTYpe>()
		{
			return GetAllTypes(
				t => t.IsClass && !t.IsAbstract
					&& t.GetInterface(typeof(TTYpe).FullName) != null);
		}

		/// <summary>
		/// Tells you if an instance of the specified type can be null. True for all reference types and Nullable'1
		/// </summary>
		public static bool CanBeNull(Type t)
		{
			return !t.IsValueType || IsNullableType(t);
		}

		public static bool IsNullableType(Type t)
		{
			return t.IsGenericType && !t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		#endregion

		#region Private Methods

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly assembly = null;

			// Only process events from the thread that started it, not any other thread
			if (isGetTypeRunningOnThisThread)
			{
				// Extract assembly name, and checking it's the same as args.Name
				// to prevent an infinite loop
				var an = new AssemblyName(args.Name);
				if (an.Name != args.Name)
				{
					assembly = ((AppDomain)sender).Load(an.Name);
				}
			}

			return assembly;
		}

		private static IEnumerable<Assembly> GetFilteredAssemblies()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			return filteredAssemblyCache ??= assemblies
				.Where(a => !a.IsDynamic && ignoreAssemblies.TrueForAll(s => !a.Location.Contains(s)))
				.ToArray();
		}

		#endregion
	}
}