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
            SerializerImpl.Serialize(ctx, stream, GetDelegate.Invoke((T)obj)!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(SerializeContext ctx, MemoryStream stream, object obj) {
            SetDelegate((T)obj, SerializerImpl.Deserialize<VT>(ctx, stream));
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
            Expression callGet = Expression.Call(thisPara, property.GetGetMethod()!);
            this.GetDelegate = Expression.Lambda<PropertyGetDelegate>(callGet, thisPara).Compile();

            // setter封装.
            ParameterExpression valuePara = Expression.Parameter(returnType);
            Expression callSet = Expression.Call(thisPara, property.GetSetMethod()!, valuePara);
            this.SetDelegate = Expression.Lambda<PropertySetDelegate>(callSet, thisPara, valuePara).Compile();
        }
    }
}
