using System.Runtime.CompilerServices;

namespace Game.Core.PropertyTree {
    public delegate void PropChangeHandler(IPropEventDefine propEvent);

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
        internal int NodeId { get; set; } = -1;
        internal IPropTreeNodeContainer? Parent { get; set; }
        internal string? Name { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPropHandler(string listenPath, PropChangeHandler handler) {
            PropHandlerData handlerData = new(){ listenPathParts = listenPath.Split(PropHandlerData.PROP_SEP), handler = handler };
            Handlers.Add(handlerData);
        }

        public void DispatchPropEvent(IPropEventDefine propEvent) {
            foreach (PropHandlerData handlerData in Handlers) {
                if (PropertyPathMatcher.Match(handlerData.listenPathParts, propEvent)) {
                    handlerData.handler.Invoke(propEvent);
                }
            }
            if (Parent == null) return;
            propEvent.PropPathParts.Add(Name!);
            Parent.PropTreeNode.DispatchPropEvent(propEvent);
        }
    }
}
