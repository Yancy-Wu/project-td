using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpc.Rpc {
    internal interface IChannel {
        public void Read();
        public void Send();
        public void Close();
    }
}
