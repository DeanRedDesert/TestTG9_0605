// -----------------------------------------------------------------------
// <copyright file = "DenominationPlayableStatus.cs" company = "IGT">
//     Copyright (c) 2024 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	using System;
	using System.IO;
	using CompactSerialization;

	/// <summary>
	/// Represents the Denomination Playable Status.
	/// </summary>
	[Serializable]
	public class DenominationPlayableStatus : ICompactSerializable
	{
		#region Properties

		/// <summary>
		/// Gets the Denomination identifier.
		/// </summary>
		public long Denomination;

		/// <summary>
		/// Gets the Game Button Status identifier.
		/// </summary>
		public GameButtonStatus ButtonStatus;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a new <see cref="DenominationPlayableStatus"/>.
		/// </summary>
		/// This is a parameterless constructor used by <see cref="CompactSerializer" /> for serialization purposes.
		public DenominationPlayableStatus()
		{
		}

		/// <summary>
		/// Instantiates a new <see cref="DenominationPlayableStatus"/>.
		/// </summary>
		/// <param name="denomination">
		/// The denomination identifier.
		/// </param>
		/// <param name="gameButtonStatus">
		/// The gameButtonStyle identifier.
		/// </param>
		public DenominationPlayableStatus(long denomination, GameButtonStatus gameButtonStatus)
		{
			Denomination = denomination;
			ButtonStatus = gameButtonStatus;
		}

		#endregion Constructors

		#region ICompactSerializable

		public void Serialize(Stream stream)
		{
			CompactSerializer.Write(stream, Denomination);
			CompactSerializer.Write(stream, ButtonStatus);
		}

		public void Deserialize(Stream stream)
		{
			Denomination = CompactSerializer.ReadLong(stream);
			ButtonStatus = CompactSerializer.ReadEnum<GameButtonStatus>(stream);
		}

		#endregion
	}
}
