using Game.Core.Ec;
using Game.Core.Serializer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Meta {
    public class TypeMetaManager {
        private readonly Dictionary<Type, TypeMeta> _compClsToMeta = new();
        private readonly Dictionary<string, Type> _compNameToCompCls = new();

        public TypeMeta GetTypeMeta(Type type) {
            return _compClsToMeta[type];
        }

        public void AddMetaData(Type type) {
            Debug.Assert(type.IsSubclassOf(typeof(ISerializable)), $"Can only add {nameof(ISerializable)}, not {type.Name}");
            TypeMeta meta = new();
            _compClsToMeta.Add(type, meta);
            _appendSerializeMeta(type, meta);
            _checkAndAppendCompMeta(type, meta);
        }

        public Type GetTypeByName(string name) {
            return _compNameToCompCls[name];
        }

        private void _appendSerializeMeta(Type type, TypeMeta meta) {
        }

        private void _checkAndAppendCompMeta(Type type, TypeMeta meta) {
            if (!type.IsInterface || type.GetInterface(nameof(IComp)) is null) return;
            BindingFlags flag = BindingFlags.Static;
            FieldInfo compTypeField = type.GetField(nameof(IComp.CompType), flag)!;
            FieldInfo compMetaField = type.GetField(nameof(IComp.Meta), flag)!;
            int compType = (int)compTypeField.GetValue(null)!;
            CompMeta compMeta = (CompMeta)compMetaField.GetValue(null)!;
            meta.CompType = compType;
            meta.CompMeta = compMeta;
        }
    }
}
