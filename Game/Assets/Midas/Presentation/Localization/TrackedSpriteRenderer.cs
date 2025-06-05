using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Midas.Presentation.Localization
{
	/// <summary>
	/// Tracks and applies variant changes to a [SpriteRenderer]
	/// </summary>
	[Serializable]
	[DisplayName("SpriteRenderer")]
	[CustomTrackedObject(typeof(SpriteRenderer), false)]
	public class TrackedSpriteRenderer : TrackedObject
	{
		private const string SpriteProperty = "m_Sprite";

		private AsyncOperationHandle<Sprite> currentOperation;

		public override bool CanTrackProperty(string propertyPath) => propertyPath == "m_Sprite";

		public override AsyncOperationHandle ApplyLocale(Locale variantLocale, Locale defaultLocale)
		{
			if (TrackedProperties.Count == 0)
				return default;

			if (currentOperation.IsValid())
			{
				if (!currentOperation.IsDone)
					currentOperation.Completed -= SpriteOperationCompleted;
				LocalizationBridge.AddressablesInterfaceSafeRelease(currentOperation);
				currentOperation = default;
			}

			Debug.Assert(TrackedProperties.Count == 1, "Expected only 1 property to be tracked for a SpriteRenderer", Target);

			var property = TrackedProperties[0];

#if UNITY_EDITOR
			LocalizationBridge.VariantsPropertyDriverRegisterProperty(Target, property.PropertyPath);
#endif

			Debug.AssertFormat(property.PropertyPath == SpriteProperty, Target, "Expected tracked property {0} but it was {1}.", SpriteProperty, property.PropertyPath);

			switch (property)
			{
				case UnityObjectProperty objectProperty:
				{
					var fallbackIdentifier = defaultLocale != null ? defaultLocale.Identifier : default;
					if (objectProperty.GetValue(variantLocale.Identifier, fallbackIdentifier, out var spriteObject))
					{
						SetSprite(spriteObject as Sprite);
					}

					break;
				}
				case LocalizedAssetProperty localizedAssetProperty when !localizedAssetProperty.LocalizedObject.IsEmpty:
				{
					currentOperation = localizedAssetProperty.LocalizedObject.LoadAssetAsync<Sprite>();

					if (currentOperation.IsDone)
					{
						SpriteOperationCompleted(currentOperation);
					}
#if !UNITY_WEBGL // WebGL does not support WaitForCompletion
					else if (localizedAssetProperty.LocalizedObject.IsForceSynchronous())
					{
						currentOperation.WaitForCompletion();
						SpriteOperationCompleted(currentOperation);
					}
#endif
					else
					{
						currentOperation.Completed += SpriteOperationCompleted;
						return currentOperation;
					}

					break;
				}
			}

			return default;
		}

		private void SpriteOperationCompleted(AsyncOperationHandle<Sprite> assetOp) => SetSprite(assetOp.Result);
		private void SetSprite(Sprite sprite) => ((SpriteRenderer)Target).sprite = sprite;
	}
}