using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Core.PropertyTree {
    public delegate void PropChangeHandler(IPropEventDefine propEvent);

    internal interface IPropTreeNodeManager {
        internal bool IsPassive();
        internal int RequestAllocNodeId();
        internal void RequestSyncNodeId(int nodeId);
        internal void RequestFreeNodeId(int nodeId);
    }

    internal interface IPropTreeNodeContainer {
        internal PropTreeNode PropTreeNode { get; }
    }

    struct PropHandlerData {
        public const char PROP_SEP = ',';
        public PropChangeHandler handler;
        public string[] listenPathParts;
    }

    public class PropTreeNode {
        private readonly List<PropHandlerData> Handlers = new();
        private readonly HashSet<PropTreeNode> Children = new();
        internal int NodeId { get; private set; } = -1;
        internal PropTreeNode? Parent { get; private set; }
        internal IPropTreeNodeManager? Manager { get; private set; }
        internal string? Name { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPropHandler(string listenPath, PropChangeHandler handler) {
            PropHandlerData handlerData = new(){ listenPathParts = listenPath.Split(PropHandlerData.PROP_SEP), handler = handler };
            Handlers.Add(handlerData);
        }

        public void DispatchPropEvent(IPropEventDefine propEvent) {
            // 依次调用本节点的监听者.
            foreach (PropHandlerData handlerData in Handlers) {
                if (PropertyPathMatcher.Match(handlerData.listenPathParts, propEvent)) {
                    handlerData.handler.Invoke(propEvent);
                }
            }

            // 从树中往root冒泡.
            if (Parent == null) return;
            propEvent.PropPathParts.Add(Name!);
            Parent.DispatchPropEvent(propEvent);
        }

        internal void SetManager(IPropTreeNodeManager? manager) {
            // 相同时直接忽略.
            if (ReferenceEquals(Manager, manager)) return;

            // 从旧Mgr清除自己.
            Manager?.RequestFreeNodeId(NodeId);
            Manager = manager;
            NodeId = -1;

            // 拉取，或者向Mgr同步自己的节点ID.
            if(manager != null) {
                if (!manager.IsPassive()) NodeId = manager.RequestAllocNodeId();
                else {
                    Debug.Assert(NodeId != -1, "Sync To Node Mgr Error: Object NodeId is -1.");
                    manager.RequestSyncNodeId(NodeId);
                }
            }

            // 将Mgr递归传递给子节点.
            foreach (var child in Children) child.SetManager(manager);
        }

        internal void SetParent(PropTreeNode? parent) {
            // 相同时直接忽略.
            if (ReferenceEquals(parent, Parent)) return;

            // Parent设置为None，直接走Detach.
            if (parent == null) {
                Detach();
                return;
            }

            // 已有parent的直接trace.
            Debug.Assert(Parent == null, "Cannot attach exist tree node, must detach first...");

            // 纳入父节点管理.
            Parent = parent;
            parent.Children.Add(this);
            SetManager(parent.Manager);
        }
        
        internal void SetParentAndName(PropTreeNode? parent, string name) {
            SetParent(parent);
            Name = name;
        }

        internal void Detach() {
            // 从节点树中拆除.
            Parent?.Children.Remove(this);
            Manager?.RequestFreeNodeId(NodeId);
            Parent = null;
            Manager = null;
            NodeId = -1;
        }
    }
}
