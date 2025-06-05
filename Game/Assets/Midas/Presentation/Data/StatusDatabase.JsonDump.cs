using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.JsonConverters;
using Midas.Presentation.DevHelpers.JsonConverters;
using Newtonsoft.Json;

namespace Midas.Presentation.Data
{
	public static partial class StatusDatabase
	{
		#region Json Converters

		private static readonly IDictionary<Type, JsonConverter> defaultConverters =
			new Dictionary<Type, JsonConverter>
			{
				{ CreditConverter.ConverterType, new CreditConverter() },
				{ MoneyConverter.ConverterType, new MoneyConverter() },
				{ RationalNumberConverter.ConverterType, new RationalNumberConverter() },
				{ StatusBlockConverter.ConverterType, new StatusBlockConverter() },
				{ StatusPropertyConverter.ConverterType, new StatusPropertyConverter() },
				{ UnityColorConverter.ConverterType, new UnityColorConverter() }
			};

		public sealed class StatusBlockConverter : Core.JsonConverters.JsonConverter<StatusBlock>
		{
			protected override void WriteJson(JsonWriter writer, StatusBlock value, JsonSerializer serializer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("Name");
				writer.WriteValue(value.Name);

				if (value.Properties.Any())
				{
					foreach (var property in value.Properties)
					{
						writer.WritePropertyName(property.Name);
						serializer.Serialize(writer, property.Value);
					}
					//writer.WritePropertyName("Properties");
					//serializer.Serialize(writer, value.PropertyValues);
				}

				if (value is StatusBlockCompound compound && compound.StatusBlocks.Any())
				{
					foreach (var statusBlock in compound.StatusBlocks)
					{
						writer.WritePropertyName(statusBlock.Name);
						serializer.Serialize(writer, statusBlock);
					}
				}

				writer.WriteEndObject();
			}
		}

		public sealed class StatusPropertyConverter : Core.JsonConverters.JsonConverter<IStatusProperty>
		{
			protected override void WriteJson(JsonWriter writer, IStatusProperty value, JsonSerializer serializer)
			{
				writer.WriteStartObject();

				writer.WritePropertyName(value.Name);
				serializer.Serialize(writer, value.Value);

				writer.WriteEndObject();
			}
		}

		#endregion

		public static string GetJsonDump(bool niceFormat, params JsonConverter[] converters)
		{
			return JsonConvert.SerializeObject(StatusBlocksInstance, GetJsonSerializerSettings(niceFormat, converters));
		}

		private static JsonSerializerSettings GetJsonSerializerSettings(bool niceFormat, params JsonConverter[] converters)
		{
			var settings = new JsonSerializerSettings
			{
				Converters = converters.ToList(),
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = niceFormat ? Formatting.Indented : Formatting.None
			};

			foreach (var converter in defaultConverters)
			{
				if (converters.Any(x => x.CanConvert(converter.Key)))
					continue;

				settings.Converters.Add(converter.Value);
			}

			settings.Converters.Add(new EnumConverter());

			return settings;
		}
	}
}