using System.Collections.Generic;
using Midas.Presentation.Game;
using UnityEngine;

namespace Midas.Presentation.Info
{
	public sealed class RulesController : MonoBehaviour, IRulesController
	{
		private InfoController infoController;

		[SerializeField]
		public PageController pageController;

		public IReadOnlyList<RulesPageType> Setup() => pageController.EnabledPages();

		public void Awake()
		{
			infoController = GameBase.GameInstance.GetPresentationController<InfoController>();
			infoController.RegisterRulesController(this);
		}

		private void OnDestroy()
		{
			infoController.RegisterRulesController(null);
			infoController = null;
		}
	}
}