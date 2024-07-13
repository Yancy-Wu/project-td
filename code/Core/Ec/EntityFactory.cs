namespace Game.Core.Ec {
    /// <summary>
    /// Entity的创建工厂，创建方法都在这里汇总.
    /// </summary>
    public static class EntityFactory {

        /// <summary>
        /// 给定初始化的dict信息以及需要创建的comp名称
        /// </summary>
        /// <param name="metaManager"></param>
        /// <param name="presetCompData"></param>
        /// <param name="runtimeCompData"></param>
        /// <returns></returns>
        public static Entity CreateEntity(MetaManager metaManager, Dictionary<string, object> presetCompData, Dictionary<string, object> runtimeCompData) {
            Entity entity = new();
            foreach (var item in presetCompData) {
                string compName = item.Key;
                object rawData = item.Value;
                object? runtimeData = runtimeCompData.ContainsKey(compName) ? runtimeCompData[compName] : null;
                Type type = metaManager.GetCompTypeByName(compName);
                IComp comp = (IComp)Activator.CreateInstance(type)!;
                // 序列化数据，优先使用runtime数据，不存在的则使用preset数据.
                entity.AddComp(comp);
            }
            return entity;
        }
    }
}
