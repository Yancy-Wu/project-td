using Game.Core.Meta;
using System.Diagnostics;
using System.Reflection;

namespace Game.Core.Ec {

    /// <summary>
    /// Entity的生命周期状态标记.
    /// </summary>
    enum EntityState {
        INIT,       // 初始状态
        ATTACHED,   // 已经绑定上下文，纳入EntityManager管理并拥有ID
        RUNNING,    // comp激活，进入运行状态
        DESTROYED   // Entity已经销毁
    }


    /// <summary>
    /// Entity类的定义.
    /// <para> Entity是IComp组合，其由EntityManager统一管理，并给外界提供通过IComp接口获取其真实Comp的方法 </para>
    /// <para> Entity的组件不是随意可添加的，在Running之后便不可添加删除，仅可设置组件的激活状态 </para>
    /// </summary>
    public class Entity {
        private string? _id;
        public string? Id { get { return _id; } }
        private readonly Dictionary<int, IComp> _comps = new();
        private readonly Dictionary<int, IComp> _activeComps = new();
        private EntityState _state = EntityState.INIT;

        // Env是Entity所在的上下文环境，比如其所属的EntityManager等等.
        private Env? _env;

        // 用于获取当前Entity若想添加指定的comp还缺少哪些comp依赖.
        private List<int> _getMissedCompDepend(CompMeta compMeta) {
            List<int> missed = new();
            foreach(int compType in compMeta.DependCompTypes) {
                if (!_comps.ContainsKey(compType)) missed.Add(compType);
            }
            return missed;
        }

        // 将Entity绑定到Env上下文并提供id
        internal void AttachToEnv(Env env, string? id = null, bool addToManager = true) {
            Debug.Assert(_state == EntityState.INIT && _env is not null, "AttachToEnv can only apply to INIT entity, and cur _env must null");
            _env = env;
            _id = id is not null ? id : IdManager.GenUUID();
            if (addToManager) env.entityManager.AddEntity(this);
            _state = EntityState.ATTACHED;
        }

        // 使用反射依次调用所有comp的某个函数.
        internal void DispatchCompMethod(string funcName, bool activeOnly, params object?[]? param) {
            var dict = activeOnly ? _activeComps : _comps;
            foreach (IComp comp in dict.Values) {
                MethodInfo field = comp.GetType().GetMethod(funcName)!;
                field.Invoke(comp, param);
            }
        }

        // 使得当前Entity进入运行状态.
        internal void RunEntity() {
            Debug.Assert(_state == EntityState.ATTACHED, "RunEntity can only call when entity attached!");
            _state = EntityState.RUNNING;
            DispatchCompMethod("OnActive", true);
        }

        // 销毁当前Entity.
        internal void Destroy(bool callManager = true){
            Debug.Assert(_state != EntityState.DESTROYED, "Destroy can only call when entity not destroyed!");
            // 若纳入管理，通知EntityManager解引用，之后再通知销毁.
            if (callManager && _env!.entityManager is not null){
                _env!.entityManager.DestroyEntity(this);
                return;
            }
            EntityState curState = _state;
            _state = EntityState.DESTROYED;
            // 通知运行中的comp退出激活态，额外给一个标志位以便特殊处理.
            if (curState == EntityState.RUNNING) {
                foreach (IComp comp in _activeComps.Values) {
                    comp.CompInfo.IsActive = false;
                    comp.OnInactive(willDestroy: true);
                }
            }
            // 清理comp后清理数据.
            DispatchCompMethod("Destroy", false);
            _comps.Clear();
            _activeComps.Clear();
            _env = null;
            _id = null;
        }

        // 给Entity添加一个Comp.
        internal void AddComp<T>(T comp) where T : class, IComp {
            int compType = _env!.metaManager.GetCompType<T>();
            CompMeta meta = _env!.metaManager.GetCompMeta<T>();
            List<int> missed = _getMissedCompDepend(meta);
            // 检查状态、存在性以及依赖性.
            Debug.Assert(_state == EntityState.INIT, "Can only add entity comp before entity attached!");
            Debug.Assert(!_comps.ContainsKey(compType), $"Add comp error: comp type {compType} has existed");
            Debug.Assert(missed.Count == 0, $"Add comp error: comp dependency missed: {missed}");
            _comps[compType] = comp;
            if (comp.CompInfo.IsActive) _activeComps[compType] = comp;
        }

        /// <summary>
        /// 尝试通过IComp的派生接口获取到实际comp类.
        /// 若不存在会返回null.
        /// </summary>
        /// <typeparam name="T">IComp的直系派生接口</typeparam>
        /// <returns>实际的comp类，或者null</returns>
        public T? TryGetComp<T>() where T : class, IComp {
            int compType = _env!.metaManager.GetCompType<T>();
            if (_comps.TryGetValue(compType, out IComp? val)) return (T?)val;
            return null;
        }

        /// <summary>
        /// 尝试通过Comp类别数据获取到实际comp类.
        /// 若不存在会返回null.
        /// </summary>
        /// <param name="compType">comp类别</param>
        /// <returns>实际的comp类，或者null</returns>
        public IComp? TryGetComp(int compType) {
            if (_comps.TryGetValue(compType, out IComp? val)) return val;
            return null;
        }

        /// <summary>
        /// 通过IComp的派生接口获取到实际comp类，需要确保一定存在，否则会出异常.
        /// </summary>
        /// <typeparam name="T">IComp的直系派生接口</typeparam>
        /// <returns>实际的comp类</returns>
        public T GetComp<T>() where T : class, IComp {
            int compType = _env!.metaManager.GetCompType<T>();
            return (T)_comps[compType];
        }

        /// <summary>
        /// 通过Comp类别数据获取到实际comp类, 需要确保一定存在.
        /// </summary>
        /// <param name="compType">comp类别</param>
        /// <returns>实际的comp类</returns>
        public IComp GetComp(int compType) {
            return _comps[compType];
        }

        // 设置组件激活状态的具体实现.
        private void _setCompActive(int compType, bool active) {
            Debug.Assert(_state == EntityState.RUNNING, "SetCompActive can only call when entity running!");
            Debug.Assert(_comps.ContainsKey(compType), $"SetCompActive error: comp type {compType} not exist.");
            IComp comp = _comps[compType];
            if (comp.CompInfo.IsActive == active) return;
            comp.CompInfo.IsActive = active;
            // 触发组件的处理函数.
            if (active) {
                _activeComps[compType] = comp;
                comp.OnActive();
                return;
            }
            comp.OnInactive();
            _activeComps.Remove(compType);
            return;
        }

        /// <summary>
        /// 通过comp对象设置comp的激活状态.
        /// </summary>
        /// <param name="comp">需要设置的comp对象</param>
        /// <param name="active">是否激活</param>
        public void SetCompActive<T>(T _, bool active) where T : class, IComp {
            int compType = _env!.metaManager.GetCompType<T>();
            _setCompActive(compType, active);
        }

        /// <summary>
        /// 通过IComp类别设置comp的激活状态.
        /// </summary>
        /// <typeparam name="T">IComp的直系派生接口</typeparam>
        /// <param name="active">是否激活</param>
        public void SetCompActive<T>(bool active) where T : class, IComp {
            int compType = _env!.metaManager.GetCompType<T>();
            _setCompActive(compType, active);
        }

        /// <summary>
        /// 通过Comp类型设置comp的激活状态.
        /// </summary>
        /// <param name="compType">要设置的comp类型</param>
        /// <param name="active">是否激活</param>
        public void SetCompActive(int compType, bool active) {
            _setCompActive(compType, active);
        }
    }
}
