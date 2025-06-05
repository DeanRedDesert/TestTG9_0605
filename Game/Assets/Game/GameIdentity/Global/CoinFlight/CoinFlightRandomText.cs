using UnityEngine;

namespace Game.GameIdentity.Global.CoinFlight
{
	public sealed class CoinFlightRandomText : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] textList;

		public void TextActivate()
		{
			textList[Random.Range(0, textList.Length)].SetActive(true);
		}

		public void TextDeactivate()
		{
			foreach (var item in textList)
			{
				item.SetActive(false);
			}
		}
	}
}