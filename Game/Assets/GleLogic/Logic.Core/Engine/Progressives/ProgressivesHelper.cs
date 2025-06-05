using System;
using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Engine.Progressives
{
	/// <summary>
	///  A helper for functionality associated progressive levels.
	/// </summary>
	public static class ProgressivesHelper
	{
		public static IReadOnlyList<ProgressiveLevel> GetProgressiveLevels(IReadOnlyList<Input> inputs, IReadOnlyList<ProgressiveSet> progressiveSets)
		{
			foreach (var progressiveSet in progressiveSets)
			{
				var setFound = false;

				foreach (var setInputSet in progressiveSet.InputSets)
				{
					var setInputSetFound = true;
					foreach (var setInput in setInputSet)
					{
						var inputFound = false;
						var inputSame = false;

						foreach (var input in inputs)
						{
							if (setInput.Name == input.Name)
							{
								inputFound = true;
								inputSame = setInput.ToStringOrThrow("SL") == input.ToStringOrThrow("SL");
								break;
							}
						}

						if (inputFound && !inputSame)
						{
							setInputSetFound = false;
							break;
						}
					}

					if (setInputSetFound)
					{
						setFound = true;
						break;
					}
				}

				if (setFound)
					return progressiveSet.ProgressiveLevels;
			}

			return Array.Empty<ProgressiveLevel>();
		}
	}
}