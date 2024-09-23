using Game.Core.Ec;
using Game.Core.Meta;
using Game.Core.Serializer;
using Game.Core.Serializer.Obj;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Game.Core.Test {

    public interface ICompTest: IComp {
        new static readonly int CompType = 1;
        new static readonly CompMeta Meta = new();
        int GetTestData();
    }

    public abstract class CompTest1: ComponentBase, ICompTest {

    }

    public class CompTest2: ComponentBase, ICompTest {

    }

    [TestClass]
    public class TestEc {
        SerializeContext ctx;

        public TestEc() {
        } 

        [TestMethod]
        public void TestSerializeSimpleType() {
        }
    }
}
