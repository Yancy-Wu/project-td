using System.Diagnostics;
using System.Reflection;

namespace Game.Core.Ec
{
    enum EntityState
    {
        INIT,
        RUNNING,
        DESTROYED
    }

    class Entity
    {
        private string? _id;
        private readonly Dictionary<int, IComp> _comps = new();
        private readonly Dictionary<int, IComp> _activeComps = new();
        private Env? _env;
        private EntityState _state = EntityState.INIT;
        public string? Id { get { return _id; } }

        internal void AttachToEnv(Env env, string id)
        {
            _env = env;
            _id = id;
        }

        internal void DispatchCompMethod(string funcName, bool activeOnly, params object?[]? param)
        {
            var dict = activeOnly ? _activeComps : _comps;
            foreach (IComp comp in dict.Values) {
                MethodInfo field = comp.GetType().GetMethod(funcName)!;
                field.Invoke(comp, param);
            }
        }

        internal void RunEntity()
        {
            Debug.Assert(_state == EntityState.INIT, "RunEntity can only call when entity init!");
            _state = EntityState.RUNNING;
            DispatchCompMethod("OnActive", true);
        }

        internal void Destroy()
        {
            Debug.Assert(_state != EntityState.DESTROYED, "Destroy can only call when entity not destroyed!");
            EntityState curState = _state;
            _state = EntityState.DESTROYED;
            if (curState == EntityState.RUNNING)
            {
                foreach (IComp comp in _activeComps.Values)
                {
                    comp.CompInfo.IsActive = false;
                    comp.OnInactive(willDestroy: true);
                }
                _activeComps.Clear();
            }
            DispatchCompMethod("Destroy", false);
            _comps.Clear();
        }

        internal void AddComp<T>(T comp) where T : class, IComp
        {
            Debug.Assert(_state == EntityState.INIT, "Can only add entity comp before entity ready!");
            int compType = _env!.metaManager.GetCompType<T>();
            Debug.Assert(!_comps.ContainsKey(compType), $"Add comp error: comp type {compType} has existed");
            _comps[compType] = comp;
            if (comp.CompInfo.IsActive) _activeComps[compType] = comp;
        }

        public T? TryGetComp<T>() where T : class, IComp
        {
            int compType = _env!.metaManager.GetCompType<T>();
            if (_comps.TryGetValue(compType, out IComp? val)) return (T?)val;
            return null;
        }

        public IComp? TryGetComp(int compType)
        {
            if (_comps.TryGetValue(compType, out IComp? val)) return val;
            return null;
        }

        public T GetComp<T>() where T : class, IComp
        {
            int compType = _env!.metaManager.GetCompType<T>();
            return (T)_comps[compType];
        }

        public IComp GetComp(int compType)
        {
            return _comps[compType];
        }

        public void SetCompActive<T>(T comp, bool active) where T : class, IComp
        {
            Debug.Assert(_state == EntityState.RUNNING, "SetCompActive can only call when entity running!");
            if (comp.CompInfo.IsActive == active) return;
            int compType = _env!.metaManager.GetCompType<T>();
            Debug.Assert(!_comps.ContainsKey(compType), $"SetCompActive error: comp type {compType} not exist");
            comp.CompInfo.IsActive = active;
            if (active)
            {
                _activeComps[compType] = comp;
                comp.OnActive();
                return;
            }
            comp.OnInactive();
            _activeComps.Remove(compType);
            return;
        }
    }
}
