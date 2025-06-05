using System;

namespace Midas.Presentation.StageHandling
{
	/// <summary>
	///     Attribute for marking classes where to search for stages
	///     which should be shown in the unity editor
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public sealed class StagesAttribute : Attribute
	{
		#region Public

		public StagesAttribute(string groupName)
		{
			Group = groupName;
		}

		public StagesAttribute() { }
		public string Group { get; } = string.Empty;

		#endregion
	}
}