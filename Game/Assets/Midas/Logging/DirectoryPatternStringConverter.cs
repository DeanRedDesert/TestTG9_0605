using System;
using log4net.Util;
using log4net.Util.TypeConverters;

namespace Midas.Logging
{
	public sealed class DirectoryPatternStringConverter : IConvertTo, IConvertFrom
	{
		public bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public bool CanConvertTo(Type targetType)
		{
			return typeof(string).IsAssignableFrom(targetType);
		}

		public object ConvertFrom(object source)
		{
			if (!(source is string pattern))
			{
				throw ConversionNotSupportedException.Create(typeof(DirectoryPatternString), source);
			}

			return new DirectoryPatternString(pattern);
		}

		public object ConvertTo(object source, Type targetType)
		{
			if (!(source is PatternString pattern) || !CanConvertTo(targetType))
			{
				throw ConversionNotSupportedException.Create(targetType, source);
			}

			return pattern.Format();
		}
	}
}