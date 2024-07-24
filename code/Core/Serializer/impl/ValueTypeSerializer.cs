using System.Diagnostics;

namespace Game.Core.Serializer.Impl {
    internal static class ValueTypeSerializer {
        private static readonly int BUFFER_LEN = 32;
        private static readonly byte[] FixedBuffer = new byte[BUFFER_LEN];

        public static void Serialize(SerializeContext ctx, MemoryStream stream, ValueType obj) {
#if DEBUG
            Type type = obj.GetType();
            Debug.Assert(!type.IsEnum, "Serializer not support Enum type...");
            Debug.Assert(type.IsPrimitive, "Serializer not support Custom struct type, please use ISerializable impl instead.");
#endif
            // 只记录几个常用的类型，其他的就不支持了.
            byte typeInt = 0;
            byte[]? val = null;
            if (obj is int v1) { typeInt = 1; val = BitConverter.GetBytes(v1); }
            else if(obj is float v2) { typeInt = 2; val = BitConverter.GetBytes(v2); }
            else if (obj is bool v3) { typeInt = 3; val = BitConverter.GetBytes(v3); }
            else if (obj is double v4) { typeInt = 4; val = BitConverter.GetBytes(v4); }
            else if (obj is long v5) { typeInt = 5; val = BitConverter.GetBytes(v5); }
            Debug.Assert(typeInt != 0, $"Value Type Cannot Serialize {obj}.");
            // 先不区分大端小端.
            stream.WriteByte(typeInt);
            stream.Write(val!);
        }

        public static object Deserialize(SerializeContext _, MemoryStream stream) {
            byte typeInt = (byte)stream.ReadByte();
            return typeInt switch {
                1 => SerializeUtils.ReadValueTypeFromStream<int>(stream),
                2 => SerializeUtils.ReadValueTypeFromStream<float>(stream),
                3 => SerializeUtils.ReadValueTypeFromStream<bool>(stream),
                4 => SerializeUtils.ReadValueTypeFromStream<double>(stream),
                5 => SerializeUtils.ReadValueTypeFromStream<long>(stream),
                _ => throw new NotImplementedException($"unsupport value type: {typeInt}")
            };
        }
    }
}
