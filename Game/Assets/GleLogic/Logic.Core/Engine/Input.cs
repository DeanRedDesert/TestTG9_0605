using System;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.Engine
{
	/// <summary>
	/// Defines an input for a logic component.
	/// </summary>
	public class Input : IToString, IFromString
	{
		/// <summary>
		/// The name of this input.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The value of this input.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Initialises a new instance of the <see cref="Input"/> class.
		/// </summary>
		/// <param name="name">The name of this input.</param>
		/// <param name="value">The value of this input</param>
		public Input(string name, object value)
		{
			Name = name;
			Value = value;
		}

		public static string ToValueString(object value)
		{
			switch (value)
			{
				case Credits c: return c.ToStringOrThrow("SL");
				case Money m: return m.ToStringOrThrow("SL");
				case long l: return l.ToString();
				case string s: return s;
				case Cycles c:
				{
					if (c.Count != 1 || c[0].TotalCycles != 1)
						throw new NotSupportedException("Cannot call ToValueString on Cycles that are not 'Initial'");

					return c[0].Stage;
				}
				default: return value.ToString();
			}
		}

		/// <summary>
		/// Duplicate and return this Input with the new value.
		/// </summary>
		public virtual Input WithValue(object value) => new Input(Name, value);

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			var s = $"#{Name}#{ToValueString(Value)}";

			switch (Value)
			{
				case Credits _: return $"C{s}".ToSuccess();
				case Money _: return $"M{s}".ToSuccess();
				case long _: return $"N{s}".ToSuccess();
				case string _: return $"S{s}".ToSuccess();
				case Cycles _: return $"X{s}".ToSuccess();
				default: return new Error($"Unsupported input type: {Value.GetType().ToDisplayString()}");
			}
		}

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var sp = s.Split('#');
			object v;

			switch (sp[0])
			{
				case "C": v = sp[2].FromStringOrThrow<Credits>("SL"); break;
				case "M": v = sp[2].FromStringOrThrow<Money>("SL"); break;
				case "N": v = long.Parse(sp[2]); break;
				case "S": v = sp[2]; break;
				case "X": v = Cycles.CreateInitial(sp[2]); break;
				default: throw new Exception($"Unsupported input type: {sp[0]}");
			}

			return new Input(sp[1], v).ToSuccess();
		}
	}
}