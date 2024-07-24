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
        /// <returns></returns>
        public static byte[] Serialize(SerializeContext ctx, object obj) {
            Type type = obj.GetType();
            MemoryStream stream = new();
            stream.WriteByte((byte)(type.IsValueType ? 1 : 0));
            // 简单值类型.
            if (type.IsValueType) ValueTypeSerializer.Serialize(ctx, stream, (ValueType)obj);
            // ISerializable类型
            else {
                Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)}, not {type.Name}");
                Debug.Assert(type.GenericTypeArguments.Length == 0, $"Can not serialize Generic Type, please derive it to normal type.");
                ObjectSerializer.Serialize(ctx, stream, (ISerializable)obj);
            }
            // 其余的类型，比如Array，非ISerializable接口对象直接报错.
            return stream.ToArray();
        }

        public static object Deserialize(SerializeContext ctx, byte[] data) {
            MemoryStream stream = new(data);
            bool IsValueType = stream.ReadByte() == 1;
            if (IsValueType) return ValueTypeSerializer.Deserialize(ctx, stream);
            return ObjectSerializer.Deserialize(ctx, stream);
        }
    }
}
