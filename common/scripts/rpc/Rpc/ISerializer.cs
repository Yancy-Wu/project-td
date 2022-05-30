using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpc.Rpc
{
    internal interface ISerializer {
        public byte[] Serialize(params object[] args);
        public object[] Deserialize(byte[] data);
    }
}
