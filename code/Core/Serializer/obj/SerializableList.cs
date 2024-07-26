using Game.Core.Meta;
using Game.Core.Serializer.Impl;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Game.Core.Serializer.Obj {

    /// <summary>
    /// 可序列化列表的实现.
    /// </summary>
    public class SerializableList<T> : ISerializable {
        internal List<T> Items { get; } = new List<T>();

        public void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
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
            // 对于存储的是string，特殊处理一下.
            if (type == typeof(string)) {
                foreach (T v in Items) StringSerializer.Serialize(ctx, stream, (v as string)!);
                return;
            }
            // 对于引用类型，不需要多态特性的话，直接依次序列化数据就好.
            Debug.Assert(type.IsAssignableTo(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} property or ValueType property!");
            if (!type.IsInterface && !type.IsAbstract) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(type);
                foreach (T v in Items) ((ISerializable)v!).Serialize(ctx, stream, tMeta);
                return;
            }
            // 对于引用类型需要多态的，稍微麻烦一点，需要记录的东西比较多.
            foreach (T v in Items) ObjectSerializer.Serialize(ctx, stream, (ISerializable)v!);
        }

        public void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Deserialize(ctx, stream, this);

            // 值类型直接批量反序列化
            int count = SerializeUtils.ReadValueTypeFromStream<int>(stream);
            Type type = typeof(T);
            Items.EnsureCapacity(count);
            if (type.IsValueType) {
                Items.AddRange(GC.AllocateUninitializedArray<T>(count));
                SerializeUtils.ReadStreamToSpan(stream, CollectionsMarshal.AsSpan(Items));
                return;
            }
            // 对于存储的是string，特殊处理一下.
            if (type == typeof(string)) {
                for (int i = 0; i != count; ++i) Items.Add((T)(object)StringSerializer.Deserialize(ctx, stream));
                return;
            }
            // 对于引用类型，不需要多态特性的依次创建然后反序列化.
            Debug.Assert(type.IsAssignableTo(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)} property or ValueType property!");
            if (!type.IsInterface && !type.IsAbstract) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(type);
                for (int i = 0; i != count; ++i) {
                    ISerializable obj = (ISerializable)Activator.CreateInstance(typeof(T))!;
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
