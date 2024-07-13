using System.Diagnostics;

namespace Game.Core.Ec {
    internal class Env {
        static readonly MetaManager inst = new();
        public static MetaManager GetInst() { return inst; }

        private bool _initialized = false;
        public MetaManager metaManager = new();
        public EntityManager entityManager = new();

        public void Initialize(string iCompNamespace) {
            Debug.Assert(!_initialized, "cannot initialize env twice.");
            _initialized = true;
            metaManager.Initialize(iCompNamespace);
        }
    }
}
