using Game.Core.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Meta {
    public struct CompMeta {
        public int[] DependCompTypes;

        public CompMeta(int[] dependCompTypes) {
            DependCompTypes = dependCompTypes;
        }
    }

    public class TypeMeta {
        public List<IPropertySerializer> SerializeProperties { get; } = new();
        public CompMeta? CompMeta { get; set; }
        public int CompType = -1;
    }
}
