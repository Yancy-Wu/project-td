using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Game.Core.Objects {
    public class PDict<TK, TV> : SerializableDict<TK, TV>, IPropTreeNodeContainer, IDictionary<TK, TV>, IReadOnlyDictionary<TK, TV> where TK : notnull {
        public PropTreeNode PropTreeNode { get; }
        public TV this[TK key] { get => Items[key]; set => _setValue(key, value); }

        public ICollection<TK> Keys => Items.Keys;

        public ICollection<TV> Values => Items.Values;

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        IEnumerable<TK> IReadOnlyDictionary<TK, TV>.Keys => Items.Keys;

        IEnumerable<TV> IReadOnlyDictionary<TK, TV>.Values => Items.Values;

        public PDict() { PropTreeNode = new(this); }

        private void _setValue(TK key, TV value) {
            if (Items.ContainsKey(key)) {
                // 数据结构修改.
                Items[key] = value;
                var oldItem = Items[key];
                string name = key.ToString()!;


                // 树结构维护.
                if (value is IPropTreeNodeContainer v) {
                    ((IPropTreeNodeContainer)oldItem!).PropTreeNode.Detach();
                    v.PropTreeNode.SetParentAndName(PropTreeNode, name);
                }

                // post事件发送.
                PropEventSetField<TV> e = new(value);
                e.PropPathParts.Add(name);
                PropTreeNode.DispatchPropEvent(e);
            }
            Add(key, value);
        }

        public void Add(TK key, TV value) {
            // 数据结构修改.
            Items.Add(key, value);

            // 树结构维护.
            if (value is IPropTreeNodeContainer v) v.PropTreeNode.SetParentAndName(PropTreeNode, key.ToString()!);

            // post事件发送.
            PropEventDictAdd<TK, TV> e = new(key, value);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void Add(KeyValuePair<TK, TV> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            // 全员Detach.
            if (typeof(TV).IsAssignableTo(typeof(IPropTreeNodeContainer))) {
                foreach (IPropTreeNodeContainer? item in Items.Values) item!.PropTreeNode.Detach();
            }

            Items.Clear();
            PropEventClear e = new();
            PropTreeNode.DispatchPropEvent(e);
        }

        public bool Contains(KeyValuePair<TK, TV> item) {
            return Items.Contains(item);
        }

        public bool ContainsKey(TK key) {
            return Items.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() {
            return Items.GetEnumerator();
        }

        public bool Remove(TK key) {
            // 数据结构修改.
            TV value;
            Items.TryGetValue(key, out value!);
            bool ret = Items.Remove(key);
            if (!ret) return false;

            // 树结构维护.
            if (value is IPropTreeNodeContainer v) v.PropTreeNode.Detach();

            // post事件发送.
            PropEventDictRemove<TK> e = new(key);
            PropTreeNode.DispatchPropEvent(e);
            return true;
        }

        public bool Remove(KeyValuePair<TK, TV> item) {
            return Remove(item.Key);
        }

        public bool TryGetValue(TK key, [MaybeNullWhen(false)] out TV value) {
            return Items.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }
    }
}
