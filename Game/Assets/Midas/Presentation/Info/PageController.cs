using System.Collections.Generic;
using System.Linq;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Info.Page;
using UnityEngine;

namespace Midas.Presentation.Info
{
	public sealed class PageController : MonoBehaviour
	{
		private bool init;
		private RulesPage[] pages;
		private List<RulesPage> enabledPages;
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		[SerializeField]
		private PageControllerSettings settings;

		[SerializeField]
		private RulesPage[] corePages;

		public void OnEnable() => autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.InfoStatus, nameof(InfoStatus.CurrentRulesPage), OnPageChangeHandler);

		public void OnDisable() => autoUnregisterHelper.UnRegisterAll();

		public IReadOnlyList<RulesPageType> EnabledPages()
		{
			if (!init)
			{
				GeneratePages(this, settings.FolderName);
				init = true;
			}

			enabledPages = new List<RulesPage>();
			foreach (var page in pages)
			{
				page.gameObject.SetActive(false);
				if (page.CanEnable())
					enabledPages.Add(page);
			}

			foreach (var page in corePages)
			{
				page.gameObject.SetActive(false);
				if (page.CanEnable())
					enabledPages.Add(page);
			}

			enabledPages.First().gameObject.SetActive(true);
			return enabledPages.Select(e => e.PageType).ToList();
		}

		private void OnPageChangeHandler(StatusBlock sender, string propertyName)
		{
			var pageIndex = 0;
			for (var i = 0; i < enabledPages.Count; i++)
			{
				if (!enabledPages[i].CanEnable())
				{
					enabledPages[i].gameObject.SetActive(false);
					continue;
				}

				enabledPages[i].gameObject.SetActive(pageIndex == StatusDatabase.InfoStatus.CurrentRulesPage);
				pageIndex++;
			}
		}

		private void GeneratePages(PageController pageController, string folder)
		{
			var sprites = Resources.LoadAll<Sprite>($"{folder.Replace(@"\", "/")}");

			var gameRulesParent = pageController.gameObject.transform.Find("Game");

			var rules = new List<RulesPage>();
			foreach (var sprite in sprites)
			{
				var go = new GameObject(sprite.name.Replace($"Assets/{folder.Replace(@"\", "/")}/", string.Empty).Replace(".png", string.Empty));
				var srp = go.AddComponent<StandardRulesPage>();
				var sr = go.AddComponent<SpriteRenderer>();
				sr.sprite = sprite;
				srp.spriteRenderer = sr;
				go.transform.SetParent(gameRulesParent.gameObject.transform);
				go.transform.localPosition = new Vector3(0, 0, -.2f);
				go.gameObject.layer = gameRulesParent.gameObject.layer;

				if (settings.IsProgressivePage(go.name))
					settings.CreateProgressiveCeiling(go.transform);

				rules.Add(srp);
			}

			pages = rules.ToArray();
		}
	}
}