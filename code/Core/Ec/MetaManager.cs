using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Ec
{
    class MetaManager
    {
        static MetaManager inst = new MetaManager();
        private Dictionary<string, int> CompNameToType = new Dictionary<string, int>();
        public static MetaManager GetInst() { return inst; }
        public void AddComp<T>(int compType) where T: IComp
        {
            string typeName = typeof(T).Name;
            this.CompNameToType[typeName] = compType;
        }
        public int GetCompType<T>() where T : IComp
        {
            string typeName = typeof(T).Name;
            return this.CompNameToType[typeName];
        }
    }
}
