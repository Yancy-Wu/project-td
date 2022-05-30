using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpc.Rpc {
    internal class Rpc {
        private readonly IHandler handler;
        private readonly IChannel channel;
        private readonly ISerializer serializer;

        public Rpc(IChannel _channel, ISerializer _serializer, IHandler _handler) {
            channel = _channel;
            serializer = _serializer;
            handler = _handler;
            channel.PeerMsg += OnRpc;
        }

        public void Call(string callee, params object[] args) {
            args = (object[])args.Append(callee);
            byte[] data = serializer.Serialize(args);
            channel.Send(data);
        }

        private void OnRpc(byte[] data) {
            object[] args = serializer.Deserialize(data);
            string callee = (string)args[^1];
            args = args[0..^1];
            handler.OnRpc(callee, args);
        }

        public void Close() {
            channel.Close();
        }
    }
}
