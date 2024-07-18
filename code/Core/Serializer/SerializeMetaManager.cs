using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Serializer {
    delegate object MemberGetDelegate(object obj);
    delegate object MemberSetDelegate(object obj, object value);

    struct SerializeMetaInfo {
        MemberGetDelegate getDelegate;
        MemberSetDelegate setDelegate;
    }

    internal class SerializeMetaManager {
        private readonly Dictionary<Type, int> _compClsToType = new();
        public SerializeMetaManager() {
            PropertyInfo property = this.GetType().GetProperty("Name")!;
            property.GetGetMethod();
            MemberGetDelegate memberGet = (MemberGetDelegate)System.Delegate.CreateDelegate(typeof(MemberGetDelegate), property.GetGetMethod()!);
            object value = memberGet(this);
        }
    }
}
