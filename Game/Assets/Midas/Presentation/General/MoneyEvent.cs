using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.General
{
	public sealed class MoneyEvent : MonoBehaviour
	{
		private readonly AutoUnregisterHelper unregisterHelper = new AutoUnregisterHelper();

		[SerializeField]
		private UnityEvent onMoneyIn;

		[SerializeField]
		private UnityEvent onMoneyOut;

		private void Awake()
		{
			unregisterHelper.RegisterMessageHandler<MoneyInMessage>(Communication.PresentationDispatcher, OnMoneyIn);
			unregisterHelper.RegisterMessageHandler<MoneyOutMessage>(Communication.PresentationDispatcher, OnMoneyOut);
		}

		private void OnDestroy()
		{
			unregisterHelper.UnRegisterAll();
		}

		private void OnMoneyIn(MoneyInMessage msg)
		{
			onMoneyIn?.Invoke();
		}

		private void OnMoneyOut(MoneyOutMessage msg)
		{
			onMoneyOut?.Invoke();
		}
	}
}