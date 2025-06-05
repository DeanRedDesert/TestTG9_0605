using System;

namespace Game.Stages.Common.PreShow
{
	/// <summary>
	/// Used to darken controllers to darken parts of a scene for various game effects.
	/// </summary>
	public interface IDarkener
	{
		void Idle(bool immediate);
		void DarkenThenLighten(TimeSpan darkenLength);
	}
}