using System.Collections.Generic;

namespace Midas.Presentation.ButtonHandling
{
	public interface IButtonControllerOwner
	{
		IReadOnlyList<IButtonController> ButtonControllers { get; }
	}
}