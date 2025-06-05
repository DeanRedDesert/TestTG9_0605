using System;

namespace Midas.Tools.Editor
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class InitForEditorAttribute : Attribute
	{
		#region Public

		public InitForEditorAttribute(int priority)
		{
			Priority = priority;
		}

		public int Priority { get; }

		#endregion
	}
}