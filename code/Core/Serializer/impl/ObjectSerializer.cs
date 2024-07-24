using Game.Core.Meta;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Game.Core.Serializer.Impl {

    [StructLayout(LayoutKind.Sequential)]
    internal struct SerializeTypeHeadData {
        public int TypeNameLength;
        public int SerializeDataLength;
    }

    /// <summary>
    /// 负责序列化和反序列化一个ISerializable类型的对象，会在stream中写入类型信息，并在反序列化时提取类型.
    /// <para> 实现了<see cref="ISerializable">ISerializable</see>接口的普通引用类型 </para>
    /// </summary>
    internal static class ObjectSerializer {

        /// <summary>
        /// 序列化一个<see cref="ISerializable">ISerializable</see>对象.
        /// </summary>
        /// <param name="ctx">序列化上下文</param>
        /// <param name="stream">用于存储的内存数据流</param>
        /// <param name="obj">序列化对象</param>
        public static unsafe void Serialize(SerializeContext ctx, MemoryStream stream, ISerializable obj) {
            long startPos = stream.Position;
            // 获取预存储的meta信息.
            Type type = obj.GetType();
            Debug.Assert(type.GenericTypeArguments.Length == 0, $"Can not serialize Generic Type, please derive it to normal type.");
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(type);

            // 直接分配头部的空间.
            int headSize = Unsafe.SizeOf<SerializeTypeHeadData>();
            stream.SetLength(stream.Length + headSize);
            // 写入原始类型数据.
            byte[] typeName = Encoding.ASCII.GetBytes(type.Name);
            stream.Write(typeName);

            // 对象序列化.
            obj.Serialize(ctx, stream, meta);

            // 补充头部数据.
            fixed (byte* p = &stream.GetBuffer()[startPos]) {
                SerializeTypeHeadData* ptr = (SerializeTypeHeadData*)(p);
                ptr->SerializeDataLength = (int)(stream.Position - startPos);
                ptr->TypeNameLength = typeName.Length;
            }
        }

        /// <summary>
        /// 反序列化一个<see cref="ISerializable">ISerializable</see>对象.
        /// </summary>
        /// <param name="ctx">序列化上下文</param>
        /// <param name="stream">用于获取数据的内存数据流</param>
        /// <returns></returns>
        public static ISerializable Deserialize(SerializeContext ctx, MemoryStream stream) {
            // 先读头部信息.
            SerializeTypeHeadData head = SerializeUtils.ReadValueTypeFromStream<SerializeTypeHeadData>(stream);

            // 读取原始typename并获取到类型.
            stream.Read(ctx.CacheBuffer128B, 0, head.TypeNameLength);
            string name = Encoding.ASCII.GetString(ctx.CacheBuffer128B, 0, head.TypeNameLength);
            Type type = ctx.MetaManager.GetTypeByName(name);
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(type);

            // 反序列化对象并返回.
            ISerializable obj = (ISerializable)Activator.CreateInstance(type)!;
            obj.Deserialize(ctx, stream, meta);
            return obj;
        }
    }
}
