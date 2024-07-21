using Game.Core.Meta;

namespace Game.Core.Serializer.obj {
    public class SerializableObject : ISerializable {

        public void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            foreach (var prop in meta.SerializeProperties!) {
                prop.Serialize(ctx, stream, this);
            }
        }

        public void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            foreach (var prop in meta.SerializeProperties!) {
                prop.Deserialize(ctx, stream, this);
            }
        }
    }
}
