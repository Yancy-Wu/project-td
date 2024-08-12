using Game.Core.Ec;
using Game.Core.Serializer;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Meta {
    public class TypeMetaManager {
        private static readonly TypeMetaManager _Inst = new();
        public static TypeMetaManager Inst { get { return _Inst; } }
        private readonly Dictionary<Type, TypeMeta> _clsToMeta = new();
        private readonly Dictionary<string, Type> _nameToCls = new();

        private TypeMetaManager() { }

        public TypeMeta GetTypeMeta(Type type) {
            return _clsToMeta[type];
        }

        internal void AddMetaData(Type type) {
            TypeMeta meta = new();
            _clsToMeta.Add(type, meta);
            _nameToCls.Add(type.Name, type);
            _appendSerializeMeta(type, meta);
            _checkAndAppendCompMeta(type, meta);
        }

        public Type GetTypeByName(string name) {
            return _nameToCls[name];
        }

        private void _appendSerializeMeta(Type type, TypeMeta meta) {
            foreach(PropertyInfo property in type.GetProperties()) {
                GamePropertyAttribute? attr = property.GetCustomAttribute<GamePropertyAttribute>();
                if (attr is null) continue;
                IPropertySerializer serializer = SerializeUtils.CreatePropertySerializer(type, property.Name);
                meta.SerializeProperties.Add(serializer);
            }
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

        internal void ScanNamespaceTypes(string iCompNamespace) {
            Assembly curAssembly = this.GetType().Assembly;
            Module[] mods = curAssembly.GetModules();
            foreach (Module md in mods) {
                foreach (Type type in md.GetTypes()) {
                    if (type.Namespace != iCompNamespace) continue;
                    if (!type.IsAssignableTo(typeof(ISerializable))) continue;
                    Console.WriteLine("MetaManager Find Type: {0}", type.Name);
                    AddMetaData(type);
                }
            }
        }
    }
}
