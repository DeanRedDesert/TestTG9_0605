using System;
using System.Linq;
using System.Text;
using IGT.Game.Fuel.Data.Translation;

namespace Midas.Fuel.Editor
{
	public static class FuelDbText
	{
		/// <summary>
		/// Convert fuel translation into plain text.
		/// </summary>
		/// <param name="translation">Translation to convert.</param>
		/// <returns>Plain text representation of passed translation.</returns>
		public static string ToPlainText(this Translation translation)
		{
			var plainText = new StringBuilder();

			foreach (var line in translation.Lines)
			{
				// Append new line if this is not the first line.
				if (translation.Lines.IndexOf(line) > 0)
				{
					plainText.Append("\n");
				}

				foreach (var fuelTextItem in line.Items)
				{
					switch (fuelTextItem)
					{
						case FontTranslationItem item:
						{
							var fontItem = item;
							// Ignore the fontItem's id field because plain text cannot have changing fonts.
							plainText.Append(fontItem.Text);
							break;
						}
						case ImageTranslationItem item:
						{
							var imgItem = item;
							plainText.Append("[IMG");
							plainText.Append(imgItem.Id);
							plainText.Append("]");
							break;
						}
						default:
							throw new InvalidOperationException("Unknown Fuel Text Item Type. " + fuelTextItem.GetType());
					}
				}
			}

			return plainText.ToString();
		}

		/// <summary>
		/// Convert from plain text to a <see cref="Translation"/> object.
		/// </summary>
		/// <param name="text">Text to translate.</param>
		/// <returns><see cref="Translation"/> object representing the plain text.</returns>
		public static Translation ToFuelTranslation(this string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new Translation();
			}

			var dbText = new Translation();

			// Since plain text can contain line breaks, need to split them into separate fuel text lines.
			var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var currentFont = new FontTranslationItem
				{
					// There can only be one font type for this kind of text.
					Id = "0",
					Text = lines[lineIndex]
				};

				var currentLine = new TranslationLine();
				currentLine.Items.Add(currentFont);
				dbText.Lines.Add(currentLine);
			}

			return dbText;
		}
	}
}