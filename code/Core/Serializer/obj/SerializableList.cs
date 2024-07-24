using Game.Core.Meta;
using Game.Core.Serializer.Impl;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Core.Serializer.Obj {
    public interface ISerializableList<T> : ISerializable where T: new() {
        internal List<T> Items { get; }
        internal bool IsPolyItem { get; }

        public new void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Serialize(ctx, stream, this);

            // 先写入个数量.
            SerializeUtils.WriteValueTypeToStream(stream, Items.Count);
            // 对于存储的是值类型，直接掏出来序列化就好.
            Type type = typeof(T);
            if (type.IsValueType) {
                SerializeUtils.WriteSpanToStream(stream, CollectionsMarshal.AsSpan(Items));
                return;
            }
            // 对于引用类型，不需要多态特性的话，直接依次序列化数据就好.
            Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} property or ValueType property!");
            if (!IsPolyItem) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(type);
                foreach (T v in Items) ((ISerializable)v!).Serialize(ctx, stream, tMeta);
                return;
            }
            // 对于引用类型需要多态的，稍微麻烦一点，需要记录的东西比较多.
            foreach (T v in Items) ObjectSerializer.Serialize(ctx, stream, (ISerializable)v!);
        }

        public new void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Deserialize(ctx, stream, this);

            // 值类型直接批量反序列化
            int count = SerializeUtils.ReadValueTypeFromStream<int>(stream);
            Type type = typeof(T);
            Items.Capacity = count;
            if (type.IsValueType) {
                SerializeUtils.ReadStreamToSpan(stream, CollectionsMarshal.AsSpan(Items));
                return;
            }
            // 对于引用类型，不需要多态特性的依次创建然后反序列化.
            Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)} property or ValueType property!");
            if (!IsPolyItem) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(type);
                for (int i = 0; i != count; ++i) {
                    ISerializable obj = (ISerializable)new T();
                    obj.Deserialize(ctx, stream, tMeta);
                    Items.Add((T)obj);
                }
                return;
            }
            // 对于引用类型需要多态的，麻烦的进行Object反序列化.
            for (int i = 0; i != count; ++i) Items.Add((T)ObjectSerializer.Deserialize(ctx, stream));
        }
    }
}
