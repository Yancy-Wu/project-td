using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Serializer {
    delegate object PropertyGetDelegate(object obj);
    delegate object PropertySetDelegate(object obj, object value);

    internal interface ISerializePropertyInfo {
        PropertyGetDelegate GetDelegate { get; set; }
        PropertySetDelegate SetDelegate { get; set; }
    }

    internal interface ISerializeTypeMeta {
        Dictionary<string, ISerializePropertyInfo> PropNameToMetaInfo { get; }
        List<ISerializePropertyInfo> SerializeProperties { get; }
    }

    internal interface ISerializeTypeMetaManager {
        ISerializeTypeMeta GetSerializeTypeMeta(Type type);
    }
}
