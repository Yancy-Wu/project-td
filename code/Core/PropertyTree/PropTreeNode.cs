using System.Runtime.CompilerServices;

namespace Game.Core.PropertyTree {
    delegate void PropChangeHandler(IPropEventDefine propEvent);

    internal interface IPropTreeNode {
        public const char PROP_SEP = ',';
        internal void DispatchPropEvent(IPropEventDefine propEvent);
    }

    struct PropHandlerData {
        public PropChangeHandler handler;
        public string[] listenPathParts;
    }

    internal class PropTreeNode: IPropTreeNode {
        private readonly List<PropHandlerData> Handlers = new();
        internal int NodeId { get; set; } = -1;
        internal IPropTreeNode? Parent { get; set; }
        internal string? Name { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPropHandler(string listenPath, PropChangeHandler handler) {
            PropHandlerData handlerData = new(){ listenPathParts = listenPath.Split(IPropTreeNode.PROP_SEP), handler = handler };
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
            Parent.DispatchPropEvent(propEvent);
        }
    }
}
