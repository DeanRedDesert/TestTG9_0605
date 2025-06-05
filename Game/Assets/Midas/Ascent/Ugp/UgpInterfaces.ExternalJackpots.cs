using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpExternalJackpots externalJackpots;
		private readonly ExternalJackpots noStandaloneJackpotsValue = new ExternalJackpots();

		/// <summary>
		/// Event rasied when the external jackpot has changed.
		/// </summary>
		public event EventHandler<ExternalJackpotChangedEventArgs> ExternalJackpotChanged;

		private void InitExternalJackpots()
		{
			externalJackpots = GameLib.GetInterface<IUgpExternalJackpots>();
			if (externalJackpots != null)
			{
				externalJackpots.ExternalJackpotChanged += OnExternalJackpotChanged;
			}
		}

		private void DeInitExternalJackpots()
		{
			if (externalJackpots != null)
				externalJackpots.ExternalJackpotChanged -= OnExternalJackpotChanged;

			externalJackpots = null;
		}

		/// <summary>
		/// Gets the data for the external jackpots.
		/// </summary>
		public ExternalJackpots GetExternalJackpots()
		{
			return externalJackpots == null ? noStandaloneJackpotsValue : externalJackpots.GetExternalJackpots();
		}

		private void OnExternalJackpotChanged(object s, ExternalJackpotChangedEventArgs e)
		{
			ExternalJackpotChanged?.Invoke(s, e);
		}
	}
}