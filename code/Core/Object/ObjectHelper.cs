using Game.Core.PropertyTree;
using System.Runtime.CompilerServices;

namespace Game.Core.Object {
    internal static class ObjectHelper {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _attachTreeNode(IPropTreeNodeContainer value, IPropTreeNodeContainer parent, string name) {
            // 挂接树节点属性.
            var node = value.PropTreeNode;
            node.Parent = parent;
            node.Name = name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryAttachTreeNode<T, TK>(T value, IPropTreeNodeContainer parent, TK index) {
            // 先看看类型是不是需要.
            if (value is not IPropTreeNodeContainer v) return;
            _attachTreeNode(v, parent, index!.ToString()!);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryAttachListTreeNodeBatch<T>(List<T> list, IPropTreeNodeContainer parent, int startIndex) {
            // 先看看类型是不是需要.
            if (!typeof(T).IsAssignableTo(typeof(IPropTreeNodeContainer))) return;
            for (int i = startIndex; i != list.Count; ++i) {
                _attachTreeNode((list[i] as IPropTreeNodeContainer)!, parent, i.ToString());
            }
        }
    }
}
