using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	public class TextAreaDimmer : MonoBehaviour
	{
		#region Constants

		private const float fadeTime = 0.1f;
		
		#endregion

		#region Fields

		private float t;
		private KeyValuePair<TMP_Text, Color>[] rendererColors;

		#endregion

		#region Public Methods

		/// <summary>
		/// Initialise the dimmer with a new set of renderers.
		/// </summary>
		/// <param name="textAreas">The new set of renderers.</param>
		public void Initialise(TMP_Text[] textAreas)
		{
			// Restore the old renderers if it was previously initialised.

			if (rendererColors != null)
			{
				foreach (var r in rendererColors)
				{
					if (r.Key)
						r.Key.color = r.Value;
				}
			}

			rendererColors = textAreas.Select(r => new KeyValuePair<TMP_Text, Color>(r, r.color)).ToArray();
			t = 1;
		}

		/// <summary>
		/// Set the dim state.
		/// </summary>
		/// <param name="dim">True to dim the renderers, false to undim the renderers.</param>
		public void SetDim(bool dim, bool immediate = false)
		{
			StopAllCoroutines();
			if (gameObject.activeInHierarchy && !immediate)
				StartCoroutine(dim ? DoDim() : DoUndim());
			else
			{
				foreach (var r in rendererColors)
					r.Key.color = dim ? r.Value * Color.gray : r.Value;

				t = dim ? 0 : 1;
			}
		}

		#endregion

		#region Private Methods

		private IEnumerator DoDim()
		{
			if (t <= 0)
				yield break;
			
			for (;;)
			{
				t -= Time.deltaTime / fadeTime;
				var c = Color.Lerp(Color.grey, Color.white, t);
				foreach (var r in rendererColors)
					r.Key.color = r.Value * c;

				if (t <= 0)
				{
					t = 0;
					yield break;
				}

				yield return null;
			}
		}

		private IEnumerator DoUndim()
		{
			if (t >= 1)
				yield break;

			for (;;)
			{
				t += Time.deltaTime / fadeTime;
				var c = Color.Lerp(Color.grey, Color.white, t);
				foreach (var r in rendererColors)
					r.Key.color = r.Value * c;

				if (t >= 1)
				{
					t = 1;
					yield break;
				}

				yield return null;
			}
		}

		#endregion
	}
}