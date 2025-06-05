using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;
using UnityEngine;

namespace Midas.Tools.Editor.AssetOptimizer
{
	public static class PngInspector
	{
		public static (long DataSize, long MetaSize, IReadOnlyList<string> MetaText) GetMetadataSize(string filename)
		{
			long metadataSize = 0;
			long pngDataSize = 0;
			var metaText = new List<string>();

			try
			{
				using var inputFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

				ReadPngSignature(inputFileStream);

				while (inputFileStream.Position < inputFileStream.Length)
				{
					ReadChunkLength(inputFileStream, out var length);
					ReadChunkType(inputFileStream, out var chunkType);

					var chunkSize = length + 4;

					if (chunkType == "tEXt" || chunkType == "iTXt" || chunkType == "zTXt")
					{
						metadataSize += chunkSize;
						var dataBytes = new byte[length];
						ReadChunkBytes(length, inputFileStream, (startIndex, bytes, bytesRead) => Array.Copy(bytes, 0, dataBytes, startIndex, bytesRead));
						ReadCrc(inputFileStream);

						switch (chunkType)
						{
							case "iTXt":
							{
								var d = dataBytes.Where(b => b != 0).ToArray();
								var iTxtData = Encoding.UTF8.GetString(d, 0, d.Length);
								metaText.Add(iTxtData);
								break;
							}
							case "zTXt":
							{
								var nullIndex = Array.IndexOf(dataBytes, (byte)0);
								var compressionMethod = dataBytes[nullIndex + 1];
								var compressedData = new byte[dataBytes.Length - nullIndex - 2];
								Array.Copy(dataBytes, nullIndex + 2, compressedData, 0, compressedData.Length);

								if (compressionMethod == 0)
								{
									// Decompress the data
									using var compressedStream = new MemoryStream(compressedData);
									using var zlibStream = new ZlibStream(compressedStream, CompressionMode.Decompress);
									using var resultStream = new MemoryStream();
									zlibStream.CopyTo(resultStream);
									var text = Encoding.UTF8.GetString(resultStream.ToArray());
									metaText.Add(text);
								}

								break;
							}
						}
					}
					else
					{
						pngDataSize += chunkSize;
						inputFileStream.Seek(length + 4, SeekOrigin.Current);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			return (pngDataSize, metadataSize, metaText);
		}

		public static void RemoveMetadata(string filename)
		{
			var destFileName = Path.GetTempFileName();
			try
			{
				using var inputFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
				using var outputFileStream = new FileStream(destFileName, FileMode.Open, FileAccess.Write);

				var pngSignature = ReadPngSignature(inputFileStream);
				outputFileStream.Write(pngSignature, 0, pngSignature.Length);

				while (inputFileStream.Position < inputFileStream.Length)
				{
					var lengthBytes = ReadChunkLength(inputFileStream, out var length);
					var typeBytes = ReadChunkType(inputFileStream, out var chunkType);

					var chunkSize = length + 4;

					// Write the chunk to the output file if it's not a metadata chunk
					if (chunkType == "tEXt" || chunkType == "iTXt" || chunkType == "zTXt")
					{
						inputFileStream.Seek(chunkSize, SeekOrigin.Current);
					}
					else
					{
						outputFileStream.Write(lengthBytes, 0, lengthBytes.Length);
						outputFileStream.Write(typeBytes, 0, typeBytes.Length);

						// ReSharper disable once AccessToDisposedClosure
						ReadChunkBytes(length, inputFileStream, (startIndex, bytes, bytesRead) => outputFileStream.Write(bytes, 0, bytesRead));
						var crc = ReadCrc(inputFileStream);
						outputFileStream.Write(crc, 0, crc.Length);
					}
				}

				outputFileStream.Flush();
				outputFileStream.Close();
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not parse {filename}.");
				Debug.LogError(e);
			}

			try
			{
				var gameFolder = Directory.GetParent(Application.dataPath);
				// ReSharper disable once PossibleNullReferenceException
				File.Copy(destFileName, Path.Combine(gameFolder.FullName, filename), true);
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not update {filename}.");
				Debug.LogError(e);
			}
		}

		private static byte[] ReadPngSignature(Stream inputFileStream)
		{
			var pngSignature = new byte[8];
			// ReSharper disable once MustUseReturnValue
			inputFileStream.Read(pngSignature, 0, 8);
			return pngSignature;
		}

		private static byte[] ReadChunkType(Stream inputFileStream, out string chunkType)
		{
			var typeBytes = new byte[4];
			// ReSharper disable once MustUseReturnValue
			inputFileStream.Read(typeBytes, 0, 4);
			chunkType = Encoding.ASCII.GetString(typeBytes);
			return typeBytes;
		}

		private static void ReadChunkBytes(uint length, Stream inputFileStream, Action<long, byte[], int> dataRead)
		{
			const int bufferSize = 1024 * 1024;
			var buffer = new byte[bufferSize];
			long totalBytesRead = length;
			long index = 0;
			while (totalBytesRead > 0)
			{
				int size;
				if (totalBytesRead > bufferSize)
					size = bufferSize;
				else
					size = (int)totalBytesRead;

				var bytesRead = inputFileStream.Read(buffer, 0, size);
				if (bytesRead == 0)
					break;

				dataRead(index, buffer, bytesRead);
				totalBytesRead -= bytesRead;
				index += bytesRead;
			}
		}

		private static byte[] ReadChunkLength(Stream inputFileStream, out uint chunkLength)
		{
			var lengthBytes = new byte[4];
			// ReSharper disable once MustUseReturnValue
			inputFileStream.Read(lengthBytes, 0, 4);
			chunkLength = BitConverter.ToUInt32(lengthBytes.Reverse().ToArray(), 0);
			return lengthBytes;
		}

		private static byte[] ReadCrc(Stream inputFileStream)
		{
			var lengthBytes = new byte[4];
			// ReSharper disable once MustUseReturnValue
			inputFileStream.Read(lengthBytes, 0, 4);
			return lengthBytes;
		}
	}
}