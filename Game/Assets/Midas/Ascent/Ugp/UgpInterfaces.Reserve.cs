using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpReserve reserve;
		private readonly ReserveParameters noReserveParameters = new ReserveParameters(false, false, 0, 0);

		/// <summary>
		/// Event raised when the Reserve parameters have changed.
		/// </summary>
		public event EventHandler<ReserveParametersChangedEventArgs> ReserveParametersChanged;

		private void InitReserve()
		{
			reserve = GameLib.GetInterface<IUgpReserve>();

			if (reserve != null)
				reserve.ReserveParametersChanged += OnReserveParametersChanged;
		}

		private void DeInitReserve()
		{
			if (reserve != null)
				reserve.ReserveParametersChanged -= OnReserveParametersChanged;

			reserve = null;
		}

		/// <summary>
		/// Gets the current reserve parameters.
		/// </summary>
		public ReserveParameters GetReserveParameters()
		{
			return reserve == null ? noReserveParameters : reserve.GetReserveParameters();
		}

		/// <summary>
		/// Sends the Reserve activation status to the foundation.
		/// </summary>
		/// <param name="isActive">The flag indicating whether Reserve is active or not.</param>
		public void SendReserveActivationChanged(bool isActive)
		{
			reserve?.SendActivationChanged(isActive);
		}

		private void OnReserveParametersChanged(object s, ReserveParametersChangedEventArgs e)
		{
			ReserveParametersChanged?.Invoke(s, e);
		}
	}
}