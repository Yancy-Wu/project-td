using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;

namespace Game.Core.Objects {
    public class PObject : SerializableObject, IPropTreeNodeContainer {
        public PropTreeNode PropTreeNode { get; } = new();

        internal void AfterSetProp<T>(string name, T item, T oldItem) {
            // 用于IL Weaving，属性事件变更后的处理内容.
            // 树结构维护.
            if (typeof(T).IsAssignableTo(typeof(IPropTreeNodeContainer))) {
                ((IPropTreeNodeContainer)oldItem!).PropTreeNode.Detach();
                ((IPropTreeNodeContainer)item!).PropTreeNode.SetParentAndName(PropTreeNode, name);
            }

            // post事件发送.
            PropEventSetField<T> e = new(item);
            e.PropPathParts.Add(name);
            PropTreeNode.DispatchPropEvent(e);
        }
    }
}
