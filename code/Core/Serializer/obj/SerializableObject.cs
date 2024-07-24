using Game.Core.Meta;

namespace Game.Core.Serializer.Obj {
    public interface ISerializableObject : ISerializable {

        public new void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Serialize(ctx, stream, this);
        }

        public new void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Deserialize(ctx, stream, this);
        }
    }
}
