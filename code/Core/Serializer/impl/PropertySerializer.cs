using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer.Impl {
    internal class PropertySerializer<T, VT> : IPropertySerializer {
        public delegate VT PropertyGetDelegate(T obj);
        public delegate void PropertySetDelegate(T obj, VT value);
        public PropertyGetDelegate GetDelegate { get; }
        public PropertySetDelegate SetDelegate { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(SerializeContext ctx, MemoryStream stream, object obj) {
            VT value = GetDelegate.Invoke((T)obj)!;
            // 返回值类型的情况下，直接序列化.
            if (typeof(VT).IsValueType) {
                SerializeUtils.WriteValueTypeToStream(stream, value);
                return;
            }
            // 对于返回的是string，特殊处理一下.
            if (typeof(VT) == typeof(string)) {
                StringSerializer.Serialize(ctx, stream, (value as string)!);
                return;
            }
            // 否则走ISerializable的序列化
            Debug.Assert(typeof(VT).IsSubclassOf(typeof(ISerializable)), $"Can only serialize {nameof(ISerializable)} property!");
            ObjectSerializer.Serialize(ctx, stream, (ISerializable)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(SerializeContext ctx, MemoryStream stream, object obj) {
            // 返回值类型的情况下，直接反序列化, 大小从类型中读取.
            if (typeof(VT).IsValueType) {
                SetDelegate((T)obj, SerializeUtils.ReadValueTypeFromStream<VT>(stream));
                return;
            }
            // 对于返回的是string，特殊处理一下.
            if (typeof(VT) == typeof(string)) {
                SetDelegate((T)obj, (VT)(object)StringSerializer.Deserialize(ctx, stream));
                return;
            }
            // 否则走ISerializable的反序列化
            Debug.Assert(typeof(VT).IsSubclassOf(typeof(ISerializable)), $"Can only deserialize {nameof(ISerializable)}!");
            VT value = (VT)ObjectSerializer.Deserialize(ctx, stream);
            SetDelegate((T)obj, value);
        }

        public PropertySerializer(string propName) {
            Type objType = typeof(T);
            Type returnType = typeof(VT);
            // 一些强制要求检查.
            Debug.Assert(!returnType.IsGenericType && returnType.GenericTypeArguments.Length == 0, $"Not Support property return Generic Type.");
            Debug.Assert(!returnType.IsValueType || !RuntimeHelpers.IsReferenceOrContainsReferences<VT>(), "Value Type Cannot Contain Ref Field!");

            // 构建获取属性，和设置属性的lambda表达式并编译之，可加速反射速度.
            PropertyInfo property = objType.GetProperty(propName)!;
            ParameterExpression thisPara = Expression.Parameter(objType);
            Expression callGet = Expression.Call(property.GetGetMethod()!);
            this.GetDelegate = Expression.Lambda<PropertyGetDelegate>(callGet, thisPara).Compile();

            // setter封装.
            ParameterExpression valuePara = Expression.Parameter(returnType);
            Expression callSet = Expression.Call(property.GetSetMethod()!);
            this.SetDelegate = Expression.Lambda<PropertySetDelegate>(callSet, thisPara, valuePara).Compile();
        }
    }
}
