using Game.Core.Meta;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Game.Core.Serializer.Impl {

    [StructLayout(LayoutKind.Sequential)]
    internal struct SerializeTypeHeadData {
        public int TypeNameLength;
        public int GenericTypeCount;
        public int SerializeDataLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SerializeGenericTypeHeadData {
        public fixed int GenericTypeNameLength[ObjectSerializer.MAX_GENERIC_ARGS_CNT];
    }

    /// <summary>
    /// 负责序列化和反序列化一个ISerializable类型的对象，会在stream中写入类型信息，并在反序列化时提取类型，目前仅支持：
    /// <para> 实现了<see cref="ISerializable">ISerializable</see>接口的普通引用类型，和至多两个泛型参数的泛型类型, 不允许嵌套. </para>
    /// </summary>
    internal static class ObjectSerializer {
        private static readonly byte[] EMPTY_BUFFER = new byte[Unsafe.SizeOf<SerializeTypeHeadData>() + Unsafe.SizeOf<SerializeGenericTypeHeadData>() + 10];
        public const int MAX_GENERIC_ARGS_CNT = 2;

        /// <summary>
        /// 序列化一个<see cref="ISerializable">ISerializable</see>对象.
        /// </summary>
        /// <param name="ctx">序列化上下文</param>
        /// <param name="stream">用于存储的内存数据流</param>
        /// <param name="obj">序列化对象</param>
        public static unsafe void Serialize(SerializeContext ctx, MemoryStream stream, ISerializable obj) {
            long startPos = stream.Position;
            // 获取预存储的meta信息(使用原始类型).
            Type type = obj.GetType();
            Type[] genericArgs = type.GenericTypeArguments;
            int genericCount = genericArgs.Length;
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(genericCount > 0 ? type.GetGenericTypeDefinition() : type);

            // 获取obj的类型数据.
            Debug.Assert(genericCount <= MAX_GENERIC_ARGS_CNT, $"Can only support generic type whose args count less than " +
                $"{MAX_GENERIC_ARGS_CNT}, type {type} has {genericCount}");
            // 直接分配头部的空间.
            int headSize = Unsafe.SizeOf<SerializeTypeHeadData>();
            int allHeadSize = headSize;
            if (genericCount > 0) allHeadSize += Unsafe.SizeOf<SerializeGenericTypeHeadData>();
            stream.Write(EMPTY_BUFFER, 0, allHeadSize);
            // 写入头部数据和原始类型.
            byte[] typeName = Encoding.ASCII.GetBytes(type.Name);
            fixed (void* p = &stream.GetBuffer()[startPos]) {
                SerializeTypeHeadData* ptr = (SerializeTypeHeadData*)p;
                ptr->TypeNameLength = typeName.Length;
                ptr->GenericTypeCount = genericCount;
            }
            stream.Write(typeName);
            // 写入第二头部数据和泛型类型数据.
            if (genericCount > 0) {
                fixed (void* p = &stream.GetBuffer()[startPos + headSize]) {
                    int* ptr = ((SerializeGenericTypeHeadData*)p)->GenericTypeNameLength;
                    foreach (Type genericType in genericArgs) {
                        byte[] gTypeName = Encoding.ASCII.GetBytes(type.Name);
                        *(ptr++) = gTypeName.Length;
                        stream.Write(gTypeName);
                    }
                }
            }

            // 对象序列化.
            obj.Serialize(ctx, stream, meta);

            // 补充头部数据.
            fixed (byte* p = &stream.GetBuffer()[startPos]) {
                SerializeTypeHeadData* ptr = (SerializeTypeHeadData*)(p);
                ptr->SerializeDataLength = (int)(stream.Position - startPos);
            }
        }

        /// <summary>
        /// 反序列化一个<see cref="ISerializable">ISerializable</see>对象.
        /// </summary>
        /// <param name="ctx">序列化上下文</param>
        /// <param name="stream">用于获取数据的内存数据流</param>
        /// <returns></returns>
        public static unsafe ISerializable Deserialize(SerializeContext ctx, MemoryStream stream) {
            // 先读第一头部信息.
            SerializeTypeHeadData head;
            fixed (byte* p = &stream.GetBuffer()[stream.Position]) {
                head = Unsafe.ReadUnaligned<SerializeTypeHeadData>(ref *p);
                stream.Position += Unsafe.SizeOf<SerializeTypeHeadData>();
            }
            // 判断是否是泛型类型.
            SerializeGenericTypeHeadData head2;
            if (head.GenericTypeCount > 0) { 
                fixed (byte* p = &stream.GetBuffer()[stream.Position]) {
                    head2 = Unsafe.ReadUnaligned<SerializeGenericTypeHeadData>(ref *p);
                    stream.Position += Unsafe.SizeOf<SerializeGenericTypeHeadData>();
                }
            }

            // 读取原始typename并获取到类型.
            stream.Read(EMPTY_BUFFER, 0, head.TypeNameLength);
            string name = Encoding.ASCII.GetString(EMPTY_BUFFER, 0, head.TypeNameLength);
            Type type = ctx.MetaManager.GetTypeByName(name);
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(type);
            // 泛型类型额外生成真实类型.
            if (head.GenericTypeCount > 0) {
                Type[] generticTypes = new Type[head.GenericTypeCount];
                for(int i = 0; i != head.GenericTypeCount; ++i) {
                    int gTypeLen = head2.GenericTypeNameLength[i];
                    stream.Read(EMPTY_BUFFER, 0, gTypeLen);
                    string gName = Encoding.ASCII.GetString(EMPTY_BUFFER, 0, gTypeLen);
                    generticTypes[i] = ctx.MetaManager.GetTypeByName(name);
                }
                type = type.MakeGenericType(generticTypes);
            }

            // 反序列化对象并返回.
            ISerializable obj = (ISerializable)Activator.CreateInstance(type)!;
            obj.Deserialize(ctx, stream, meta);
            return obj;
        }
    }
}
