using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Serializer {

    internal static class SerializeMetaHelper {
        public static void AppendSerializeMetaData(ISerializeTypeMetaManager manager, Type type){
            Debug.Assert(type.IsSubclassOf(typeof(ISerializableObject)), $"Can only add {nameof(ISerializableObject)}, not {type.Name}");
            ISerializeTypeMeta meta = manager.GetSerializeTypeMeta(type);
        }
    }
}
