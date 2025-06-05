// -----------------------------------------------------------------------
// <copyright file = "GameButtonBehavior.cs" company = "IGT">
//     Copyright (c) 2024 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	using System;
	using System.IO;
	using CompactSerialization;

	/// <summary>
	/// Represents the game button behavior.
	/// </summary>
	[Serializable]
	public class GameButtonBehavior : ICompactSerializable
	{
		#region Properties

		/// <summary>
		/// Gets the Game Button type identifier.
		/// </summary>
		public GameButtonTypeEnum ButtonType;

		/// <summary>
		/// Gets the Game Button Status identifier.
		/// </summary>
		public GameButtonStatus ButtonStatus;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a new <see cref="GameButtonBehavior"/>.
		/// </summary>
		/// This is a parameterless constructor used by <see cref="CompactSerializer" /> for serialization purposes.
		public GameButtonBehavior()
		{

		}

		/// <summary>
		/// Instantiates a new <see cref="GameButtonBehavior"/>.
		/// </summary>
		/// <param name="gameButtonTypeEnum">
		/// The gameButtonTypeEnum identifier.
		/// </param>
		/// <param name="gameButtonStatus">
		/// The gameButtonStatus identifier.
		/// </param>
		public GameButtonBehavior(GameButtonTypeEnum gameButtonTypeEnum, GameButtonStatus gameButtonStatus)
		{
			ButtonType = gameButtonTypeEnum;
			ButtonStatus = gameButtonStatus;
		}

		#endregion

		#region ICompactSerializable

		public void Serialize(Stream stream)
		{
			CompactSerializer.Write(stream, ButtonType);
			CompactSerializer.Write(stream, ButtonStatus);
		}

		public void Deserialize(Stream stream)
		{
			ButtonType = CompactSerializer.ReadEnum<GameButtonTypeEnum>(stream);
			ButtonStatus = CompactSerializer.ReadEnum<GameButtonStatus>(stream);
		}

		#endregion
	}
}
