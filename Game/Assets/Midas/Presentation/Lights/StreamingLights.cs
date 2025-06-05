using Midas.Presentation.Cabinet;
using UnityEngine;

namespace Midas.Presentation.Lights
{
	[CreateAssetMenu(menuName = "Midas/Lights/Streaming Lights")]
	public sealed class StreamingLights : LightsBase, IStreamingLights
	{
		[SerializeField]
		private string choreographyId;

		public string ChoreographyId => choreographyId;

		public override LightsHandle Register()
		{
			return CabinetManager.Cabinet.AddLights(this);
		}
	}
}