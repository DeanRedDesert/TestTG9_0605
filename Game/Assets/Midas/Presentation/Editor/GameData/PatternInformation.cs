using System.Collections.Generic;

namespace Midas.Presentation.Editor.GameData
{
	public sealed class LinePatternInformation
	{
		public string Name { get; }
		public IReadOnlyList<LineDefinition> Lines { get; }

		public LinePatternInformation(string name, IReadOnlyList<LineDefinition> lines)
		{
			Name = name;
			Lines = lines;
		}
	}

	public sealed class LineDefinition
	{
		public string Name { get; }
		public IReadOnlyList<(int Column, int Row)> Cells { get; }

		public LineDefinition(string name, IReadOnlyList<(int Column, int Row)> cells)
		{
			Name = name;
			Cells = cells;
		}
	}
}