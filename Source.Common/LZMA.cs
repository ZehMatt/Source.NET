using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.Common;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LZMAHeader
{
	public const int LZMA_ID = ('A' << 24) | ('M' << 16) | ('Z' << 8) | 'L';

	public uint ID;
	public uint ActualSize;
	public uint LZMASize;
	public InlineArray5<byte> Properties;
}

public static class LZMA
{
	public static bool IsCompressed(ReadOnlySpan<byte> input) {
		if (input.Length < Unsafe.SizeOf<LZMAHeader>())
			return false;

		return MemoryMarshal.Read<LZMAHeader>(input).ID == LZMAHeader.LZMA_ID;
	}

	public static uint GetActualSize(ReadOnlySpan<byte> input) {
		if (!IsCompressed(input))
			return 0;

		return MemoryMarshal.Read<LZMAHeader>(input).ActualSize;
	}

	public static unsafe uint Uncompress(ReadOnlySpan<byte> input, Span<byte> output) {
		uint actualSize = GetActualSize(input);
		if (actualSize == 0)
			return 0;

		LZMAHeader header = MemoryMarshal.Read<LZMAHeader>(input);
		int headerSize = Unsafe.SizeOf<LZMAHeader>();
		if (header.LZMASize > (uint)(input.Length - headerSize) || actualSize > (uint)output.Length) {
			Assert(false);
			return 0;
		}

		SevenZip.Compression.LZMA.Decoder decoder = new();
		decoder.SetDecoderProperties(((ReadOnlySpan<byte>)header.Properties).ToArray());

		fixed (byte* inputPtr = input)
		fixed (byte* outputPtr = output) {
			using UnmanagedMemoryStream inStream = new(inputPtr + headerSize, header.LZMASize);
			using UnmanagedMemoryStream outStream = new(outputPtr, 0, actualSize, FileAccess.Write);
			decoder.Code(inStream, outStream, header.LZMASize, actualSize, null);

			if (outStream.Position != actualSize) {
				Assert(false);
				return 0;
			}
		}

		return actualSize;
	}
}
