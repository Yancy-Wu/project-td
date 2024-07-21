using Game.Core.Meta;

namespace Game.Core.Serializer {

    public interface ISerializable {
        public void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta);
        public void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta);
    }

    public interface IPropertySerializer {
        public void Serialize(SerializeContext ctx, MemoryStream stream, object obj);
        public void Deserialize(SerializeContext ctx, MemoryStream stream, object obj);
    }

    public struct SerializeContext {
        public TypeMetaManager MetaManager { get; set; }
    }
}
