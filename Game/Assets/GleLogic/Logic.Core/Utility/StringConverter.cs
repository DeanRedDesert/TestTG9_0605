using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Logic.Core.Types;
using Logic.Core.Types.Exits;
using Logic.Core.WinCheck;

namespace Logic.Core.Utility
{
	/// <summary>
	/// Static class to handle conversion of objects to and from a string representation using
	/// some standard formatting and implementations of IToString and IFromString.
	/// </summary>
	public static class StringConverter
	{
		private static readonly char[] delimiters = { ' ', ',', '|', '-', '~', '#' };

		#region Public Methods

		/// <summary>
		/// Converts the given object to an <see cref="IResult"/> (string) using the given format.
		/// Expected formats are:
		///  SL - Single line
		///  ML - Multi line
		/// </summary>
		public static IResult ToString(object obj, string format)
			=> ToStringInternal(obj, format);

		/// <summary>
		/// Tries to convert the given object to a string.
		/// Returns false if there was an error or the conversion was not supported, otherwise true.
		/// On false the result parameter will contain the reason.
		/// </summary>
		public static bool TryToString(object obj, string format, out string result)
		{
			switch (ToStringInternal(obj, format))
			{
				case StringSuccess s: result = s.Value; return true;
				case ObjectSuccess _: result = $"ToString should not return an ObjectSuccess: {obj.GetType().ToDisplayString()}"; return false;
				case Error e: result = $"Error: {e.Description}"; return false;
				default: result = $"Not Supported: {obj.GetType().ToDisplayString()}"; return false;
			}
		}

		/// <summary>
		/// Converts the given object to an <see cref="IResult"/> (object) using the given format.
		/// Expected formats are:
		///  SL - Single line
		///  ML - Multi line
		/// </summary>
		public static IResult FromString(string str, string format, Type destType)
			=> FromStringInternal(str, format, destType);

		/// <summary>
		/// Tries to convert the given string to an object of the given type.
		/// Returns false if there was an error or the conversion was not supported, otherwise true.
		/// On false the result parameter will contain the reason.
		/// </summary>
		public static bool TryFromString(string str, string format, Type destType, out object obj)
		{
			switch (FromStringInternal(str, format, destType))
			{
				case StringSuccess _: obj = $"FromString should not return a StringSuccess: {destType.ToDisplayString()}"; return false;
				case ObjectSuccess s: obj = s.Value; return true;
				case Error e: obj = $"Error: {e.Description}"; return false;
				default: obj = $"Not Supported: {destType.ToDisplayString()}"; return false;
			}
		}

		#endregion

		private static IResult FromStringInternal(string str, string format, Type destType)
		{
			try
			{
				if (destType == null)
					return new Error("destType is null");

				if (str == null || str == "null")
					return new ObjectSuccess(null);

				// First check if the object has a specific IFromString implementation.
				if (typeof(IFromString).IsAssignableFrom(destType))
				{
					try
					{
						var method = destType.GetMethod("FromString", BindingFlags.Static | BindingFlags.Public);

						if (method == null)
							return new Error($"{destType.ToDisplayString()} should have a static FromString method declared");

						var result = (IResult)(method.Invoke(null, new object[] { str, format }) ?? new Error("Invoke returned null"));

						if (result.IsSuccessOrError())
							return result;
					}
					catch (Exception e)
					{
						return new Error(e.Message);
					}
				}

				// Convert lists to single line string.
				if (destType.IsSupportedList(out var itemType))
				{
					// If there is a ListToString method on the itemType then call it.
					var listFromString = itemType.GetMethod("ListFromString", BindingFlags.Static | BindingFlags.Public);

					var result = listFromString == null
						? new NotSupported()
						: (IResult)listFromString.Invoke(null, new object[] { str, format, destType });

					if (result.IsSuccessOrError())
						return result;

					// Fallback to our default list handling if required.
					switch (format)
					{
						case "SL": return TryListFromSingleLineString(str, destType, itemType);
						case "ML": return TryListFromMultiLineString(str, destType, itemType);
						default: return new NotSupported();
					}
				}

				// Then fallback to our known conversions
				if (destType == typeof(string))
					return new ObjectSuccess(str);
				if (destType == typeof(int))
					return int.TryParse(str, out var i) ? i.ToSuccess() : new Error($"Could not convert '{str}' to an int value");
				if (destType == typeof(long))
					return long.TryParse(str, out var l) ? l.ToSuccess() : new Error($"Could not convert '{str}' to an long value");
				if (destType == typeof(uint))
					return uint.TryParse(str, out var ui) ? ui.ToSuccess() : new Error($"Could not convert '{str}' to an uint value");
				if (destType == typeof(ulong))
					return ulong.TryParse(str, out var ul) ? ul.ToSuccess() : new Error($"Could not convert '{str}' to an ulong value");
				if (destType == typeof(double))
					return double.TryParse(str, out var d) ? d.ToSuccess() : new Error($"Could not convert '{str}' to an double value");
				if (destType == typeof(bool))
					return bool.TryParse(str, out var b) ? b.ToSuccess() : new Error($"Could not convert '{str}' to an bool value");

				if (destType.IsEnum)
				{
					try
					{
						// Cannot use TryParse here due to .net 4.8
						return Enum.Parse(destType, str).ToSuccess();
					}
					catch
					{
						// Swallow the exception.
					}

					return new Error($"Error: Could not convert '{str}' to a {destType.ToDisplayString()} value");
				}

				if (destType == typeof(ValueTuple<string, int>))
				{
					// JBTODO Change this to Parse any tuple, this only supports (string, int)
					str = str.Trim('(', ')');
					var sp = str.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
					return new ValueTuple<string, int>(sp[0], int.Parse(sp[1])).ToSuccess();
				}
			}
			catch (Exception e)
			{
				return new Error(UnwrapMessage(e));
			}

			return new NotSupported();
		}

		private static IResult ToStringInternal(object obj, string format)
		{
			try
			{
				// ReSharper disable once ConvertIfStatementToSwitchStatement
				if (obj == null)
					return "null".ToSuccess();

				// First check if the object has a specific IToString implementation.
				if (obj is IToString ts)
				{
					var r1 = ts.ToString(format);

					if (r1.IsSuccessOrError())
						return r1;
				}

				// Second check if the object has a custom conversion.
				var r2 = CustomToString(obj, format);

				if (r2.IsSuccessOrError())
					return r2;

				// Then try convert lists to strings.
				if (obj.GetType().IsSupportedList(out var itemType))
				{
					// If there is a ListToString method on the itemType then call it.
					var listToString = itemType.GetMethod("ListToString", BindingFlags.Static | BindingFlags.Public);

					if (listToString != null)
					{
						var r3 = (IResult)listToString.Invoke(null, new[] { obj, format });

						if (r3.IsSuccessOrError())
							return r3;
					}

					// Fallback to our default list handling if required.
					switch (format)
					{
						case "SL": return TryListToSingleLineString(obj);
						case "ML":
						{
							var r = TryListToMultiLineTable(obj);
							return r.IsSuccessOrError() ? r : TryListToMultiLineString(obj);
						}
						default: return new NotSupported();
					}
				}

				// Then try convert dictionaries to strings.
				if (obj.GetType().IsSupportedDict(out _, out _))
				{
					// Fallback to our default list handling if required.
					switch (format)
					{
						case "ML": return TryDictToMultiLineString((IDictionary)obj);
						default: return new NotSupported();
					}
				}

				// Fallback to our known conversions
				switch (obj)
				{
					case string s: return s.ToSuccess();
					case int i: return i.ToString("G", CultureInfo.CurrentCulture).ToSuccess();
					case long l: return l.ToString("G", CultureInfo.CurrentCulture).ToSuccess();
					case uint ui: return ui.ToString("G", CultureInfo.CurrentCulture).ToSuccess();
					case ulong ul: return ul.ToString("G", CultureInfo.CurrentCulture).ToSuccess();
					case double d: return d.ToString("G", CultureInfo.CurrentCulture).ToSuccess();
					case bool b: return b.ToString().ToSuccess();
					case ITuple t:
					{
						var str = "";

						for (var i = 0; i < t.Length; i++)
						{
							if (i > 0)
								str += ", ";

							var r = ToStringInternal(t[i], "SL");

							if (r is StringSuccess s)
								str += s.Value;
							else
								return r;
						}

						if (obj.GetType().FullName?.StartsWith("System.Tuple`") ?? false)
							return $"[{str}]".ToSuccess();

						return $"({str})".ToSuccess();
					}
					default:
					{
						var type = obj.GetType();

						if (type.IsEnum)
							return obj.ToString().ToSuccess();

						// Last option try a generic object to text process
						switch (format)
						{
							case "SL": return new NotSupported();
							case "ML": return TryObjectToMultiLineString(obj);
							default: return new NotSupported();
						}
					}
				}
			}
			catch (Exception e)
			{
				return new Error(UnwrapMessage(e));
			}
		}

		private static IResult TryListToSingleLineString(object list)
		{
			var validDelimiters = delimiters.ToList();
			var convertedItems = new List<string>();

			foreach (var item in (IEnumerable)list)
			{
				var r = ToStringInternal(item, "SL");

				if (r is StringSuccess s)
				{
					for (var i = validDelimiters.Count - 1; i >= 0; i--)
					{
						if (s.Value.Contains(validDelimiters[i]))
							validDelimiters.RemoveAt(i);
					}

					if (validDelimiters.Count == 0)
						return new Error("No delimiters are available");

					convertedItems.Add(s.Value);
				}
				else
					return r;
			}

			return convertedItems.Count == 0
				? "empty".ToSuccess()
				: (validDelimiters[0] + string.Join(validDelimiters[0].ToString(), convertedItems.ToArray())).ToSuccess();
		}

		private static IResult TryListToMultiLineTable(object list)
		{
			var table = new List<IReadOnlyList<string>>();
			IReadOnlyList<PropertyInfo> props = null;
			Type itemType = null;

			var count = 0;

			// First try to convert all items to single line values.
			foreach (var o in (IEnumerable)list)
			{
				count++;

				// If the object has a single line conversion then we abort the table rendering and
				// let it fallback and show the multi-line list instead.
				if (ToStringInternal(o, "SL") is StringSuccess)
					return new NotSupported();

				if (itemType == null)
				{
					// First item, initialise the type and properties.
					itemType = o.GetType();
					props = GetProperties(itemType);

					// Types that have no properties cannot be rendered as a table.
					if (props.Count == 0)
						return new NotSupported();

					// Add the header row
					table.Add(props.Select(p => p.Name).ToArray());
				}
				else if (itemType != o.GetType())
				{
					// Cannot create a table if the item types are different.
					return new NotSupported();
				}

				var row = new List<string>();

				foreach (var prop in props)
				{
					if (prop.GetIndexParameters().Length > 0)
						continue;

					switch (ToStringInternal(prop.GetValue(o), "SL"))
					{
						case StringSuccess s: row.Add(s.Value); break;
						case NotSupported _: return new NotSupported();
						case Error e: return e;
					}
				}

				table.Add(row);
			}

			return count == 0
				? new StringSuccess("empty")
				: table.ToTableResult();
		}

		private static IResult TryListToMultiLineString(object list)
		{
			var results = new List<string>();
			var aborted = false;

			// First try convert all items to single line values.
			foreach (var o in (IEnumerable)list)
			{
				switch (ToStringInternal(o, "SL"))
				{
					case StringSuccess s: results.Add(s.Value); break;
					case NotSupported _: aborted = true; break;
					case Error e: return e;
				}

				if (aborted)
					break;
			}

			if (!aborted)
				return results.Join().ToSuccess();

			// Second try convert all items to multi line values.
			results.Clear();

			foreach (var o in (IEnumerable)list)
			{
				switch (ToStringInternal(o, "ML"))
				{
					case StringSuccess s: results.Add(s.Value); break;
					case NotSupported n: return n;
					case Error e: return e;
				}
			}

			return results.Join(string.Format("{0}{0}==========={0}{0}", Environment.NewLine)).ToSuccess();
		}

		private static IResult TryDictToMultiLineString(IDictionary dict)
		{
			var results = new List<IReadOnlyList<string>> { new[] { "Key", "Value" } };

			// First try convert all items to single line values.
			foreach (var key in dict.Keys)
			{
				var value = dict[key];

				var kResult = ToStringInternal(key, "SL");
				var vResult = ToStringInternal(value, "SL");

				if (kResult is StringSuccess k && vResult is StringSuccess v)
				{
					results.Add(new[] { k.Value, v.Value });
				}
				else
				{
					if (kResult is Error e1)
						return e1;
					if (vResult is Error e2)
						return e2;
					if (kResult is NotSupported || vResult is NotSupported)
						return new NotSupported();
				}
			}

			return results.ToTableResult();
		}

		private static IResult TryObjectToMultiLineString(object obj)
		{
			var results = new List<string>();

			foreach (var prop in GetProperties(obj.GetType()))
				results.AddRange(PropertyToLines(prop.Name, prop.GetValue(obj)));

			return results.Join().ToSuccess();

			IReadOnlyList<string> PropertyToLines(string name, object value)
			{
				var sl = ToStringInternal(value, "SL");

				switch (sl)
				{
					case StringSuccess s1: return new[] { $"{name}: {s1.Value}" };
					case Error e1: return new[] { $"{name}: ERROR {e1.Description}" };
				}

				var ml = ToStringInternal(value, "ML");

				switch (ml)
				{
					case StringSuccess s2: return new[] { $"{name}:" }.Concat(s2.Value.Indent(2, ' ').ToLines(false, false)).ToArray();
					case Error e2: return new[] { $"{name}: ERROR {e2.Description}" };
				}

				return new[] { $"{name}: <not supported {obj.GetType().ToDisplayString()}>" };
			}
		}

		private static IResult TryListFromSingleLineString(string source, Type listType, Type itemType)
		{
			if (source == string.Empty || source == "empty")
				return CreateList(Array.CreateInstance(itemType, 0), listType);

			var delimiter = source[0];
			var items = source.Substring(1).Split(delimiter);
			var parsedItems = Array.CreateInstance(itemType, items.Length);

			for (var i = 0; i < items.Length; i++)
			{
				var r = FromStringInternal(items[i], "SL", itemType);

				if (r is ObjectSuccess s)
					parsedItems.SetValue(s.Value, i);
				else
					return r;
			}

			return CreateList(parsedItems, listType);
		}

		private static IResult TryListFromMultiLineString(string source, Type listType, Type itemType)
		{
			var lines = source.ToLines();
			var parsedItems = Array.CreateInstance(itemType, lines.Count);

			for (var i = 0; i < lines.Count; i++)
			{
				var r = FromStringInternal(lines[i], "SL", itemType);

				if (r is ObjectSuccess s)
					parsedItems.SetValue(s.Value, i);
				else
					return r;
			}

			return CreateList(parsedItems, listType);
		}

		private static IResult CreateList(Array items, Type listType)
		{
			// Create different lists depending on the expected type.
			if (listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
			{
				// Create an array
				return items.ToSuccess();
			}

			if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
			{
				// Create a list
				var list = Activator.CreateInstance(listType);
				var listAdd = listType.GetMethod("AddRange");
				listAdd?.Invoke(list, new object[] { items });

				return list.ToSuccess();
			}

			if (listType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
			{
				// Create the object with the items as the constructor parameter
				listType.GetConstructors()[0].Invoke(new object[] { items }).ToSuccess();
			}

			return new NotSupported();
		}

		/// <summary>
		/// A method where we can add ToString conversions that are not supported by the current system.
		/// For instance lists of a base/interface type.
		/// </summary>
		private static IResult CustomToString(object obj, string format)
		{
			switch (obj)
			{
				case IReadOnlyList<ICyclesModifier> l: return format == "ML" ? l.Select(GetExitString).Join().ToSuccess() : new NotSupported();
				case IReadOnlyList<ISymbolListStrip> l:
				{
					if (format != "ML")
						return new NotSupported();

					var lines = new List<string>();

					foreach (var strip in l)
					{
						var r = ToString(strip, "SL");

						if (r is StringSuccess s)
							lines.Add(s.Value);
						else
							return r;
					}

					return lines.Join().ToSuccess();
				}
				default: return new NotSupported();
			}

			string GetExitString(ICyclesModifier m)
			{
				return TryToString(m, "SL", out var s)
					? s
					: TryToString(m, "ML", out s)
						? s
						: m.GetType().ToDisplayString();
			}
		}

		private static IReadOnlyList<PropertyInfo> GetProperties(Type type)
		{
			var isString = type == typeof(string);
			var isList = type.IsSupportedList(out _);
			var isDict = type.IsSupportedDict(out _, out _);
			var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
			var isMask = type == typeof(ReadOnlyMask);

			return isString || isList || isDict || isNullable || isMask
				? Array.Empty<PropertyInfo>()
				: type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0).ToArray();
		}

		private static string UnwrapMessage(Exception e)
		{
			switch (e)
			{
				case TargetInvocationException t: return t.InnerException != null ? UnwrapMessage(t.InnerException) : t.Message;
				case AggregateException a: return a.InnerExceptions.Count > 0 ? UnwrapMessage(a.InnerExceptions[0]) : a.Message;
				default: return e.Message;
			}
		}
	}
}