using Game.Core.Serializer.impl;
using System.Diagnostics;

namespace Game.Core.Serializer {

    public static class Serializer {

        public static byte[] Serialize(SerializeContext ctx, object obj) {
            Type type = obj.GetType();
            MemoryStream stream = new();
            stream.WriteByte((byte)(type.IsValueType ? 1 : 0));
            if (type.IsValueType) ValueTypeSerializer.Serialize(ctx, stream, (ValueType)obj);
            else {
                Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)}, not {type.Name}");
                ObjectSerializer.Serialize(ctx, stream, (ISerializable)obj);
            }
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
