using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Collection {
    public class PList<T> : SerializableList<T>, IList<T>, IList, IReadOnlyList<T>, IPropTreeNode {
        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        object? IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public void Add(T item) {
            throw new NotImplementedException();
        }

        public int Add(object? value) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(T item) {
            throw new NotImplementedException();
        }

        public bool Contains(object? value) {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator() {
            throw new NotImplementedException();
        }

        public int IndexOf(T item) {
            throw new NotImplementedException();
        }

        public int IndexOf(object? value) {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item) {
            throw new NotImplementedException();
        }

        public void Insert(int index, object? value) {
            throw new NotImplementedException();
        }

        public bool Remove(T item) {
            throw new NotImplementedException();
        }

        public void Remove(object? value) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        void IPropTreeNode.DispatchPropEvent(IPropEventDefine propEvent) {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}
