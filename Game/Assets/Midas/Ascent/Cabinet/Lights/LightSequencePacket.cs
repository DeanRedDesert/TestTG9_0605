using System.Runtime.InteropServices;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;

namespace Midas.Ascent.Cabinet.Lights
{
	public enum LightSequenceLoopingBehaviour
	{
		Unknown = 0,
		Looping = 1,
		NonLooping = 2
	}

	/// <summary>
	/// This struct is used to alternatively cache the streaming light hardware and light sequence object,
	/// or the light sequence file name within one slot in a dictionary.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal readonly struct LightSequencePacket
	{
		#region Public

		/// <summary>
		/// Construct the instance with the hardware type and the light sequence object.
		/// </summary>
		/// <param name="hardware">The hardware type of the streaming light device.</param>
		/// <param name="sequence">This object holds the light sequence data to play.</param>
		/// <param name="originalLightSequenceSequenceLooping">The original LightSequence.Loop value</param>
		public LightSequencePacket(StreamingLightHardware hardware, LightSequence sequence, LightSequenceLoopingBehaviour originalLightSequenceSequenceLooping)
		{
			sequenceFileName = null;
			fileNameBit = 0;
			this.hardware = hardware;
			this.sequence = sequence;
			originalLightSequenceLoopingBehaviour = originalLightSequenceSequenceLooping;
		}

		/// <summary>
		/// Construct the instance with the light sequence file name.
		/// </summary>
		/// <param name="sequenceFileName">The light sequence file name.</param>
		public LightSequencePacket(string sequenceFileName)
		{
			hardware = StreamingLightHardware.Unknown;
			fileNameBit = FileNameBitMask;
			sequence = null;
			this.sequenceFileName = sequenceFileName;
			originalLightSequenceLoopingBehaviour = LightSequenceLoopingBehaviour.Unknown;
		}

		/// <summary>
		/// Gets whether the current packet represents a light sequence file name.
		/// If false, the current packet represents the streaming light sequence data, which contains
		/// the hardware information and the sequence data object.
		/// </summary>
		public bool IsSequenceFileName => (fileNameBit & FileNameBitMask) == FileNameBitMask;

		/// <summary>
		/// Gets the hardware type of the streaming light device.
		/// </summary>
		public StreamingLightHardware Hardware => IsSequenceFileName ? StreamingLightHardware.Unknown : hardware;

		/// <summary>
		/// Gets the sequence file name.
		/// </summary>
		public string SequenceFileName => IsSequenceFileName ? sequenceFileName : null;

		/// <summary>
		/// Gets the light sequence object which holds the sequence data.
		/// </summary>
		public LightSequence Sequence => IsSequenceFileName ? null : sequence;

		/// <summary>
		/// The original LightSequence.Loop value
		/// </summary>
		// ReSharper disable once ConvertToAutoProperty
		public LightSequenceLoopingBehaviour OriginalLightSequenceLoopingBehaviour => originalLightSequenceLoopingBehaviour;

		#endregion

		#region Private

		/// <summary>
		/// This bit mask is used to check the value of <see cref="fileNameBit" />.
		/// </summary>
		private const uint FileNameBitMask = 0x80000000;

		/// <summary>
		/// True indicates this packet represents a sequence file name.
		/// </summary>
		/// <remarks>
		/// Based on the fact that the top 1 bit is not used by <see cref="StreamingLightHardware" />,
		/// we reuse this bit to indicates the content type of this packet.
		/// </remarks>
		[FieldOffset(0)]
		private readonly uint fileNameBit;

		/// <summary>
		/// The hardware type of the streaming light device.
		/// </summary>
		[FieldOffset(0)]
		private readonly StreamingLightHardware hardware;

		/// <summary>
		/// The original LightSequence.Loop value
		/// </summary>
		[FieldOffset(8)]
		private readonly LightSequenceLoopingBehaviour originalLightSequenceLoopingBehaviour;

		/// <summary>
		/// The sequence file name.
		/// </summary>
		[FieldOffset(16)]
		private readonly string sequenceFileName;

		/// <summary>
		/// This object holds the light sequence data to play.
		/// </summary>
		[FieldOffset(16)]
		private readonly LightSequence sequence;

		#endregion
	}
}