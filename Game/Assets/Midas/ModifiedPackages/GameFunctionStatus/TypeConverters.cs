// -----------------------------------------------------------------------
// <copyright file = "TypeConverters.cs" company = "IGT">
//     Copyright (c) 2024 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	using F2XGameButtonBehaviorType = F2X.Schemas.Internal.GameFunctionStatus.GameButtonBehaviorType;
	using F2XDenominationPlayableStatusType = F2X.Schemas.Internal.GameFunctionStatus.DenominationPlayableStatusType;

	/// <summary>
	/// A class that contains static extension methods to convert between public and internal F2X schema types.
	/// </summary>
	internal static class TypeConverters
	{
		/// <summary>
		/// Converts the F2X <see cref="F2XGameButtonBehaviorType"/> schema type to the public
		/// <see cref="GameButtonBehavior"/> type.
		/// </summary>
		/// <param name="gameButtonBehaviorType">
		/// The value to convert.
		/// </param>
		/// <returns> 
		/// The conversion result.
		/// </returns>
		public static GameButtonBehavior ToPublic(this F2XGameButtonBehaviorType gameButtonBehaviorType)
		{
			return gameButtonBehaviorType == null
				? null
				: new GameButtonBehavior((GameButtonTypeEnum)gameButtonBehaviorType.GameButton,
					(GameButtonStatus)gameButtonBehaviorType.GameButtonStatus);
		}

		/// <summary>
		/// Converts the F2X <see cref="F2XDenominationPlayableStatusType"/> schema type to the public
		/// <see cref="DenominationPlayableStatus"/> type.
		/// </summary>
		/// <param name="denominationPlayableStatusType">
		/// The value to convert.
		/// </param>
		/// <returns> 
		/// The conversion result.
		/// </returns>
		public static DenominationPlayableStatus ToPublic(this F2XDenominationPlayableStatusType denominationPlayableStatusType)
		{
			return denominationPlayableStatusType == null
				? null
				: new DenominationPlayableStatus(denominationPlayableStatusType.Denomination,
					(GameButtonStatus)denominationPlayableStatusType.DenominationButtonStatus);
		}
	}
}
