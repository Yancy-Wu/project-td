using Game.Core.Meta;
using Game.Core.Serializer.Impl;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer.Obj {
    public interface ISerializableDict<TK, TV> : ISerializable where TK: notnull where TV: new() {
        internal Dictionary<TK, TV> Items { get; }
        internal bool IsPolyItem { get; }

        public new void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Serialize(ctx, stream, this);

            // 先写入个数量.
            int count = Items.Count;
            SerializeUtils.WriteValueTypeToStream(stream, count);
            //Key必须是值类型，其他的直接报trace.
            Debug.Assert(typeof(TK).IsValueType, $"Serializable Dict Key Type Must Be ValueType!");
            SerializeUtils.WriteSeqToStream(stream, Items.Keys, count);

            // 对于存储的是值类型，直接掏出来序列化就好.
            Type vType = typeof(TV);
            if (vType.IsValueType) {
                SerializeUtils.WriteSeqToStream(stream, Items.Values, count);
                return;
            }
            // 对于引用类型，不需要多态特性的话，直接依次序列化数据就好.
            Debug.Assert(vType.IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} value or ValueType value!");
            if (!IsPolyItem) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(vType);
                foreach (TV v in Items.Values) ((ISerializable)v!).Serialize(ctx, stream, tMeta);
                return;
            }
            // 对于引用类型需要多态的，稍微麻烦一点，需要记录的东西比较多.
            foreach (TV v in Items.Values) ObjectSerializer.Serialize(ctx, stream, (ISerializable)v!);
        }

        public new void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Deserialize(ctx, stream, this);

            // 掏一下数量.
            int count = SerializeUtils.ReadValueTypeFromStream<int>(stream);
            // 搞到所有的key. 防止栈溢出这里不用stackalloc了，直接用未初始化的内存.
            byte[] buffer = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<TK>() * count);
            Span<TK> keys = SerializeUtils.ReadSpanFromStream<TK>(stream, buffer, count);

            // 搞到所有的value并加入到字典里面.
            Type type = typeof(TV);
            if (type.IsValueType) {
                buffer = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<TV>() * count);
                Span<TV> values = SerializeUtils.ReadSpanFromStream<TV>(stream, buffer, count);
                for(int i = 0; i != count; ++i) Items[keys[i]] = values[i];
                return;
            }
            // 对于引用类型，不需要多态特性的依次创建然后反序列化.
            Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)} property or ValueType property!");
            if (!IsPolyItem) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(type);
                for (int i = 0; i != count; ++i) {
                    ISerializable obj = (ISerializable)new TV();
                    obj.Deserialize(ctx, stream, tMeta);
                    Items[keys[i]] = (TV)obj;
                }
                return;
            }
            // 对于引用类型需要多态的，麻烦的进行Object反序列化.
            for (int i = 0; i != count; ++i) Items[keys[i]] = (TV)ObjectSerializer.Deserialize(ctx, stream);
        }
    }
}
