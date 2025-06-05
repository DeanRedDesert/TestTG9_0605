using System.Collections.Generic;

namespace Gaff.Core
{
	public sealed class GaffSequence
	{
		public string Title { get; }
		public bool DeveloperOnly { get; }
		public IReadOnlyList<GaffStep> OrderedSteps { get; }

		public GaffSequence(string title, bool developerOnly, IReadOnlyList<GaffStep> orderedSteps)
		{
			Title = title;
			DeveloperOnly = developerOnly;
			OrderedSteps = orderedSteps;
		}

		public GaffSequence WithSteps(IReadOnlyList<GaffStep> orderedSteps)
			=> new GaffSequence(Title, DeveloperOnly, orderedSteps);
	}
}