using Game.Core.Meta;
using Game.Core.Objects;
using Game.Core.PropertyTree;

namespace Game.Core.Ec {

    public interface IComp {
        // 下面这两个一定需要复写.
        static readonly int CompType;
        static readonly CompMeta Meta;
        public Entity? Ent { get; internal set; }

        internal RuntimeCompInfo CompInfo { get; }
        void OnActive();
        void OnInactive(bool willDestroy = false);
        void Destroy();
    }

    public class RuntimeCompInfo {
        public bool IsActive = false;
    }

    public class ComponentBase : PObject, IPropTreeNodeManager {
        bool IPropTreeNodeManager.IsPassive() {
            throw new NotImplementedException();
        }

        int IPropTreeNodeManager.RequestAllocNodeId(IPropTreeNodeContainer node) {
            throw new NotImplementedException();
        }

        void IPropTreeNodeManager.RequestFreeNodeId(int nodeId) {
            throw new NotImplementedException();
        }

        void IPropTreeNodeManager.RequestSyncNodeId(int nodeId, IPropTreeNodeContainer node) {
            throw new NotImplementedException();
        }
    }
}
