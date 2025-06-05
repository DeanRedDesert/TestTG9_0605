using System;

namespace Midas.Presentation.ButtonHandling
{
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public sealed class ButtonFunctionsAttribute : Attribute
	{
		public string Group { get; } = string.Empty;

		public ButtonFunctionsAttribute() { }

		public ButtonFunctionsAttribute(string groupName)
		{
			Group = groupName;
		}
	}
}