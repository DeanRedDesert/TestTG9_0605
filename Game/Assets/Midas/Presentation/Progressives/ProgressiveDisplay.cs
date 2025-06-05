using Midas.Core.General;
using Midas.Presentation.Game;
using Midas.Presentation.Meters;
using UnityEngine;

namespace Midas.Presentation.Progressives
{
	[RequireComponent(typeof(MoneyMeter))]
	public sealed class ProgressiveDisplay : MonoBehaviour, IProgressiveDisplay
	{
		private MoneyMeter meter;

		[SerializeField]
		private string levelId;

		private void Awake()
		{
			meter = GetComponent<MoneyMeter>();
		}

		private void OnEnable()
		{
			if (meter == null)
				return;

			GameBase.GameInstance.GetPresentationController<ProgressiveDisplayController>().RegisterProgressiveDisplay(this, levelId);
		}

		private void OnDisable()
		{
			GameBase.GameInstance.GetPresentationController<ProgressiveDisplayController>().UnRegisterProgressiveDisplay(this);
		}

		public void SetValue(string progLevelId, Money? value)
		{
			if (levelId == progLevelId && value.HasValue)
				meter.SetValue(value.Value);
		}

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(string progLevelId)
		{
			levelId = progLevelId;
		}
#endif

		#endregion.
	}
}