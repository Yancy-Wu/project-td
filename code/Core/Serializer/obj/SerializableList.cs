using Game.Core.Meta;
using Game.Core.Serializer.Impl;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Core.Serializer.Obj {
    public interface ISerializableList<T> : ISerializable where T: new() {
        internal List<T> Items { get; }
        internal bool IsPolyItem { get; }

        public new unsafe void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) {
                prop.Serialize(ctx, stream, this);
            }
            // 先写入个数量.
            stream.Write(BitConverter.GetBytes(Items.Count));
            // 对于存储的是值类型，直接掏出来序列化就好.
            Type type = typeof(T);
            if (type.IsValueType) {
                int sz = Unsafe.SizeOf<T>();
                Span<T> span = CollectionsMarshal.AsSpan(Items);
                fixed (void * s = span) {
                    ReadOnlySpan<byte> byteSpan = new(s, span.Length * sz);
                    stream.Write(byteSpan);
                }
                return;
            }
            // 对于引用类型，不需要多态特性的话，直接依次序列化数据就好.
            Debug.Assert(typeof(T).IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} property or ValueType property!");
            if (!IsPolyItem) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(typeof(T));
                foreach (T v in Items) {
                    ((ISerializable)v!).Serialize(ctx, stream, tMeta);
                }
                return;
            }
            // 对于引用类型需要多态的，稍微麻烦一点，需要记录的东西比较多.
            foreach (T v in Items) {
                ObjectSerializer.Serialize(ctx, stream, (ISerializable)v!);
            }
        }

        public new unsafe void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) {
                prop.Deserialize(ctx, stream, this);
            }
            // 值类型直接批量反序列化
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            int count = BitConverter.ToInt32(buffer);
            Type type = typeof(T);
            if (type.IsValueType) {
                Items.Capacity = count;
                int sz = Unsafe.SizeOf<T>();
                Span<T> span = CollectionsMarshal.AsSpan(Items);
                fixed (void* s = span) {
                    Span<byte> byteSpan = new(s, span.Length * sz);
                    stream.Read(byteSpan);
                }
                return;
            }
            // 对于引用类型，不需要多态特性的依次创建然后反序列化.
            Debug.Assert(typeof(T).IsSubclassOf(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)} property or ValueType property!");
            if (!IsPolyItem) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(typeof(T));
                for (int i = 0; i != count; ++i) {
                    ISerializable obj = (ISerializable)new T();
                    obj.Deserialize(ctx, stream, tMeta);
                }
                return;
            }
            // 对于引用类型需要多态的，麻烦的进行Object反序列化.
            for (int i = 0; i != count; ++i) {
                ObjectSerializer.Deserialize(ctx, stream);
            }
        }
    }
}
