using Game.Core.Object;
using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using System.Collections;

namespace Game.Core.Collection {
    public class PList<T> : SerializableList<T>, IList<T>, IReadOnlyList<T>, IPropTreeNodeContainer {
        public PropTreeNode PropTreeNode { get; } = new();
        public T this[int index] { get => Items[index]; set => _setItem(index, value); }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        private void _setItem(int index,  T item) {
            // 树结构维护.
            Items[index] = item;
            ObjectHelper.TryAttachTreeNode(item, this, index);
            // post事件发送.
            PropEventSetField<T> e = new(item);
            e.PropPathParts.Add(index.ToString());
            PropTreeNode.DispatchPropEvent(e);
        }

        public void Add(T item) {
            // 树结构维护.
            Items.Add(item);
            ObjectHelper.TryAttachTreeNode(item, this, Items.Count - 1);
            // post事件发送.
            PropEventListInsert<T> e = new(item);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void Clear() {
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
            // 树结构维护，重新修正索引.
            Items.Insert(index, item);
            ObjectHelper.TryAttachListTreeNodeBatch(Items, this, index);
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
            // 树结构维护，重新修正索引.
            Items.RemoveAt(index);
            ObjectHelper.TryAttachListTreeNodeBatch(Items, this, index);
            // post事件发送.
            PropEventListRemove<T> e = new(index);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void AddRange(IEnumerable<T> collection) {
            // 树结构维护，重新修正索引.
            int startIndex = Items.Count;
            Items.AddRange(collection);
            ObjectHelper.TryAttachListTreeNodeBatch(Items, this, startIndex);
            // post事件发送.
            PropEventListInsert<T> e = new(Items.GetRange(startIndex, Items.Count - startIndex).ToArray());
            PropTreeNode.DispatchPropEvent(e);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }
    }
}
