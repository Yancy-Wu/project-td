using Game.Core.Meta;
using System.Diagnostics;

namespace Game.Core.Ec {
    internal class Env {
        private bool _initialized = false;
        public TypeMetaManager metaManager = TypeMetaManager.Inst;
        public EntityManager entityManager = new();

        public void Initialize(string iCompNamespace) {
            Debug.Assert(!_initialized, "cannot initialize env twice.");
            metaManager.ScanNamespaceTypes(iCompNamespace);
            _initialized = true;
        }
    }
}
