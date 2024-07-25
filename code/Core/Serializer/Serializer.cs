using Game.Core.Serializer.Impl;
using System.Diagnostics;

namespace Game.Core.Serializer {

    public static class Serializer {

        /// <summary>
        /// 序列化入口，现在支持：
        /// <para> 1. 不包含引用的简单值类型序列化，Primitive </para>
        /// <para> 2. 实现了<see cref="ISerializable">ISerializable</see>接口的对象序列化，支持多态，但不支持泛型类型[为了序列化简便] </para>
        /// 现在不支持：
        /// <para> 数组类型的序列化 </para>
        /// 不支持泛型类型的原因是有类型递归记录的问题，而且多出一堆判断出来，不值当.
        /// </summary>
        /// <param name="ctx">序列化上下文数据</param>
        /// <param name="obj">需要序列化的对象</param>
        /// <returns>序列化后的byte数据</returns>
        public static byte[] Serialize(SerializeContext ctx, object obj) {
            Type type = obj.GetType();
            MemoryStream stream = new();
            byte typeFlag = 0;
            stream.WriteByte(0);
            // 简单值类型.
            if (type.IsValueType) {
                typeFlag = 1;
                ValueTypeSerializer.Serialize(ctx, stream, (ValueType)obj);
            }
            // 字符串引用需要特殊处理一下.
            else if (obj is string str) {
                typeFlag = 2;
                StringSerializer.Serialize(ctx, stream, str);
            }
            // ISerializable类型
            // 其余的类型，比如Array，非ISerializable接口对象直接报错.
            else {
                typeFlag = 3;
                Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)}, not {type.Name}");
                Debug.Assert(type.GenericTypeArguments.Length == 0, $"Can not serialize Generic Type, please derive it to normal type.");
                ObjectSerializer.Serialize(ctx, stream, (ISerializable)obj);
            }
            SerializeUtils.WriteValueTypeToStream(stream, typeFlag, 0);
            return stream.ToArray();
        }

        /// <summary>
        /// 反序列化入口，类型要求可参考<see cref="Serialize">Serialize函数</see>
        /// </summary>
        /// <param name="ctx">序列化上下文数据</param>
        /// <param name="data">反序列化的原始数据</param>
        /// <returns>序列化后的对象</returns>
        public static object Deserialize(SerializeContext ctx, byte[] data) {
            MemoryStream stream = new(data);
            // 先读取类型，再分类处理.
            int typeFlag = stream.ReadByte();
            if (typeFlag == 1) return ValueTypeSerializer.Deserialize(ctx, stream);
            else if (typeFlag == 2) return StringSerializer.Deserialize(ctx, stream);
            Debug.Assert(typeFlag == 3, $"Incorrect typeFlag: {typeFlag}");
            return ObjectSerializer.Deserialize(ctx, stream);
        }
    }
}
