using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Object {
    public class PObject : SerializableObject {
    }

    internal class PObjectProxy : IPropTreeNode {
        void IPropTreeNode.DispatchPropEvent(IPropEventDefine propEvent) {
            throw new NotImplementedException();
        }
    }
}
