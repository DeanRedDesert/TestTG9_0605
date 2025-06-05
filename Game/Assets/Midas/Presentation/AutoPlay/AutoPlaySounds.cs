using Midas.Presentation.Audio;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Midas.Presentation.AutoPlay
{
	public sealed class AutoPlaySounds : MonoBehaviour
	{
		private AutoPlayState? lastState;

		[SerializeField]
		private PropertyReferenceValueType<AutoPlayState> statePropertyReference;

		[SerializeField]
		private SoundPlayerBase onSound;

		[SerializeField]
		private SoundPlayerBase offSound;

		private void OnEnable()
		{
			if (statePropertyReference == null)
				return;

			statePropertyReference.ValueChanged += OnValueChanged;
			PlaySoundForState();
		}

		private void OnDisable()
		{
			statePropertyReference.ValueChanged -= OnValueChanged;
			statePropertyReference.DeInit();
		}

		private void OnValueChanged(PropertyReference propertyRef, string path)
		{
			PlaySoundForState();
		}

		private void PlaySoundForState()
		{
			var newState = statePropertyReference.Value;

			if (lastState == null)
			{
				lastState = newState;
				return;
			}

			if (lastState == newState)
				return;

			switch (newState)
			{
				case AutoPlayState.Active:
					onSound.Play();
					offSound.Stop();
					break;

				case AutoPlayState.Idle:
					if (lastState != AutoPlayState.WaitPlayerConfirm && lastState != AutoPlayState.Starting)
					{
						offSound.Play();
						onSound.Stop();
					}

					break;
			}

			lastState = newState;
		}
	}
}