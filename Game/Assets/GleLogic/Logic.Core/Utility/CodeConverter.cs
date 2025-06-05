using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Logic.Core.Utility
{
	/// <summary>
	/// Static class to handle conversion of objects to and from a code representation using
	/// some standard C# formatting and implementations of IToCode.
	/// </summary>
	public static class CodeConverter
	{
		/// <summary>
		/// Converts the type specified to a C# string representation.
		/// This method should never fail, but an exception will be thrown if something unexpected happens.
		/// </summary>
		public static string ToCode(this Type type, CodeGenArgs args) => TryToCode(args, type, out var s) ? s : throw new Exception(s);

		/// <summary>
		/// Converts the type specified in T to a C# string representation.
		/// This method should never fail, but an exception will be thrown if something unexpected happens.
		/// </summary>
		public static string ToCode<T>(CodeGenArgs args) => TryToCode(args, typeof(T), out var s) ? s : throw new Exception(s);

		/// <summary>
		/// Converts the obj to a C# string representation.
		/// Use this method if you know the conversion will always succeed.
		/// If it fails an exception will be thrown.
		/// </summary>
		public static string ToCodeOrThrow(CodeGenArgs args, object obj) => TryToCode(args, obj, out var s) ? s : throw new Exception(s);

		/// <summary>
		/// Trys to convert the <see cref="obj"/> to a C# string representation.
		/// Use this method if you don't want an exception on failure.
		/// This can fail if the conversion causes an error or if the <see cref="obj"/> is not supported.
		/// Returns true if the conversion succeeded and <see cref="str"/> will contain the C# string representation.
		/// Returns false if the conversion failed and <see cref="str"/> will contain the error information.
		/// </summary>
		public static bool TryToCode(CodeGenArgs args, object obj, out string str)
		{
			switch (ToCodeInternal(args, obj))
			{
				case StringSuccess s: str = s.Value; return true;
				case Error e: str = e.Description; return false;
				default: str = $"Not Supported {obj.GetType().ToDisplayString()}"; return false;
			}
		}

		private static IResult ToCodeInternal(CodeGenArgs args, object obj)
		{
			if (obj == null)
				return "null".ToSuccess();

			// Test if the obj reference is already contained in the fields.
			if (args.FieldNames.TryGetValue(obj, out var field))
				return field.ToSuccess();

			// Test if the object has a custom ToCode implementation
			if (obj is IToCode c)
			{
				var r = c.ToCode(args);

				if (r.IsSuccessOrError())
					return r;
			}

			var type = obj.GetType();

			// Test if the object has a list whose items have a custom ToCode implementation
			if (type.IsSupportedList(out var itemType) && typeof(IToCode).IsAssignableFrom(itemType))
			{
				// If there is a ListToCode method on the itemType then call it.
				var listToCode = itemType.GetMethod("ListToCode", BindingFlags.Static | BindingFlags.Public);

				if (listToCode != null)
				{
					var r = (IResult)listToCode.Invoke(null, new[] { args, obj });

					if (r.IsSuccessOrError())
						return r;
				}
			}

			// Check core types
			var r1 = CoreTypesToCode(args, obj);

			if (r1.IsSuccessOrError())
				return r1;

			// Check tuple
			if (obj is ITuple tuple)
			{
				var r = TupleToCode(args, tuple);

				if (r.IsSuccessOrError())
					return r;
			}

			// Check enum
			if (type.IsEnum)
			{
				var r = EnumToCode(args, obj, type);

				if (r.IsSuccessOrError())
					return r;
			}

			// Check array
			if (type.IsArray)
			{
				var r = ArrayToCode(args, obj, type);

				if (r.IsSuccessOrError())
					return r;
			}

			// Check generic types
			if (type.IsGenericType)
			{
				var r = GenericToCode(args, obj, type);

				if (r.IsSuccessOrError())
					return r;
			}

			// Check record types
			return RecordToCode(args, obj, type);
		}

		private static IResult CoreTypesToCode(CodeGenArgs args, object obj)
		{
			switch (obj)
			{
				case Type t:
				{
					if (t == typeof(object))
						return "object".ToSuccess();
					if (t == typeof(string))
						return "string".ToSuccess();
					if (t == typeof(bool))
						return "bool".ToSuccess();
					if (t == typeof(int))
						return "int".ToSuccess();
					if (t == typeof(uint))
						return "uint".ToSuccess();
					if (t == typeof(long))
						return "long".ToSuccess();
					if (t == typeof(ulong))
						return "ulong".ToSuccess();
					if (t == typeof(double))
						return "double".ToSuccess();
					if (t.IsArray)
						return $"{ToCodeOrThrow(args, t.GetElementType())}[]".ToSuccess();
					if (t.IsGenericType && t.IsValueType && t.Name.StartsWith("ValueTuple`"))
						return $"({string.Join(", ", t.GetGenericArguments().Select(a => ToCodeOrThrow(args, a)))})".ToSuccess();

					// Output the type name and namespace.
					args.AddNamespace(t.Namespace);

					var name = TrimName(t.UnderlyingSystemType.Name);

					if (t.IsNested)
						name = $"{ToCodeOrThrow(args, t.DeclaringType)}.{name}";

					return t.IsGenericType
						? $"{name}<{string.Join(", ", t.GetGenericArguments().Select(a => ToCodeOrThrow(args, a)))}>".ToSuccess()
						: $"{name}".ToSuccess();

					string TrimName(string n)
					{
						var indexOf = n.IndexOf('`');
						return n.Substring(0, indexOf == -1 ? n.Length : indexOf);
					}
				}
				case string s: return $"\"{s.Replace(@"\", @"\\")}\"".ToSuccess();
				case bool b: return $"{b}".ToLower().ToSuccess();
				case int i: return $"{i}".ToSuccess();
				case uint ui: return $"{ui}U".ToSuccess();
				case long l: return $"{l}L".ToSuccess();
				case ulong ul: return $"{ul}UL".ToSuccess();
				case double d: return $"{d}".ToSuccess();
				default: return new NotSupported();
			}
		}

		private static IResult TupleToCode(CodeGenArgs args, ITuple tuple)
		{
			var str = tuple.GetType().FullName?.StartsWith("System.Tuple`") ?? false
				? $"new {ToCodeOrThrow(args, tuple.GetType())}("
				: "(";

			for (var i = 0; i < tuple.Length; i++)
			{
				switch (ToCodeInternal(args, tuple[i]))
				{
					case StringSuccess s: str += i == 0 ? s.Value : $", {s.Value}"; break;
					case Error e: return e;
					case NotSupported _: return new Error($"Object inside tuple is not supported: {tuple[i]?.GetType().ToDisplayString() ?? "null"}");
				}
			}

			str += ")";
			return str.ToSuccess();
		}

		private static IResult EnumToCode(CodeGenArgs args, object obj, Type type)
		{
			args.AddNamespace(type.Namespace);

			if (type.IsDefined(typeof(FlagsAttribute), false))
			{
				return Enum.GetValues(type).Cast<Enum>()
					.Where(value => ((Enum)obj).HasFlag(value))
					.Aggregate("", (current, value) => current + $"{(current == "" ? "" : " | ")}{ToCodeOrThrow(args, type)}.{value}")
					.ToSuccess();
			}

			return $"{ToCodeOrThrow(args, type)}.{obj}".ToSuccess();
		}

		private static IResult ArrayToCode(CodeGenArgs args, object obj, Type type)
		{
			var elementType = type.GetElementType();

			if (((IList)obj).Count == 0)
			{
				switch (args.LanguageVersion)
				{
					case LanguageVersion.CSharp7_3: return $"new {ToCodeOrThrow(args, elementType)}[0]".ToSuccess();
					case LanguageVersion.CSharp11: return $"{ToCodeOrThrow(args, typeof(Array))}.{nameof(Array.Empty)}<{ToCodeOrThrow(args, elementType)}>()".ToSuccess();
					case LanguageVersion.Latest: return "[]".ToSuccess();
					default: return new NotSupported();
				}
			}

			var r = ItemsToCode(args, ((IList)obj).Cast<object>());

			if (r is StringSuccess ss)
			{
				if (args.LanguageVersion == LanguageVersion.Latest)
				{
					return ss.Value.Contains(Environment.NewLine)
						? $"[{Environment.NewLine}{ss.Value.Indent()}{Environment.NewLine}]".ToSuccess()
						: $"[{ss.Value}]".ToSuccess();
				}

				return ss.Value.Contains(Environment.NewLine)
					? $"new {ToCodeOrThrow(args, type.GetElementType())}[]{Environment.NewLine}{{{Environment.NewLine}{ss.Value.Indent()}{Environment.NewLine}}}".ToSuccess()
					: $"new {ToCodeOrThrow(args, type.GetElementType())}[] {{ {ss.Value} }}".ToSuccess();
			}

			return r;
		}

		private static IResult GenericToCode(CodeGenArgs args, object obj, Type type)
		{
			if (type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) || type.Name == "<>z__ReadOnlyArray`1")
			{
				var elementType = type.GenericTypeArguments[0];
				var items = ((IList)obj).OfType<object>().ToArray();

				if (items.Length == 0)
				{
					return args.LanguageVersion == LanguageVersion.CSharp7_3
						? $"new {ToCodeOrThrow(args, elementType)}[0]".ToSuccess()
						: $"{ToCodeOrThrow(args, typeof(Array))}.{nameof(Array.Empty)}<{ToCodeOrThrow(args, elementType)}>()".ToSuccess();
				}

				var r = ItemsToCode(args, items);

				if (r is StringSuccess ss)
				{
					return ss.Value.Contains(Environment.NewLine)
						? $"new {ToCodeOrThrow(args, elementType)}[]{Environment.NewLine}{{{Environment.NewLine}{ss.Value.Indent()}{Environment.NewLine}}}".ToSuccess()
						: $"new {ToCodeOrThrow(args, elementType)}[] {{ {ss.Value} }}".ToSuccess();
				}

				return r;
			}

			if (type.GetGenericTypeDefinition() == typeof(List<>))
			{
				var items = ((IList)obj).OfType<object>().ToArray();

				if (items.Length == 0)
					return $"new {ToCodeOrThrow(args, type)}()".ToSuccess();

				var r = ItemsToCode(args, items);

				if (r is StringSuccess ss)
				{
					return ss.Value.Contains(Environment.NewLine)
						? $"new {ToCodeOrThrow(args, type)}(){Environment.NewLine}{{{Environment.NewLine}{ss.Value.Indent()}{Environment.NewLine}}}".ToSuccess()
						: $"new {ToCodeOrThrow(args, type)}() {{ {ss.Value} }}".ToSuccess();
				}

				return r;
			}

			if (type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) ||
				type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			{
				var typeStr = ToCodeOrThrow(args, type);

				if (typeStr.StartsWith("IReadOnly"))
					typeStr = typeStr.Substring(9);

				var items = ((IDictionary)obj).OfType<object>().ToArray();

				if (items.Length == 0)
					return $"new {typeStr}()".ToSuccess();

				var r = KeyValuePairsToCode(args, items);

				return r is StringSuccess ss
					? $"new {typeStr}(){Environment.NewLine}{{{Environment.NewLine}{ss.Value.Indent()}{Environment.NewLine}}}".ToSuccess()
					: r;
			}

			return new NotSupported();
		}

		private static IResult RecordToCode(CodeGenArgs args, object obj, Type type)
		{
			try
			{
				// Try to save it out as a record type.
				var constructors = type.GetConstructors();

				if (constructors.Length == 1)
				{
					var parameterStrings = new List<string>();
					var parameters = constructors[0].GetParameters();
					var properties = type.GetProperties();
					var privateFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

					for (var i = 0; i < parameters.Length; i++)
					{
						var parameter = parameters[i];
						var property = properties.SingleOrDefault(p => p.Name.IsSame(parameter.Name));

						if (property == null)
						{
							var field = privateFields.SingleOrDefault(p => p.Name.IsSame(parameter.Name));

							if (field == null)
								return new Error($"\"not supported {ToCodeOrThrow(args, type)} cannot find data to use in parameter {parameter.Name}\"");

							var value = field.GetValue(obj);

							if (!TryToCode(args, value, out var f))
								return new Error(f);

							parameterStrings.Add(value is Type ? $"typeof({f})" : f);
						}
						else
						{
							var value = property.GetValue(obj);

							if (!TryToCode(args, value, out var p))
								return new Error(p);

							parameterStrings.Add(value is Type ? $"typeof({p})" : p);
						}
					}

					if (parameterStrings.Any(ps => ps.Contains(Environment.NewLine)))
					{
						var sb = new StringBuilder();
						sb.AppendLine($"new {ToCodeOrThrow(args, type)}(");

						for (var i = 0; i < parameterStrings.Count; i++)
						{
							var ps = parameterStrings[i];
							sb.AppendLine(i == parameterStrings.Count - 1 ? ps.Indent() : $"{ps.Indent()},");
						}

						sb.Append(')');
						return sb.ToString().ToSuccess();
					}

					return $"new {ToCodeOrThrow(args, type)}({string.Join(", ", parameterStrings)})".ToSuccess();
				}

				return new Error($"\"not supported {ToCodeOrThrow(args, type)} too many constructors\"");
			}
			catch (Exception e)
			{
				return new Error($"\"error {e.Message}\"");
			}
		}

		private static IResult ItemsToCode(CodeGenArgs args, IEnumerable<object> objects)
		{
			var items = new List<string>();

			foreach (var o in objects)
			{
				switch (ToCodeInternal(args, o))
				{
					case StringSuccess s: items.Add(s.Value); break;
					case Error e: return e;
					case NotSupported _: return new Error($"Object inside list is not supported: {o?.GetType().ToDisplayString() ?? "null"}");
				}
			}

			if (items.Count == 0)
				return "".ToSuccess();

			var totalLength = items.Sum(i => i.Length);
			var averageLength = totalLength / items.Count;
			var multiline = totalLength > 250 && averageLength > 10;

			return string.Join(multiline ? $",{Environment.NewLine}" : ", ", items).ToSuccess();
		}

		private static IResult KeyValuePairsToCode(CodeGenArgs args, IEnumerable<object> keyValuePairs)
		{
			var items = new List<string>();

			foreach (var o in keyValuePairs)
			{
				var t = o.GetType();
				var key = t.GetProperty("Key")?.GetValue(o);
				var value = t.GetProperty("Value")?.GetValue(o);

				var k = ToCodeInternal(args, key);
				var v = ToCodeInternal(args, value);

				if (k is StringSuccess ks && v is StringSuccess vs)
				{
					if (ks.Value.Contains(Environment.NewLine) || vs.Value.Contains(Environment.NewLine))
						items.Add($"{{{Environment.NewLine}{ks.Value.Indent()},{Environment.NewLine}{vs.Value.Indent()}{Environment.NewLine}}}");
					else
						items.Add($"{{ {ks.Value}, {vs.Value} }}");
				}
				else
				{
					switch (k)
					{
						case Error e: return e;
						case NotSupported _: return new Error($"Object inside key is not supported: {key?.GetType().ToDisplayString() ?? "null"}");
					}

					switch (v)
					{
						case Error e: return e;
						case NotSupported _: return new Error($"Object inside value is not supported: {value?.GetType().ToDisplayString() ?? "null"}");
					}
				}
			}

			switch (items.Count)
			{
				case 0: return "".ToSuccess();
				case 1: return items[0].ToSuccess();
				default: return string.Join($",{Environment.NewLine}", items).ToSuccess();
			}
		}
	}
}