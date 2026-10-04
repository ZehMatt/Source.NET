using Source.Common;
using Source.Common.Filesystem;
using Source.Common.Hashing;
using Source.Common.SceneFileCache;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Source.Engine;

public class SceneFileCache(IFileSystem filesystem) : ISceneFileCache
{
	byte[]? SceneImageFile;

	public void Init() {
		const string sceneImageName = "scenes/scenes.image";

		if (SceneImageFile == null) {
			using IFileHandle? handle = filesystem.Open(sceneImageName, FileOpenOptions.Read | FileOpenOptions.Binary, "MOD");
			if (handle != null) {
				SceneImageFile = new byte[handle.Stream.Length];
				handle.Stream.ReadExactly(SceneImageFile);

				ref readonly SceneImageHeader header = ref GetHeader(SceneImageFile);
				if (header.Id != SceneImageHeader.SCENE_IMAGE_ID || header.Version != SceneImageHeader.SCENE_IMAGE_VERSION)
					Error($"CSceneFileCache: Bad scene image file {sceneImageName}\n");
			}
			else
				SceneImageFile = null;
		}
	}

	public void Shutdown() {
		SceneImageFile = null;
	}

	public void Reload() {
		Shutdown();
		Init();
	}

	public nuint GetSceneBufferSize(ReadOnlySpan<char> filename) {
		nuint returnSize = 0;

		Span<char> fn = stackalloc char[MAX_PATH];
		strcpy(fn, filename);
		StrTools.FixSlashes(fn);
		strlower(fn);

		GetSceneDataFromImage(filename, FindSceneInImage(fn), default, ref returnSize);
		return returnSize;
	}

	public bool GetSceneData(ReadOnlySpan<char> filename, Span<byte> buf) {
		Assert(!filename.IsEmpty);
		Assert(!buf.IsEmpty);

		Span<char> fn = stackalloc char[MAX_PATH];
		strcpy(fn, filename);
		StrTools.FixSlashes(fn);
		strlower(fn);

		nuint length = (nuint)buf.Length;
		return GetSceneDataFromImage(filename, FindSceneInImage(fn), buf, ref length);
	}

	public bool GetSceneCachedData(ReadOnlySpan<char> filename, out SceneCachedData data) {
		int scene = FindSceneInImage(filename);
		byte[]? image = SceneImageFile;
		if (image == null || scene < 0 || scene >= GetHeader(image).NumScenes) {
			data.SceneId = -1;
			data.Msecs = 0;
			data.NumSounds = 0;
			return false;
		}

		ref readonly SceneImageSummary summary = ref GetSummary(image, scene);

		data.SceneId = scene;
		data.Msecs = summary.Msecs;
		data.NumSounds = summary.NumSounds;

		return true;
	}

	public short GetSceneCachedSound(int scene, int sound) {
		byte[]? image = SceneImageFile;
		if (image == null || scene < 0 || scene >= GetHeader(image).NumScenes)
			return -1;

		ref readonly SceneImageSummary summary = ref GetSummary(image, scene);
		if (sound < 0 || sound >= summary.NumSounds) {
			Assert(false);
			return -1;
		}

		int soundStringsOffset = GetEntries(image)[scene].SceneSummaryOffset + Unsafe.SizeOf<SceneImageSummary>();
		return (short)MemoryMarshal.Cast<byte, int>(image.AsSpan(soundStringsOffset))[sound];
	}

	public string? GetSceneString(short stringId) {
		byte[]? image = SceneImageFile;
		if (image == null || stringId < 0 || stringId >= GetHeader(image).NumStrings)
			return null;

		return GetHeader(image).String(image, stringId);
	}

	static ref readonly SceneImageHeader GetHeader(ReadOnlySpan<byte> image) => ref MemoryMarshal.AsRef<SceneImageHeader>(image);

	static ReadOnlySpan<SceneImageEntry> GetEntries(ReadOnlySpan<byte> image) => MemoryMarshal.Cast<byte, SceneImageEntry>(image[GetHeader(image).SceneEntryOffset..]);

	static ref readonly SceneImageSummary GetSummary(ReadOnlySpan<byte> image, int scene) => ref MemoryMarshal.AsRef<SceneImageSummary>(image[GetEntries(image)[scene].SceneSummaryOffset..]);

	int FindSceneInImage(ReadOnlySpan<char> sceneName) {
		byte[]? image = SceneImageFile;
		if (image == null)
			return -1;

		ReadOnlySpan<SceneImageEntry> entries = GetEntries(image);

		Span<char> cleanName = stackalloc char[MAX_PATH];

		strcpy(cleanName, sceneName);
		strlower(cleanName);
		StrTools.FixSlashes(cleanName, '\\');
		StrTools.SetExtension(cleanName, ".vcd");

		ReadOnlySpan<char> cleanNameStr = cleanName[..(int)strlen(cleanName)];
		Span<byte> cleanNameBytes = stackalloc byte[Encoding.UTF8.GetMaxByteCount(cleanNameStr.Length)];
		int cleanNameByteCount = Encoding.UTF8.GetBytes(cleanNameStr, cleanNameBytes);

		CRC32_t crcFilename = default;
		CRC32.Init(ref crcFilename);
		CRC32.ProcessBuffer(ref crcFilename, (ReadOnlySpan<byte>)cleanNameBytes[..cleanNameByteCount]);
		CRC32.Final(ref crcFilename);

		int lowerIdx = 1;
		int upperIdx = GetHeader(image).NumScenes;
		for (; ; ) {
			if (upperIdx < lowerIdx)
				return -1;
			else {
				int middleIndex = (lowerIdx + upperIdx) / 2;
				CRC32_t probe = entries[middleIndex - 1].CrcFilename;
				if (crcFilename < probe)
					upperIdx = middleIndex - 1;
				else {
					if (crcFilename > probe)
						lowerIdx = middleIndex + 1;
					else
						return middleIndex - 1;
				}
			}
		}
	}

	bool GetSceneDataFromImage(ReadOnlySpan<char> fileName, int scene, Span<byte> sceneData, ref nuint sceneLength) {
		byte[]? image = SceneImageFile;
		if (image == null || scene < 0 || scene >= GetHeader(image).NumScenes) {
			if (!sceneData.IsEmpty)
				sceneData[0] = 0;
			sceneLength = 0;
			return false;
		}

		ref readonly SceneImageEntry entry = ref GetEntries(image)[scene];
		if ((uint)entry.DataOffset > (uint)image.Length || (uint)entry.DataLength > (uint)(image.Length - entry.DataOffset)) {
			Warning($"CSceneFileCache: Scene {scene} data out of range\n");
			if (!sceneData.IsEmpty)
				sceneData[0] = 0;
			sceneLength = 0;
			return false;
		}

		ReadOnlySpan<byte> data = image.AsSpan(entry.DataOffset, entry.DataLength);
		bool isCompressed = LZMA.IsCompressed(data);
		if (isCompressed) {
			int originalSize = (int)LZMA.GetActualSize(data);
			if (!sceneData.IsEmpty) {
				int maxLen = Math.Min((int)sceneLength, sceneData.Length);
				if (originalSize <= maxLen) {
					if (LZMA.Uncompress(data, sceneData) == 0)
						return false;
				}
				else {
					byte[] outputData = new byte[originalSize];
					if (LZMA.Uncompress(data, outputData) == 0)
						return false;
					outputData.AsSpan(0, maxLen).CopyTo(sceneData);
				}
			}
			sceneLength = (nuint)originalSize;
		}
		else {
			if (!sceneData.IsEmpty) {
				nuint countToCopy = Math.Min(sceneLength, (nuint)entry.DataLength);
				data[..(int)countToCopy].CopyTo(sceneData);
			}
			sceneLength = (nuint)entry.DataLength;
		}
		return true;
	}
}
