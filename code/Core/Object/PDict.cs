using Game.Core.Object;
using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Game.Core.Collection {
    public class PDict<TK, TV> : SerializableDict<TK, TV>, IPropTreeNodeContainer, IDictionary<TK, TV>, IReadOnlyDictionary<TK, TV> where TK : notnull {
        public PropTreeNode PropTreeNode { get; } = new();
        public TV this[TK key] { get => Items[key]; set => _setValue(key, value); }

        public ICollection<TK> Keys => Items.Keys;

        public ICollection<TV> Values => Items.Values;

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        IEnumerable<TK> IReadOnlyDictionary<TK, TV>.Keys => Items.Keys;

        IEnumerable<TV> IReadOnlyDictionary<TK, TV>.Values => Items.Values;

        private void _setValue(TK key, TV value) {
            if (Items.ContainsKey(key)) {
                // 树结构维护.
                Items[key] = value;
                ObjectHelper.TryAttachTreeNode(value, this, key);
                // post事件发送.
                PropEventSetField<TV> e = new(value);
                e.PropPathParts.Add(key.ToString()!);
                PropTreeNode.DispatchPropEvent(e);
            }
            Add(key, value);
        }

        public void Add(TK key, TV value) {
            // 树结构维护.
            Items.Add(key, value);
            ObjectHelper.TryAttachTreeNode(value, this, key);
            // post事件发送.
            PropEventDictAdd<TK, TV> e = new(key, value);
            PropTreeNode.DispatchPropEvent(e);
        }

        public void Add(KeyValuePair<TK, TV> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
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
            // 直接移除即可.
            bool ret = Items.Remove(key);
            // post事件发送.
            PropEventDictRemove<TK> e = new(key);
            PropTreeNode.DispatchPropEvent(e);
            return ret;
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
