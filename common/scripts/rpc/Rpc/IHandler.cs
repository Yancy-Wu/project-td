using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpc.Rpc {
    internal interface IHandler {
        public void OnConnected();

        public void OnDisconnected();

        public void OnError(RpcError error);
    }
}
