using Game.Core.Meta;
using System.Runtime.CompilerServices;
using System.Text;

namespace Game.Core.Serializer {
    internal struct SerializeTypeHeadData {
        public int TypeNameLength;
        public int SerializeDataLength;
    }

    internal static class ObjectSerializer {
        public static unsafe void Serialize(SerializeContext ctx, MemoryStream stream, ISerializable obj) {
            long startPos = stream.Position;
            Type type = obj.GetType();
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(type);
            byte[] name = Encoding.ASCII.GetBytes(type.Name);
            int headSize = Unsafe.SizeOf<SerializeTypeHeadData>();
            var array = GC.AllocateUninitializedArray<byte>(headSize);
            stream.Write(array);
            stream.Write(name);
            obj.Serialize(ctx, stream);
            long endPos = stream.Position;
            fixed (byte* p = &stream.GetBuffer()[0]) {
                SerializeTypeHeadData* ptr = (SerializeTypeHeadData*)(p + startPos);
                ptr->TypeNameLength = name.Length;
                ptr->SerializeDataLength = (int)(endPos - startPos);
            }
        }

        public static unsafe ISerializable Deserialize(SerializeContext ctx, MemoryStream stream) {
            long startPos = stream.Position;
            int headSize = Unsafe.SizeOf<SerializeTypeHeadData>();
            SerializeTypeHeadData head;
            fixed (byte* p = &stream.GetBuffer()[0]) {
                head = Unsafe.ReadUnaligned<SerializeTypeHeadData>(ref *(p + startPos));
            }
            byte[] nameBytes = new byte[head.TypeNameLength];
            stream.Read(nameBytes);
            string name = Encoding.ASCII.GetString(nameBytes);
            Type type = ctx.MetaManager.GetTypeByName(name);
            ISerializable obj = (ISerializable)Activator.CreateInstance(type)!;
            obj.Deserialize(ctx, stream);
            return obj;
        }
    }
}
