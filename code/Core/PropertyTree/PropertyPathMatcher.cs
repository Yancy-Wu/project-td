namespace Game.Core.PropertyTree {
    internal static class PropertyPathMatcher {
        public static unsafe bool Match(string[] listenPathParts, IPropEventDefine propEvent) {
            // 一些规则：通配符*只能匹配一个字段，propPath只要是listenPath的前缀即满足匹配，匹配后通配符的值将写入propEvent
            // 另外propEvent.PropPathParts是倒序的，因为从叶节点向root开始递归. listenPathParts则是正序的.
            var propPathParts = propEvent.PropPathParts;
            bool ret = true;
            int propPathMaxIndex = propPathParts.Count - 1;
            if (propPathMaxIndex <= listenPathParts.Length) return false;

            // 检查每一个字段.
            for (int i = 0; i < listenPathParts.Length; i++) {
                string curListenPart = listenPathParts[i];
                string curPropPart = propPathParts[propPathMaxIndex - i];

                // 字段相同匹配成功.
                if (curListenPart == curPropPart) continue;

                // 通配符匹配成功并记录index.
                if (curListenPart[0] == PropHandlerData.PROP_SEP) {
                    propEvent.MatchedIndex = curPropPart;
                    continue;
                }
                ret = false;
                break;
            }
            return ret;
        }
    }
}
