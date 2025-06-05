using System;

namespace Midas.Presentation.Data
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class ExpressionAttribute : Attribute
	{
		public string Category { get; }

		public ExpressionAttribute(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				Category = "Uncategorized";
				return;
			}

			Category = category.Trim().Replace(".", "");
		}
	}
}