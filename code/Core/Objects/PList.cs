using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using System.Collections;

namespace Game.Core.Objects {
    public class PList<T> : SerializableList<T>, IList<T>, IReadOnlyList<T>, IPropTreeNodeContainer {
        public PropTreeNode PropTreeNode { get; } = new();
        public T this[int index] { get => Items[index]; set => _setItem(index, value); }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        private void _setItem(int index,  T item) {
            // 数据结构修改.
            Items[index] = item;
            var oldItem = Items[index];

            // 树结构维护.
            string name = index.ToString();
            if (item is IPropTreeNodeContainer v) {
                v.PropTreeNode.SetParentAndName(PropTreeNode, name);
                ((IPropTreeNodeContainer)oldItem!).PropTreeNode.Detach();
            }

            // post事件发送.
            PropEventSetField<T> e = new(item);
            e.PropPathParts.Add(name);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void Add(T item) {
            // 数据结构修改.
            Items.Add(item);

            // 树结构维护.
            if (item is IPropTreeNodeContainer v) {
                string name = (Items.Count - 1).ToString();
                v.PropTreeNode.SetParentAndName(PropTreeNode, name);
            }

            // post事件发送.
            PropEventListInsert<T> e = new(item);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void Clear() {
            // 全员Detach.
            if (typeof(T).IsAssignableTo(typeof(IPropTreeNodeContainer))) {
                foreach(IPropTreeNodeContainer? item in Items) item!.PropTreeNode.Detach();
            }

            Items.Clear();
            PropEventClear e = new();
            PropTreeNode.DispatchPropEvent(e);
        }

        public bool Contains(T item) {
            return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            return Items.GetEnumerator();
        }

        public int IndexOf(T item) {
            return Items.IndexOf(item);
        }

        public void Insert(int index, T item) {
            // 数据结构修改.
            Items.Insert(index, item);

            // 树结构维护，重新修正索引.
            if (item is IPropTreeNodeContainer v) {
                v.PropTreeNode.SetParent(PropTreeNode);
                for (int i = index; i != Items.Count; ++i) ((IPropTreeNodeContainer)Items[i]!).PropTreeNode.Name = i.ToString();
            }

            // post事件发送.
            PropEventListInsert<T> e = new(index, item);
            PropTreeNode.DispatchPropEvent(e);
        }

        public bool Remove(T item) {
            // Remove操作直接转RemoveAt.
            int index = Items.IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index) {
            // 数据结构修改.
            T v = Items[index];
            Items.RemoveAt(index);

            // 树结构维护，重新修正索引.
            if (typeof(T).IsAssignableTo(typeof(IPropTreeNodeContainer))) {
                ((IPropTreeNodeContainer)v!).PropTreeNode.Detach();
                for (int i = index; i != Items.Count; ++i) ((IPropTreeNodeContainer)Items[i]!).PropTreeNode.Name = i.ToString();
            }

            // post事件发送.
            PropEventListRemove<T> e = new(index);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void AddRange(IEnumerable<T> collection) {
            // 数据结构修改.
            int startIndex = Items.Count;
            Items.AddRange(collection);

            // 树结构维护，重新修正索引.
            if (typeof(T).IsAssignableTo(typeof(IPropTreeNodeContainer))) {
                for (int i = startIndex; i != Items.Count; ++i) 
                    ((IPropTreeNodeContainer)Items[i]!).PropTreeNode.SetParentAndName(PropTreeNode, i.ToString());
            }

            // post事件发送.
            PropEventListInsert<T> e = new(Items.GetRange(startIndex, Items.Count - startIndex).ToArray());
            PropTreeNode.DispatchPropEvent(e);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }
    }
}
