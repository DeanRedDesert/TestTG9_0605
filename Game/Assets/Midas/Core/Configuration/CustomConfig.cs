using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Midas.Core.Configuration
{
	public sealed class CustomConfig
	{
		public IReadOnlyDictionary<string, object> ThemeConfigItems { get; }
		public IReadOnlyDictionary<string, object> PayVarConfigItems { get; }

		public CustomConfig(IReadOnlyDictionary<string, object> themeValues, IReadOnlyDictionary<string, object> payVarValues)
		{
			ThemeConfigItems = themeValues;
			PayVarConfigItems = payVarValues;
		}

		public override string ToString()
		{
			var builder = new StringBuilder(128);
			builder.Append("ThemeValues:");
			foreach (var pair in ThemeConfigItems)
			{
				builder.Append('(');
				builder.Append(pair.Key);
				builder.Append(", ");
				builder.Append(pair.Value);
				builder.Append(')');
			}

			builder.Append(" PayVarValues:");
			foreach (var pair in PayVarConfigItems)
			{
				builder.Append('(');
				builder.Append(pair.Key);
				builder.Append(", ");
				builder.Append(pair.Value);
				builder.Append(')');
			}

			return builder.ToString();
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex)
		{
			serializeComplex(writer, ThemeConfigItems);
			serializeComplex(writer, PayVarConfigItems);
		}

		public static CustomConfig Deserialize(BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var themeValues = (IReadOnlyDictionary<string, object>)deserializeComplex(reader);
			var payVarValues = (IReadOnlyDictionary<string, object>)deserializeComplex(reader);

			return new CustomConfig(themeValues, payVarValues);
		}
	}
}