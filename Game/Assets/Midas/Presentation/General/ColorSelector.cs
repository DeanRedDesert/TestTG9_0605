using UnityEngine;

namespace Midas.Presentation.General
{
	public abstract class ColorSelector : ScriptableObject
	{
		public abstract Color GetColor(int index);
	}
}