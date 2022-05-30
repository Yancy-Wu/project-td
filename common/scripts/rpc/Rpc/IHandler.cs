/* IHandler.cs
 * rpc handler is responsible for net status change and rpc msg.
 */

namespace Rpc.Rpc
{
    internal interface IHandler {
        public void OnConnected();

        public void OnDisconnected();

        public void OnRpc(string callee, params object[] args);
    }
}
