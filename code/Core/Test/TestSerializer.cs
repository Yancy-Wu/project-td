using Game.Core.Meta;
using Game.Core.Serializer;
using Game.Core.Serializer.Obj;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Game.Core.Test {

    class IntList : SerializableList<int> {}
    class StringList : SerializableList<string> { }

    class IntIntDict : SerializableDict<int, int> { }
    class StringStringDict : SerializableDict<string, string> { }
    class StringCustomDict : SerializableDict<string, CustomObject2> { }
    class StringCustom3Dict : SerializableDict<string, ITestInterface> { }
    class StringCustom4Dict : SerializableDict<string, CustomObject4> { }
    class TestList : SerializableList<Test> { }

    class CustomObject2 : SerializableObject {
        [GameProperty]
        public int Id { get; set; } = 88888;

        [GameProperty]
        public string Name { get; set; } = "";

        [GameProperty]
        public IntList l1 { get; set; } = new IntList();

        [GameProperty]
        public IntIntDict l2 { get; set; } = new IntIntDict();

        [GameProperty]
        public StringList l3 { get; set; } = new StringList();
    }

    interface ITestInterface: ISerializable { }


    class CustomObject3: SerializableObject, ITestInterface {

        [GameProperty]
        public int P1 { get; set; } = 33;

        [GameProperty]
        public string P2 { get; set; } = "22";

        [GameProperty]
        public string P3 { get; set; } = "MM";
    }

    class CustomObject4 : SerializableObject, ITestInterface {

        [GameProperty]
        public int P1 { get; set; } = 33;

        [GameProperty]
        public string P2 { get; set; } = "22";
    }

    struct Test {
        int v1 = 1;
        float v2 = 2.0f;
        bool v3 = false;

        public Test(int _v1, float _v2, bool _v3) {
            this.v1 = _v1;
            this.v2 = _v2;
            this.v3 = _v3;
        }
    }

    class CustomObjectD : SerializableObject {
        [GameProperty]
        public string s1 { get; set; } = "";

        [GameProperty]
        public CustomObject2 s2 { get; set; } = new CustomObject2();

        [GameProperty]
        public Test s3 { get; set; }

        [GameProperty]
        public StringStringDict s4 { get; set; } = new StringStringDict();

        [GameProperty]
        public StringCustomDict s5 { get; set; } = new StringCustomDict();

        [GameProperty]
        public TestList s6 { get; set; } = new TestList();
    }

    class CustomObject: CustomObjectD {
        [GameProperty]
        public ITestInterface si { get; set; }
    }

    [TestClass]
    public class TestSerializer {
        TypeMetaManager metaManager = new TypeMetaManager();
        SerializeContext ctx;

        public TestSerializer() {
            metaManager.ScanNamespaceTypes(this.GetType().Namespace!);
            ctx = new SerializeContext(metaManager);
        } 

        public T SerializeCopy<T>(T obj) {
            return (T)SerializerImpl.Deserialize(ctx, SerializerImpl.Serialize(ctx, obj!));
        }

        [TestMethod]
        public void TestSerializeSimpleType() {
            int v1 = 12315;
            bool v2 = true;
            float v3 = 9884.5f;
            double v4 = 112.44;
            long v5 = 114514;
            string s = "这是一段测试数据";
            Assert.AreEqual(v1, SerializeCopy(v1));
            Assert.AreEqual(v2, SerializeCopy(v2));
            Assert.AreEqual(v3, SerializeCopy(v3));
            Assert.AreEqual(v4, SerializeCopy(v4));
            Assert.AreEqual(v5, SerializeCopy(v5));
            Assert.AreEqual(s, SerializeCopy(s));
        }

        [TestMethod]
        public void TestSerializeSeperate() {
            IntList t1 = new();
            t1.Items.AddRange(new int []{ 1, 3, 4, 6 });
            CollectionAssert.AreEqual(t1.Items, SerializeCopy(t1).Items);  // IntList测试
            StringStringDict t2 = new();
            t2.Items["XX1"] = "YY1";
            t2.Items["XX2"] = "YY2";
            CollectionAssert.AreEqual(t2.Items, SerializeCopy(t2).Items);  // StringStringDict测试
            StringCustom4Dict t3 = new();
            t3.Items["X1"] = new CustomObject4() { P1 = 70 };
            Assert.AreEqual(t3.Items["X1"].P1, SerializeCopy(t3).Items["X1"].P1);  // 非多态的Dict测试.
            StringCustom3Dict t4 = new();
            t4.Items["X1"] = new CustomObject4() { P1 = 70 };
            t4.Items["X2"] = new CustomObject3() { P3 = "TETETET" };
            StringCustom3Dict ret4 = SerializeCopy(t4);
            Assert.AreEqual((t4.Items["X2"] as CustomObject3)!.P3, (ret4.Items["X2"] as CustomObject3)!.P3);
            Assert.AreEqual((t4.Items["X1"] as CustomObject4)!.P1, (ret4.Items["X1"] as CustomObject4)!.P1);  // 多态的Dict测试.
        }


        CustomObject PrepareObj(out CustomObject2 obj2) {
            obj2 = new();
            obj2.Name = "TestObj2";
            obj2.l1.Items.Add(5642);
            obj2.l2.Items[2233] = 7788;
            obj2.l3.Items.Add("字符串999");
            CustomObject obj = new();
            obj.s1 = "123123";
            obj.s2.Name = "Name";
            obj.s2.l1.Items.Add(666);
            obj.s2.l1.Items.Add(777);
            obj.s2.l2.Items[1] = 114514;
            obj.s2.l2.Items[2] = 1515380;
            obj.s2.l3.Items.Add("字符串1");
            obj.s2.l3.Items.Add("字符串2");
            obj.s3 = new Test(9, 8.0f, false);
            obj.s4.Items["X1"] = "字符串3";
            obj.s4.Items["X2"] = "字符串4";
            obj.s5.Items["MM1"] = obj2;
            obj.s5.Items["MM2"] = obj2;
            obj.s6.Items.Add(new Test(-1, 5.4f, false));
            obj.s6.Items.Add(new Test(-2, -77.4f, true));
            obj.si = new CustomObject4() { P1 = 1000 };
            return obj;
        }

        [TestMethod]
        public void TestSerializeObjectProperty() {
            CustomObject obj = PrepareObj(out CustomObject2 obj2);
            CustomObject objCopy = SerializeCopy(obj);
            Assert.AreEqual(obj.s1, obj.s1);
            CollectionAssert.AreEqual(obj.s4.Items, objCopy.s4.Items);  // Dict测试
            CollectionAssert.AreEqual(obj.s6.Items, objCopy.s6.Items);  // List struct测试
            CollectionAssert.AreEqual(obj.s2.l1.Items, objCopy.s2.l1.Items);  // 子对象测试
            Assert.AreEqual(obj.s2.Name, objCopy.s2.Name);
            Assert.AreEqual(obj.s2.Id, objCopy.s2.Id);
            CollectionAssert.AreEqual(obj.s2.l2.Items, objCopy.s2.l2.Items);
            CollectionAssert.AreEqual(obj.s2.l3.Items, objCopy.s2.l3.Items);
            var mm2 = objCopy.s5.Items["MM2"];
            Assert.AreEqual(obj2.Id, mm2.Id);
            CollectionAssert.AreEqual(obj2.l1.Items, mm2.l1.Items);  // 容器对象测试.
            CollectionAssert.AreEqual(obj2.l2.Items, mm2.l2.Items);
            CollectionAssert.AreEqual(obj2.l3.Items, mm2.l3.Items);
        }

        [TestMethod]
        public void TestPerformance() {
            CustomObjectD obj = PrepareObj(out CustomObject2 _);
            DateTime ts = DateTime.Now;
            /*
            for(int i = 0; i < 10000; i++) {
                // json序列化器不支持多态. 另外这段跑需要加一些东西, 然后json反序列化出来的东西也不太对劲没法测
                string data = JsonSerializer.Serialize(obj);
                JsonSerializer.Deserialize<CustomObject>(data);
            }
            */
            DateTime te = DateTime.Now;
            TimeSpan t = te.Subtract(ts);
            Console.WriteLine("JsonSerializer总共花费{0}ms.", t.TotalMilliseconds);
            ts = DateTime.Now;
            for (int i = 0; i < 10000; i++) {
                byte[] data = SerializerImpl.Serialize(ctx, obj);
                SerializerImpl.Deserialize(ctx, data);
            }
            te = DateTime.Now;
            t = te.Subtract(ts);
            Console.WriteLine("SerializeImpl总共花费{0}ms.", t.TotalMilliseconds);
        }
    }
}
