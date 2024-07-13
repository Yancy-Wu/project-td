using System.Reflection;

namespace Game.Core.Ec {
    public class MetaManager {
        private readonly Dictionary<Type, int> _compClsToType = new();
        private readonly Dictionary<Type, CompMeta> _compClsToCompMeta = new();
        private readonly Dictionary<string, Type> _compNameToCompCls = new();

        private void _addComp(Type type) {
            BindingFlags flag = BindingFlags.Static;
            FieldInfo compTypeField = type.GetField(nameof(IComp.CompType), flag)!;
            FieldInfo compMetaField = type.GetField(nameof(IComp.Meta), flag)!;
            int compType = (int)compTypeField.GetValue(null)!;
            CompMeta meta = (CompMeta)compMetaField.GetValue(null)!;
            _compClsToType[type] = compType;
            _compClsToCompMeta[type] = meta;
            _compNameToCompCls[type.Name] = type;
        }

        internal int GetCompType<T>() where T : IComp {
            return _compClsToType[typeof(T)];
        }

        internal CompMeta GetCompMeta<T>() where T : IComp {
            return _compClsToCompMeta[typeof(T)];
        }

        internal Type GetCompTypeByName(string name) {
            return _compNameToCompCls[name];
        }

        internal void Initialize(string iCompNamespace) {
            Assembly curAssembly = this.GetType().Assembly;
            Module[] mods = curAssembly.GetModules();
            foreach (Module md in mods) {
                foreach(Type type in md.GetTypes()) {
                    if (type.Namespace != iCompNamespace) continue;
                    if (!type.IsInterface || type.GetInterface(nameof(IComp)) is null) continue;
                    Console.WriteLine("MetaManager Find IComp Interface: {0}", type.Name);
                    _addComp(type);
                }
            }
        }
    }
}
