using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer.Impl {
    internal static class ValueTypeSerializer {
        private static readonly Func<MemoryStream, object>[] PRIMITIVE_READ_FUNCS = {
            _readPrimitiveValue<int>,
            _readPrimitiveValue<float>,
            _readPrimitiveValue<bool>,
            _readPrimitiveValue<double>,
            _readPrimitiveValue<long>,
        };
        private static readonly int BUFFER_LEN = 32;
        private static readonly byte[] FixedBuffer = new byte[BUFFER_LEN];

        private static object _readPrimitiveValue<T>(MemoryStream stream) {
            int sz = Unsafe.SizeOf<T>();
            stream.Read(FixedBuffer, 0, sz);
            return Unsafe.ReadUnaligned<T>(ref FixedBuffer[0])!;
        }

        public static void Serialize(SerializeContext ctx, MemoryStream stream, ValueType obj) {
#if DEBUG
            Type type = obj.GetType();
            Debug.Assert(!type.IsEnum, "Serializer not support Enum type...");
            Debug.Assert(type.IsPrimitive, "Serializer not support Custom struct type, please use ISerializable impl instead.");
#endif
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

        public static object Deserialize(SerializeContext ctx, MemoryStream stream) {
            byte typeInt = (byte)stream.ReadByte();
            return PRIMITIVE_READ_FUNCS[typeInt].Invoke(stream);
        }
    }
}
