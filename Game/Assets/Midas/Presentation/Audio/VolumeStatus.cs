using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;

namespace Midas.Presentation.Audio
{
	public sealed class VolumeStatus : StatusBlock
	{
		private StatusProperty<VolumeConfig> volumeConfig;
		private StatusProperty<float> gameVolumeAttenuation;

		public VolumeConfig VolumeConfig
		{
			get => volumeConfig.Value;
			set => volumeConfig.Value = value;
		}

		/// <summary>
		/// The game volume attenuation in dB.
		/// </summary>
		public float GameVolumeAttenuation
		{
			get => gameVolumeAttenuation.Value;
			set => gameVolumeAttenuation.Value = value;
		}

		public VolumeStatus() : base(nameof(VolumeStatus))
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			volumeConfig = AddProperty<VolumeConfig>(nameof(VolumeConfig), null);
			gameVolumeAttenuation = AddProperty<float>(nameof(GameVolumeAttenuation), 0);
		}
	}
}