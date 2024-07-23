using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Core.Serializer.Impl {
    internal class PropertySerializer<T, VT> : IPropertySerializer {
        public delegate VT PropertyGetDelegate(T obj);
        public delegate void PropertySetDelegate(T obj, VT value);
        public PropertyGetDelegate GetDelegate { get; }
        public PropertySetDelegate SetDelegate { get; }

        public void Serialize(SerializeContext ctx, MemoryStream stream, object obj) {
            VT value = GetDelegate.Invoke((T)obj)!;
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<VT>()) {
                int size = Unsafe.SizeOf<VT>();
                var array = GC.AllocateUninitializedArray<byte>(size);
                Unsafe.WriteUnaligned(ref MemoryMarshal.GetArrayDataReference(array), value);
                stream.Write(BitConverter.GetBytes(size));
                stream.Write(array);
                return;
            }
            Debug.Assert(typeof(VT).IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} property or ValueType property!");
            ObjectSerializer.Serialize(ctx, stream, (ISerializable)obj);
        }

        public void Deserialize(SerializeContext ctx, MemoryStream stream, object obj) {
            byte[] sizeBytes = { 0, 0, 0, 0 };
            stream.Read(sizeBytes);
            int size = BitConverter.ToInt32(sizeBytes, 0);
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<VT>()) {
                int valueSize = Unsafe.SizeOf<VT>();
                Debug.Assert(size == valueSize, $"Value Type size incorrect! serialized is {size}, type is {valueSize}.");
                byte[] data = new byte[size];
                stream.Read(data);
                VT v = Unsafe.ReadUnaligned<VT>(ref MemoryMarshal.GetArrayDataReference(data));
                SetDelegate((T)obj, v);
                return;
            }
            Debug.Assert(typeof(VT).IsSubclassOf(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)} property or ValueType property!");
            VT value = (VT)ObjectSerializer.Deserialize(ctx, stream);
            SetDelegate((T)obj, value);
        }

        public PropertySerializer(string propName) {
            Debug.Assert(!typeof(VT).IsGenericType, $"Not Support property in Generic Type.");
            PropertyInfo property = typeof(T).GetProperty(propName)!;
            ParameterExpression thisPara = Expression.Parameter(typeof(T));
            Expression callGet = Expression.Call(property.GetGetMethod()!);
            this.GetDelegate = Expression.Lambda<PropertyGetDelegate>(callGet, thisPara).Compile();
            ParameterExpression valuePara = Expression.Parameter(typeof(VT));
            Expression callSet = Expression.Call(property.GetSetMethod()!);
            this.SetDelegate = Expression.Lambda<PropertySetDelegate>(callSet, thisPara, valuePara).Compile();
            Debug.Assert(!typeof(T).IsValueType || !RuntimeHelpers.IsReferenceOrContainsReferences<VT>(), "Value Type Cannot Contain Ref Field!");
        }
    }
}
