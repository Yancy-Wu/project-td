using Game.Core.Serializer.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Serializer {
    public static class SerializeUtils {
        public static IPropertySerializer CreatePropertySerializer(Type objType, string propName) {
            PropertyInfo property = objType.GetProperty(propName)!;
            Type vt = property.GetGetMethod()!.ReturnType;
            Type type = typeof(PropertySerializer<,>).MakeGenericType(objType, vt);
            return (IPropertySerializer)Activator.CreateInstance(type, propName)!;
        }
    }
}
