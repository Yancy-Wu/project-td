using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Game.Core.Serializer.Impl {

    /// <summary>
    /// 字符串序列化的实现，部分参照了MemoryPack的实现方式.
    /// </summary>
    internal static class StringSerializer {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(SerializeContext _, MemoryStream stream, string obj) {
            // 这里直接搬运了MemoryPack的实现，序列化成UTF-8，内存中的貌似是UTF-16的格式.
            // 另外这个utf16-length貌似也没什么用.

            // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
            // UTF8.GetMaxByteCount -> (length + 1) * 3
            ReadOnlySpan<char> source = obj.AsSpan();
            int maxByteCount = (source.Length + 1) * 3;
            stream.SetLength(stream.Length + maxByteCount + 8);
            ref byte destPointer = ref stream.GetBuffer()[stream.Position]; // header

            // write utf16-length
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref destPointer, 4), source.Length);

            // write bytes data
            Span<byte> dest = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref destPointer, 8), maxByteCount);
            int bytesWritten = Encoding.UTF8.GetBytes(obj, dest);

            // write written utf8-length in header, that is ~length
            Unsafe.WriteUnaligned(ref destPointer, ~bytesWritten);
            stream.Position += 8 + bytesWritten;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Deserialize(SerializeContext _, MemoryStream stream) {
            // 从流位置依次读取子节数、char数目[貌似没用]以及byte数据.
            ref byte ptr = ref stream.GetBuffer()[stream.Position]; // header
            int byteCount = ~Unsafe.ReadUnaligned<int>(ref ptr);
            int charCount = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref ptr, 4));
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref ptr, 8), byteCount);

            // 构建UTF-8字符串
            string ret = Encoding.UTF8.GetString(span);
            Debug.Assert(ret.Length == charCount, "string deserialize error.");

            // stream直接往前推进.
            stream.Position += 8 + byteCount;
            return ret;
        }
    }
}
