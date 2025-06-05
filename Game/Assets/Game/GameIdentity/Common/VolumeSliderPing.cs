using Midas.Presentation.Audio;
using Midas.Presentation.General;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	[RequireComponent(typeof(TouchRegion))]
	public sealed class VolumeSliderPing : MonoBehaviour
	{
		private TouchRegion touchRegion;
		private ISound sound;

		[SerializeField]
		private SoundId soundId;

		private void Awake()
		{
			touchRegion = GetComponent<TouchRegion>();
		}

		private void OnEnable()
		{
			if (sound == null)
				sound = AudioService.CreateSound(soundId);

			touchRegion.OnTouchUp += OnTouchUp;
		}

		private void OnDisable()
		{
			touchRegion.OnTouchUp -= OnTouchUp;
		}

		private void OnDestroy()
		{
			if (sound == null)
				return;

			sound.Destroy();
			AudioService.DestroySound(sound);
			sound = null;
		}

		private void OnTouchUp()
		{
			sound.Play();
		}
	}
}