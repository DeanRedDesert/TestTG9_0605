using System.Collections.Generic;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class LanguagePanelLayout : MonoBehaviour
	{
		private SpriteRenderer spriteRenderer;
		private float bottom;
		private bool refreshRequired;

		[SerializeField]
		private GameObject title;

		[SerializeField]
		private float titleHeight;

		[SerializeField]
		private float buttonHeight;

		[SerializeField]
		private float buttonGap;

		[SerializeField]
		private float footerHeight;

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			bottom = transform.localPosition.y - spriteRenderer.size.y / 2f;
		}

		public void Refresh()
		{
			refreshRequired = true;
		}

		private void OnEnable()
		{
			refreshRequired = true;
		}

		private void LateUpdate()
		{
			if (!refreshRequired)
				return;

			if (StatusDatabase.ConfigurationStatus.LanguageConfig == null)
				return;

			var count = transform.childCount;
			var activeButtons = new List<Transform>();

			for (var i = 0; i < count; i++)
			{
				var child = transform.GetChild(i);

				if (!child.gameObject.activeInHierarchy || child.gameObject == title)
					continue;

				activeButtons.Add(child);
			}

			var h = titleHeight + activeButtons.Count * (buttonHeight + buttonGap) + buttonGap + footerHeight;
			transform.SetLocalPosY(bottom + h / 2f);

			spriteRenderer.size = new Vector2(spriteRenderer.size.x, h);

			title.transform.SetLocalPosY(h / 2f - titleHeight / 2f);

			var y = h / 2 - (titleHeight + buttonGap + buttonHeight / 2f);

			foreach (var b in activeButtons)
			{
				b.transform.SetLocalPosY(y);
				y -= buttonHeight + buttonGap;
			}

			refreshRequired = false;
		}
	}
}