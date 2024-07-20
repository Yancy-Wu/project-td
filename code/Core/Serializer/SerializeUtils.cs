using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Serializer {
    public static class SerializeUtils {
        public static ISerializable CreatePropertySerializer<T>(string propName) {
            PropertyInfo property = typeof(T).GetProperty(propName)!;
            Type vt = property.GetGetMethod()!.ReturnType;
            Type type = typeof(PropertySerializer<,>).MakeGenericType(typeof(T), vt);
            return (ISerializable)Activator.CreateInstance(type, propName)!;
        }
    }
}
