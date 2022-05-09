/* IChannel.cs
 * Abstract representation of a network connection.
 */

namespace Rpc.Rpc
{
    internal interface IChannel {
        /// <summary>
        /// It will be called when channel receive a complete packet msg from peer.
        /// </summary>
        /// <param name="msg">msg content</param>
        public delegate void OnPeerMsg(byte[] msg);

        /// <summary>
        /// Send msg to channel peer.
        /// </summary>
        /// <param name="msg">msg content</param>
        public void Send(byte[] msg);

        /// <summary>
        /// Close this channel.
        /// </summary>
        public void Close();
    }
}
