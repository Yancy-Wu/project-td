using Game.Core.Meta;

namespace Game.Core.Serializer {
    public class SerializableObject: ISerializable {

        public void Serialize(SerializeContext ctx, MemoryStream stream) {
            Type type = GetType();
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(type);
            foreach (var prop in meta.SerializeProperties!) {
                prop.Serialize(ctx, stream, this);
            }
        }

        public void Deserialize(SerializeContext ctx, MemoryStream stream) {
            Type type = GetType();
            TypeMeta meta = ctx.MetaManager.GetTypeMeta(type);
            foreach (var prop in meta.SerializeProperties!) {
                prop.Deserialize(ctx, stream, this);
            }
        }
    }
}
