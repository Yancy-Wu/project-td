using Game.Core.Meta;
using Game.Core.Serializer;
using Game.Core.Serializer.Obj;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Test {

    class IntList : SerializableList<int> { }
    class StringList : SerializableList<string> { }
    class FloatList : SerializableList<float> { }
    class BoolList : SerializableList<bool> { }

    class IntIntDict : SerializableDict<int, int> { }
    class StringIntDict : SerializableDict<string, int> { }
    class StringStringDict : SerializableDict<string, string> { }

    class CustomObject2 : SerializableObject {
        [GameProperty]
        public string Name { get; } = "";
    }

    struct Test {
        int v1;
        float v2;
        bool v3;
    }

    class CustomObject: SerializableObject {
        [GameProperty]
        public string s1 { get; } = "";

        [GameProperty]
        public CustomObject2 s2 { get; } = new CustomObject2();

        [GameProperty]
        public Test s3 { get; }

        [GameProperty]
        public StringStringDict s4 { get; } = new StringStringDict();
    }

    [TestClass]
    public class TestSerializer {
    }
}
