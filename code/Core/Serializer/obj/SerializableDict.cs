using Game.Core.Meta;
using Game.Core.Serializer.Impl;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer.Obj {

    /// <summary>
    /// 可序列化字典的实现.
    /// </summary>
    public class SerializableDict<TK, TV> : ISerializable where TK: notnull {
        internal Dictionary<TK, TV> Items { get; } = new Dictionary<TK, TV>();

        public void Serialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Serialize(ctx, stream, this);

            // 先写入个数量.
            int count = Items.Count;
            SerializeUtils.WriteValueTypeToStream(stream, count);
            //Key必须是值类型或者string，其他的直接报trace.
            if(typeof(TK) == typeof(string)) foreach (TK v in Items.Keys) StringSerializer.Serialize(ctx, stream, (v as string)!);
            else {
                Debug.Assert(typeof(TK).IsValueType, $"Serializable Dict Key Type Must Be ValueType!");
                SerializeUtils.WriteSeqToStream(stream, Items.Keys, count);
            }

            // 对于存储的是值类型，直接掏出来序列化就好.
            Type vType = typeof(TV);
            if (vType.IsValueType) {
                SerializeUtils.WriteSeqToStream(stream, Items.Values, count);
                return;
            }
            // 对于存储的是string，特殊处理一下.
            if (vType == typeof(string)) {
                foreach (TV v in Items.Values) StringSerializer.Serialize(ctx, stream, (v as string)!);
                return;
            }
            // 对于引用类型，不需要多态特性的话，直接依次序列化数据就好.
            Debug.Assert(vType.IsAssignableTo(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} value or ValueType value!");
            if (!vType.IsInterface && !vType.IsAbstract) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(vType);
                foreach (TV v in Items.Values) ((ISerializable)v!).Serialize(ctx, stream, tMeta);
                return;
            }
            // 对于引用类型需要多态的，稍微麻烦一点，需要记录的东西比较多.
            foreach (TV v in Items.Values) ObjectSerializer.Serialize(ctx, stream, (ISerializable)v!);
        }

        public void Deserialize(SerializeContext ctx, MemoryStream stream, TypeMeta meta) {
            // 先反序列化带属性标记的.
            foreach (var prop in meta.SerializeProperties!) prop.Deserialize(ctx, stream, this);

            // 掏一下数量.
            int count = SerializeUtils.ReadValueTypeFromStream<int>(stream);
            Span<TK> keys;
            // 搞到所有的key. 直接用未初始化的内存.
            if (typeof(TK) == typeof(string)) {
                keys = GC.AllocateUninitializedArray<TK>(count);
                for (int i = 0; i != count; ++i) keys[i] = (TK)(object)StringSerializer.Deserialize(ctx, stream);
            }
            else {
                byte[] buffer = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<TK>() * count);
                keys = SerializeUtils.ReadSpanFromStream<TK>(stream, buffer, count);
            }

            // 搞到所有的value并加入到字典里面.
            Type type = typeof(TV);
            if (type.IsValueType) {
                byte[] buffer = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<TV>() * count);
                Span<TV> values = SerializeUtils.ReadSpanFromStream<TV>(stream, buffer, count);
                for(int i = 0; i != count; ++i) Items[keys[i]] = values[i];
                return;
            }
            // 对于存储的是string，特殊处理一下.
            if (type == typeof(string)) {
                for (int i = 0; i != count; ++i) Items[keys[i]] = (TV)(object)StringSerializer.Deserialize(ctx, stream);
                return;
            }
            // 对于引用类型，不需要多态特性的依次创建然后反序列化.
            Debug.Assert(type.IsAssignableTo(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)} property or ValueType property!");
            if (!type.IsInterface && !type.IsAbstract) {
                TypeMeta tMeta = ctx.MetaManager.GetTypeMeta(type);
                for (int i = 0; i != count; ++i) {
                    ISerializable obj = (ISerializable)Activator.CreateInstance(type)!;
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
