using System;
using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Represents a single weight mapped to an arbitrary object.
	/// </summary>
	public sealed class WeightedItem<T> : IToCode, IToString, IFromString
	{
		/// <summary>
		/// The value mapped to the weight.
		/// </summary>
		public T Value { get; }

		/// <summary>
		/// The weight.
		/// </summary>
		public ulong Weight { get; }

		public WeightedItem(T value, ulong weight)
		{
			Value = value;
			Weight = weight;
		}

		/// <summary>
		/// Parse the string to create a list of WeightedItem{T}.
		/// </summary>
		public static IReadOnlyList<WeightedItem<T>> CreateList(string str)
		{
			if (StringConverter.TryFromString(str, "SL", typeof(IReadOnlyList<string>), out var items))
			{
				var itemsList = new List<WeightedItem<T>>();

				foreach (var s in (IReadOnlyList<string>)items)
				{
					if (StringConverter.TryFromString(s, "SL", typeof(WeightedItem<T>), out var c))
						itemsList.Add((WeightedItem<T>)c);
					else
						throw new Exception($"Cannot parse WeightedItem: {s}");
				}

				return itemsList;
			}

			throw new Exception($"Cannot parse list of WeightedItems: {str}");
		}

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs)" />
		IResult IToCode.ToCode(CodeGenArgs args) => new NotSupported();

		/// <summary>Implementation of IToCode.ListToCode(CodeGenArgs, object)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToCode(CodeGenArgs args, object list)
		{
			var canToString = StringConverter.TryToString((IReadOnlyList<WeightedItem<T>>)list, "SL", out var s);
			var canFromString = StringConverter.TryFromString(s, "SL", typeof(IReadOnlyList<WeightedItem<T>>), out _);
			return canToString && canFromString
				? $"{CodeConverter.ToCode<WeightedItem<T>>(args)}.{nameof(CreateList)}(\"{s}\")".ToSuccess()
				: new NotSupported();
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		IResult IToString.ToString(string format)
		{
			return format == "SL" && StringConverter.TryToString(Value, "SL", out var value)
				? $"{value}:{Weight}".ToSuccess()
				: new NotSupported();
		}

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && typeof(T) == typeof(string) && list is IReadOnlyList<WeightedItem<T>> items)
			{
				return items.ToStringArrays(
						new[] { "Item", "Weight" },
						i => new[] { i.Value?.ToString() ?? "null", i.Weight.ToString() })
					.ToTableResult();
			}

			return new NotSupported();
		}

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			if (format == "SL")
			{
				var colon = s.LastIndexOf(':');
				var canParseValue = StringConverter.TryFromString(s.Substring(0, colon), "SL", typeof(T), out var value);
				var canParseWeight = ulong.TryParse(s.Substring(colon + 1), out var weight);

				if (canParseValue && canParseWeight)
					return new WeightedItem<T>((T)value, weight).ToSuccess();
			}

			return new NotSupported();
		}
	}
}