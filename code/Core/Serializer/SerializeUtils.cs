using Game.Core.Serializer.Impl;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer {
    public static class SerializeUtils {
        public static IPropertySerializer CreatePropertySerializer(Type objType, string propName) {
            PropertyInfo property = objType.GetProperty(propName)!;
            Type vt = property.GetGetMethod()!.ReturnType;
            Type type = typeof(PropertySerializer<,>).MakeGenericType(objType, vt);
            return (IPropertySerializer)Activator.CreateInstance(type, propName)!;
        }

        /// <summary>
        /// 从stream中读入指定数量的T类型数据到mem内存地址，并返回其T类型的Span表示.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="mem">存储的内存地址(托管或者非托管的)</param>
        /// <returns>mem地址的T类型表示</returns>
        public static unsafe Span<T> ReadSpanFromStream<T>(MemoryStream stream, Span<byte> mem, int count) {
            // 掏一下数量.
            int size = Unsafe.SizeOf<T>();
            stream.Read(mem);
            // 搞到所有的key.
            fixed (byte* ptr = mem) {
                return new(ptr, count);
            }
        }

        /// <summary>
        /// 从stream中读入数据到mem地址中，数量由mem的长度给出.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="mem">存储的内存地址(托管或者非托管的)</param>
        public static unsafe void ReadStreamToSpan<T>(MemoryStream stream, Span<T> mem) {
            int size = Unsafe.SizeOf<T>();
            fixed (void* s = mem) {
                Span<byte> byteSpan = new(s, mem.Length * size);
                stream.Read(byteSpan);
            }
        }

        /// <summary>
        /// 写入迭代器数据到stream数据中, 仅支持值类型迭代器.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="values">迭代器数据</param>
        /// <param name="count">需要写入的数量</param>
        public static unsafe void WriteSeqToStream<T>(MemoryStream stream, IEnumerable<T> values, int count) {
            long startPos = stream.Position;
            int size = Unsafe.SizeOf<T>();
            stream.SetLength(stream.Length + size * count);
            fixed (byte* ptr = &stream.GetBuffer()[startPos]) {
                byte* cur = ptr;
                foreach (T v in values) {
                    Unsafe.WriteUnaligned(ref cur[0], v);
                    cur += size;
                }
            }
        }

        /// <summary>
        /// 将T类型的内存数据写入到stream中，数量由span的长度指定，仅支持值类型.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="span">T类型内存数据</param>
        public static unsafe void WriteSpanToStream<T>(MemoryStream stream, Span<T> span) {
            int size = Unsafe.SizeOf<T>();
            fixed (void* s = span) {
                ReadOnlySpan<byte> byteSpan = new(s, span.Length * size);
                stream.Write(byteSpan);
            }
        }

        /// <summary>
        /// 从stream数据流中读取一个T类型数据，仅支持值类型.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        public static unsafe T ReadValueTypeFromStream<T>(MemoryStream stream) {
            int sz = Unsafe.SizeOf<T>();
            fixed (byte* p = &stream.GetBuffer()[stream.Position]) {
                stream.Position += sz;
                return Unsafe.ReadUnaligned<T>(ref *p);
            }
        }

        /// <summary>
        /// 从stream数据流中读取一个T类型数据，仅支持值类型.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        public static void WriteValueTypeToStream<T>(MemoryStream stream, T val) {
            int sz = Unsafe.SizeOf<T>();
            long pos = stream.Position;
            stream.SetLength(stream.Length + sz);
            Unsafe.WriteUnaligned(ref stream.GetBuffer()[pos], val);
        }
    }
}
