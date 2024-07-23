using Game.Core.Meta;

namespace Game.Core.Serializer.Obj {
    public interface ISerializableObject : ISerializable {

        public new void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            foreach (var prop in meta.SerializeProperties!) {
                prop.Serialize(ctx, stream, this);
            }
        }

        public new void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            foreach (var prop in meta.SerializeProperties!) {
                prop.Deserialize(ctx, stream, this);
            }
        }
    }
}
