using Game.Core.Serializer.Impl;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer {
    public static class SerializeUtils {

        /// <summary>
        /// 针对类型type上指定的属性名称，构造其属性序列化器.
        /// </summary>
        /// <param name="objType">要构造的类型</param>
        /// <param name="propName">要构造的属性名称</param>
        /// <returns>属性序列化实现</returns>
        public static IPropertySerializer CreatePropertySerializer(Type objType, string propName) {
            PropertyInfo property = objType.GetProperty(propName)!;
            Type vt = property.GetGetMethod()!.ReturnType;
            Type type = typeof(PropertySerializer<,>).MakeGenericType(objType, vt);
            return (IPropertySerializer)Activator.CreateInstance(type, propName)!;
        }

        /// <summary>
        /// 从stream中读入指定数量的T类型数据到mem内存地址，并返回其T类型的Span表示, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="mem">存储的内存地址(托管或者非托管的)</param>
        /// <returns>mem地址的T类型表示</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// 从stream中读入数据到mem地址中，数量由mem的长度给出, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="mem">存储的内存地址(托管或者非托管的)</param>
#pragma warning disable CS8500
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadStreamToSpan<T>(MemoryStream stream, Span<T> mem) {
            int size = Unsafe.SizeOf<T>();
            fixed (void* s = mem) {
                Span<byte> byteSpan = new(s, mem.Length * size);
                stream.Read(byteSpan);
            }
        }
#pragma warning restore

        /// <summary>
        /// 写入迭代器数据到stream数据中, 仅支持值类型迭代器, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="values">迭代器数据</param>
        /// <param name="count">需要写入的数量</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            stream.Position += size * count;
        }

        /// <summary>
        /// 将T类型的内存数据写入到stream中，数量由span的长度指定，仅支持值类型, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="span">T类型内存数据</param>
#pragma warning disable CS8500
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteSpanToStream<T>(MemoryStream stream, Span<T> span) {
            int size = Unsafe.SizeOf<T>();
            fixed (void* s = span) {
                ReadOnlySpan<byte> byteSpan = new(s, span.Length * size);
                stream.Write(byteSpan);
            }
        }
#pragma warning restore

        /// <summary>
        /// 从stream数据流中读取一个T类型数据，仅支持值类型, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadValueTypeFromStream<T>(MemoryStream stream) {
            int sz = Unsafe.SizeOf<T>();
            long pos = stream.Position;
            stream.Position += sz;
            return Unsafe.ReadUnaligned<T>(ref stream.GetBuffer()[pos]);
        }

        /// <summary>
        /// 写入一个T类型数据到stream，仅支持值类型, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="val">写入数据</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueTypeToStream<T>(MemoryStream stream, T val) {
            int sz = Unsafe.SizeOf<T>();
            long pos = stream.Position;
            stream.SetLength(stream.Length + sz);
            Unsafe.WriteUnaligned(ref stream.GetBuffer()[pos], val);
            stream.Position += sz;
        }

        /// <summary>
        /// 预分配一个T类型数据到stream，仅支持值类型, 流指针会后移
        /// </summary>
        /// <param name="stream">原始数据流</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AllocStreamData<T>(MemoryStream stream) {
            int sz = Unsafe.SizeOf<T>();
            stream.SetLength(stream.Length + sz);
            stream.Position += sz;
        }

        /// <summary>
        /// 写入一个T类型数据到stream的指定位置，仅支持值类型，流指针的位置不会变动.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        /// <param name="val">写入的值</param>
        /// <param name="pos">写入流位置</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueTypeToStream<T>(MemoryStream stream, T val, long pos) {
            Unsafe.WriteUnaligned(ref stream.GetBuffer()[pos], val);
        }

        /// <summary>
        /// 从stream数据流指定位置中读取一个T类型数据，仅支持值类型，流指针的位置不会变动.
        /// </summary>
        /// <param name="stream">原始数据流</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadValueTypeFromStream<T>(MemoryStream stream, long pos) {
            return Unsafe.ReadUnaligned<T>(ref stream.GetBuffer()[pos]);
        }
    }
}
