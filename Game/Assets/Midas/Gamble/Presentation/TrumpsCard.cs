using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Gamble.LogicToPresentation;
using UnityEngine;

namespace Midas.Gamble.Presentation
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class TrumpsCard : MonoBehaviour
	{
		private SpriteRenderer spriteRenderer;
		private Sprite[] lookup;

		[SerializeField]
		private Sprite cardBack;

		[SerializeField]
		private CardFace[] cardFaces;

		public void Initialise(TrumpsSuit? suit)
		{
			if (spriteRenderer == null)
			{
				spriteRenderer = GetComponent<SpriteRenderer>();
				lookup = new Sprite[Enum.GetValues(typeof(TrumpsSuit)).Length];

				foreach (var cf in cardFaces)
					lookup[(int)cf.suit] = cf.sprite;
			}

			var sprite = cardBack;

			if (suit.HasValue)
				sprite = lookup[(int)suit];

			spriteRenderer.sprite = sprite;
		}

		public IEnumerator<CoroutineInstruction> Reveal(TrumpsSuit suit)
		{
			spriteRenderer.sprite = lookup[(int)suit];
			yield break;
		}

		public IEnumerator<CoroutineInstruction> Hide()
		{
			spriteRenderer.sprite = cardBack;
			yield break;
		}
	}
}