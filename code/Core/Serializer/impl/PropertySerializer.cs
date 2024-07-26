using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Game.Core.Serializer.Impl {

    /// <summary>
    /// 属性序列化的实现机制，这里用了Delegate构建代替反射加速属性获取与赋值.
    /// </summary>
    /// <typeparam name="T">属性所在类的类型</typeparam>
    /// <typeparam name="VT">属性的返回值类型</typeparam>
    internal class PropertySerializer<T, VT> : IPropertySerializer {

        // 获取和设置属性的Delegate类型.
        public delegate VT PropertyGetDelegate(T obj);
        public delegate void PropertySetDelegate(T obj, VT value);

        // 预购建的delegate.
        public PropertyGetDelegate GetDelegate { get; }
        public PropertySetDelegate SetDelegate { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(SerializeContext ctx, MemoryStream stream, object obj) {
            // 获取到值后，直接调用强类型的序列化器.
            SerializerImpl.Serialize(ctx, stream, GetDelegate.Invoke((T)obj)!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(SerializeContext ctx, MemoryStream stream, object obj) {
            // 调用强类型反序列化器后直接设置值.
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
