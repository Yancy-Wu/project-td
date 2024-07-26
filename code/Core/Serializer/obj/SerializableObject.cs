using Game.Core.Meta;

namespace Game.Core.Serializer.Obj {

    /// <summary>
    /// 可序列化对象的实现.
    /// </summary>
    public class SerializableObject : ISerializable {

        public void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Serialize(ctx, stream, this);
        }

        public void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Deserialize(ctx, stream, this);
        }
    }
}
