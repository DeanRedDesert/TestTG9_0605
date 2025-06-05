using System;
using Midas.Presentation.Audio;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

namespace Midas.Presentation.Localization
{
	[Serializable]
	[System.ComponentModel.DisplayName("Sound Player")]
	[CustomTrackedObject(typeof(SoundPlayer), false)]
	public class TrackedSoundPlayer : JsonSerializerTrackedObject
	{
		public override ITrackedProperty CreateCustomTrackedProperty(string propertyPath) => propertyPath.StartsWith("soundId") ? new StringTrackedProperty { PropertyPath = propertyPath } : null;
	}
}