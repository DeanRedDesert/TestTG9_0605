using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Tools.Editor
{
	public enum UnitySerializableType
	{
		/// <summary>
		/// Not serializable
		/// </summary>
		None,

		/// <summary>
		/// A Unity primitive (int, float, double, string, etc)
		/// </summary>
		Primitive,

		/// <summary>
		/// A Unity built-in serializable type (Vector2, Vector3, etc)
		/// </summary>
		UnityBuiltIn,

		/// <summary>
		/// A Unity object, including custom MonoBehaviours
		/// </summary>
		UnityObject,

		/// <summary>
		/// A class or struct with [Serializable] defined
		/// </summary>
		CustomSerializable,

		/// <summary>
		/// A collection (list or array) of UnitySerializableType.Primitive
		/// </summary>
		PrimitiveCollection,

		/// <summary>
		/// A collection (list or array) of UnitySerializableType.UnityBuiltIn
		/// </summary>
		UnityBuiltInCollection,

		/// <summary>
		/// A collection (list or array) of UnitySerializableType.UnityObject
		/// </summary>
		UnityObjectCollection,

		/// <summary>
		/// A collection (list or array) of UnitySerializableType.CustomSerializable
		/// </summary>
		CustomSerializableCollection
	}

	public static class SerializedPropertyHelper
	{
		private static readonly Regex numberExt = new Regex(@"[\d]+", RegexOptions.Compiled);

		private static readonly Type[] serializableUnityPrimitiveTypes =
		{
			typeof(bool),
			typeof(byte),
			typeof(sbyte),
			typeof(char),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(string)
		};

		private static readonly Type[] serializableUnityBuiltInTypes =
		{
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Quaternion),
			typeof(Matrix4x4),
			typeof(Color),
			typeof(Color32),
			typeof(Rect),
			typeof(RectOffset),
			typeof(LayerMask),
			typeof(AnimationCurve),
			typeof(Gradient),
			typeof(GUIStyle)
		};

		private static readonly Type[] serializableEnumTypes =
		{
			typeof(byte),
			typeof(sbyte),
			typeof(char),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint)
		};

		public static FieldInfo GetPrivateOrPublicField(this Type t, string name)
		{
			const BindingFlags bf = BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			FieldInfo fi;
			while ((fi = t.GetField(name, bf)) == null && (t = t.BaseType) != null) { }

			return fi;
		}

		public static bool TryGetFieldValueAtPath(this SerializedObject serializedObject, string path, out object result)
		{
			var o = (object)serializedObject.targetObject;
			var split = path.Split('.');

			for (var i = 0; i < split.Length; i++)
			{
				switch (split[i])
				{
					case "Array":
						if (i == split.Length - 1 || !(o is IList oAsList))
						{
							result = default;
							return false;
						}

						i++;
						var matches = numberExt.Matches(split[i]);
						if (matches.Count != 1 || !int.TryParse(matches[0].Value, out var element))
						{
							result = default;
							return false;
						}

						o = oAsList[element];
						break;

					default:
						var field = o.GetType().GetPrivateOrPublicField(split[i]);
						if (field == null)
						{
							result = default;
							return false;
						}

						o = field.GetValue(o);
						break;
				}
			}

			result = o;
			return true;
		}

		private static UnitySerializableType GetUnitySerializableType(this Type t)
		{
			var isCollection = false;
			if (t.IsArray)
			{
				if (t.GetArrayRank() != 1 || (t = t.GetElementType()) == null)
					return UnitySerializableType.None;

				isCollection = true;
			}
			else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
			{
				t = t.GetGenericArguments()[0];
				isCollection = true;
			}

			if (serializableUnityPrimitiveTypes.Contains(t) ||
				t.IsEnum && serializableEnumTypes.Contains(t.GetEnumUnderlyingType()))
				return isCollection ? UnitySerializableType.PrimitiveCollection : UnitySerializableType.Primitive;

			if (serializableUnityBuiltInTypes.Contains(t))
				return isCollection ? UnitySerializableType.UnityBuiltInCollection : UnitySerializableType.UnityBuiltIn;

			if (typeof(UnityEngine.Object).IsAssignableFrom(t))
				return isCollection ? UnitySerializableType.UnityObjectCollection : UnitySerializableType.UnityObject;

			if (t.GetCustomAttributes(typeof(SerializableAttribute)).Any())
				return isCollection ? UnitySerializableType.CustomSerializableCollection : UnitySerializableType.CustomSerializable;

			return UnitySerializableType.None;
		}

		public static IEnumerable<T> GetAllSerializedFieldsRecursive<T>(this Object unityObject)
		{
			var result = new List<T>();
			var visitedObjects = new List<object>();

			GetFieldsRecursive(unityObject);

			return result;

			void GetFieldsRecursive(object o)
			{
				if (o == null || visitedObjects.Contains(o))
					return;

				// Make sure that any before serialization work is already done to ensure the real serialized fields are populated

				if (o is ISerializationCallbackReceiver scr)
					scr.OnBeforeSerialize();

				visitedObjects.Add(o);
				foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (!field.IsPublic && !field.GetCustomAttributes<SerializeField>().Any())
						continue;

					var serType = GetUnitySerializableType(field.FieldType);
					if (serType == UnitySerializableType.None)
						continue;

					if (typeof(T).IsAssignableFrom(field.FieldType))
					{
						result.Add((T)field.GetValue(o));
						continue;
					}

					switch (serType)
					{
						case UnitySerializableType.Primitive:
						case UnitySerializableType.UnityBuiltIn:
						case UnitySerializableType.PrimitiveCollection:
						case UnitySerializableType.UnityBuiltInCollection:
						case UnitySerializableType.UnityObject:
						case UnitySerializableType.UnityObjectCollection:
							break;

						case UnitySerializableType.CustomSerializable:
							GetFieldsRecursive(field.GetValue(o));
							break;

						case UnitySerializableType.CustomSerializableCollection:
							var list = (IList)field.GetValue(o);
							if (list != null)
							{
								foreach (var val in list)
								{
									if (val is T valAsT)
									{
										result.Add(valAsT);
										continue;
									}

									GetFieldsRecursive(val);
								}
							}

							break;
					}
				}
			}
		}
	}
}