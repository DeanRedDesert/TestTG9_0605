using System;
using System.IO;
using log4net.Util;

namespace Midas.Logging
{
	public sealed class DirectoryPatternConverter : PatternConverter
	{
		protected override void Convert(TextWriter writer, object state)
		{
			switch (Option)
			{
				case "logDirectory":
				{
					writer.Write(Factory.LogDirectory);
					break;
				}

				default:
				{
					Console.Error.WriteLine($"DirectoryPatternConverter does not know how to handle option: {Option}");
					break;
				}
			}
		}
	}
}