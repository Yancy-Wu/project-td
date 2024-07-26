using Game.Core.Meta;

namespace Game.Core.Serializer {

    /// <summary>
    /// 可序列化对象接口，除了string等特殊引用类型之外必须继承自该对象才能序列化
    /// </summary>
    public interface ISerializable {
        public void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta);
        public void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta);
    }

    /// <summary>
    /// 属性序列化接口，定义了某个对象上某个属性的序列化方式
    /// </summary>
    public interface IPropertySerializer {
        public void Serialize(SerializeContext ctx, MemoryStream stream, object obj);
        public void Deserialize(SerializeContext ctx, MemoryStream stream, object obj);
    }

    /// <summary>
    /// 序列化上下文，贯穿序列化全程，用于提供额外数据等
    /// </summary>
    public struct SerializeContext {
        public TypeMetaManager MetaManager;
        public byte[] CacheBuffer128B = new byte[128];

        public SerializeContext(TypeMetaManager metaManager) {
            MetaManager = metaManager;
        }
    }
}
