using System;
using System.Collections.Generic;

namespace Midas.Presentation.Data.PropertyReference
{
	public interface IPropertyPathResolver
	{
		string Name { get; }

		IReadOnlyList<(string Name, Type PropertyType)> CollectProperties();

		PropertyBinding Resolve(string path);
	}
}