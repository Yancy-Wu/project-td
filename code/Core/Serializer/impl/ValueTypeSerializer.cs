using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Serializer.impl {
    internal class ValueTypeSerializer {
        public static unsafe void Serialize(SerializeContext ctx, MemoryStream stream, ValueType obj) {
        }
        public static unsafe ValueType Deserialize(SerializeContext ctx, MemoryStream stream) {
        }
    }
}
