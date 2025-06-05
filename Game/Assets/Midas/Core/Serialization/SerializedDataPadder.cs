using System.IO;
using System.IO.Compression;
using System.Text;

namespace Midas.Core.Serialization
{
	internal static class SerializedDataPadder
	{
		private static byte[] paddingBuffer;

		public static void Pad(byte[] data, Stream targetStream, bool compressed, int padBytes)
		{
			if (paddingBuffer == null || paddingBuffer.Length < padBytes)
				paddingBuffer = new byte[padBytes];

			using var bw = new BinaryWriter(targetStream, Encoding.UTF8, true);

			if (compressed)
			{
				using var cms = new MemoryStream();
				using (var gzip = new GZipStream(cms, CompressionLevel.Fastest, true))
					gzip.Write(data, 0, data.Length);
				data = cms.ToArray();

				bw.Write((byte)1);
			}
			else
			{
				bw.Write((byte)0);
			}

			bw.Write(data);
			bw.Flush();

			var padding = (int)(padBytes - targetStream.Position % padBytes);

			if (padding != 0)
				targetStream.Write(paddingBuffer, 0, padding);
		}

		public static Stream Unpad(Stream data)
		{
			var compressed = data.ReadByte() == 1;
			return compressed ? new GZipStream(data, CompressionMode.Decompress, false) : data;
		}
	}
}