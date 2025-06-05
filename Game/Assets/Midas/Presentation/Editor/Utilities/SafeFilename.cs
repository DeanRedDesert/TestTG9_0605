using System.IO;

namespace Midas.Presentation.Editor.Utilities
{
	/// <summary>
	/// Used to create a filename that does not clash with another filename in the project.
	/// </summary>
	public static class SafeFilename
	{
		/// <summary>
		/// Creates a safe filename that won't class with an existing file.
		/// </summary>
		/// <param name="filename">The filename to try and use.</param>
		/// <returns>Either the filename, or if the file already exists a name with a number suffix.</returns>
		public static string GetSafeFilename(string filename)
		{
			return GetSafeFilename(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename), Path.GetExtension(filename));
		}

		/// <summary>
		/// Creates a safe filename that won't class with an existing file.
		/// </summary>
		/// <param name="destinationPath">The directory to place the new file.</param>
		/// <param name="baseFilename">The base filename, without extension.</param>
		/// <param name="extension">The filename extension.</param>
		/// <returns>Either the filename, or if the file already exists a name with a number suffix.</returns>
		public static string GetSafeFilename(string destinationPath, string baseFilename, string extension)
		{
			var newControllerPath = Path.Combine(destinationPath, baseFilename + extension);
			if (!File.Exists(newControllerPath))
				return newControllerPath;

			var index = GetFilenameNumber(ref baseFilename);

			do
			{
				index++;
				newControllerPath = Path.Combine(destinationPath, $"{baseFilename}{index}{extension}");
			} while (File.Exists(newControllerPath));

			return newControllerPath;
		}

		private static int GetFilenameNumber(ref string filename)
		{
			var index = 0;
			int filenameNumber = 0;

			while (index < filename.Length && (!char.IsDigit(filename[index]) || !int.TryParse(filename.Substring(index), out filenameNumber)))
				index++;

			if (index == filename.Length)
			{
				filename += " ";
				return 0;
			}

			filename = filename.Substring(0, index);
			return filenameNumber;
		}
	}
}