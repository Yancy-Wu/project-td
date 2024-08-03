using Game.Core.PropertyTree;
using Game.Core.Serializer.Obj;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Collection {
    public class PDict<TK, TV> : SerializableDict<TK, TV>, IPropTreeNode, IDictionary<TK, TV>, IDictionary, IReadOnlyDictionary<TK, TV> where TK : notnull {
        public TV this[TK key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object? this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<TK> Keys => throw new NotImplementedException();

        public ICollection<TV> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        ICollection IDictionary.Keys => throw new NotImplementedException();

        IEnumerable<TK> IReadOnlyDictionary<TK, TV>.Keys => throw new NotImplementedException();

        ICollection IDictionary.Values => throw new NotImplementedException();

        IEnumerable<TV> IReadOnlyDictionary<TK, TV>.Values => throw new NotImplementedException();

        public void Add(TK key, TV value) {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TK, TV> item) {
            throw new NotImplementedException();
        }

        public void Add(object key, object? value) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TK, TV> item) {
            throw new NotImplementedException();
        }

        public bool Contains(object key) {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TK key) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() {
            throw new NotImplementedException();
        }

        public bool Remove(TK key) {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TK, TV> item) {
            throw new NotImplementedException();
        }

        public void Remove(object key) {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TK key, [MaybeNullWhen(false)] out TV value) {
            throw new NotImplementedException();
        }

        void IPropTreeNode.DispatchPropEvent(IPropEventDefine propEvent) {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}
