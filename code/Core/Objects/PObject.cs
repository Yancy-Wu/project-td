using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;

namespace Game.Core.Objects {
    public class PObject : SerializableObject, IPropTreeNodeContainer {
        // 这里保留一个空的类，用于IL Weaving.
        public PropTreeNode PropTreeNode { get; } = new();
    }
}
