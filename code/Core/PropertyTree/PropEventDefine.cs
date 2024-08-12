namespace Game.Core.PropertyTree {

    public interface IPropEventDefine {
        string? MatchedIndex { get; set; }
        List<string> PropPathParts { get; }
        void ParseMatchedIndex(ref int val) {
            if (MatchedIndex == null) return;
            val = int.Parse(MatchedIndex);
        }
    }

    public class PropEventDefineBase: IPropEventDefine {
        public string? MatchedIndex { get; set; }
        public List<string> PropPathParts { get; } = new();
    }

    public class PropEventSetField<T> : PropEventDefineBase {
        public T Value { get; private set; }

        public PropEventSetField(T value) {
            Value = value;
        }
    }

    public class PropEventClear : PropEventDefineBase { }

    public class PropEventDictAdd<TK, TV> : PropEventDefineBase {
        public TV Value { get; private set; }
        public TK Key { get; private set; }

        public PropEventDictAdd(TK key, TV value) {
            Key = key;
            Value = value;
        }

    }

    public class PropEventDictRemove<T> : PropEventDefineBase {
        public T Key { get; private set; }

        public PropEventDictRemove(T key) {
            Key = key;
        }
    }

    public class PropEventListInsert<T> : PropEventDefineBase {
        public int Index { get; private set; } = -1;
        public T[] Values { get; }
        public PropEventListInsert(int index, T[] values) {
            Index = index;
            Values = values;
        }
        public PropEventListInsert(T value) : this(-1, new T[1] { value }) { }
        public PropEventListInsert(T[] values) : this(-1, values) { }
        public PropEventListInsert(int index, T value) : this(index, new T[1] { value }) { }
    }

    public class PropEventListRemove<T> : PropEventDefineBase {
        public int[] Indexes{ get; private set; }
        public PropEventListRemove(int[] indexes) {
            Indexes = indexes;
        }
        public PropEventListRemove(int index) : this(new int[1] { index }) { }
    }
}
