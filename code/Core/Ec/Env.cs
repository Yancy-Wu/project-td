using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Ec
{
    internal class Env
    {
        static readonly MetaManager inst = new MetaManager();
        public MetaManager metaManager;
        public EntityManager entityManager;
        public static MetaManager GetInst() { return inst; }
    }
}
