using System.Reflection;

namespace Game.Core.Ec
{
    class MetaManager
    {
        private readonly Dictionary<Type, int> _compClsToType = new();
        private readonly Dictionary<Type, CompMeta> _compClsToCompMeta = new();

        public void AddComp<T>() where T: IComp
        {
            Type type = typeof(T);
            BindingFlags flag = BindingFlags.Static;
            FieldInfo compTypeField = type.GetField("CompType", flag)!;
            FieldInfo compMetaField = type.GetField("Meta", flag)!;
            int compType = (int)compTypeField.GetValue(null)!;
            CompMeta meta = (CompMeta)compMetaField.GetValue(null)!;
            _compClsToType[type] = compType;
            _compClsToCompMeta[type] = meta;
        }

        public int GetCompType<T>() where T : IComp
        {
            return _compClsToType[typeof(T)];
        }

        public CompMeta GetCompMeta<T>() where T : IComp
        {
            return _compClsToCompMeta[typeof(T)];
        }
    }
}
