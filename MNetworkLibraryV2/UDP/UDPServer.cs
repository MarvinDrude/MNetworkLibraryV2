using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MNetworkLib.UDP {

    /// <summary>
    /// Server which handles clients authed by keys
    /// </summary>
    public class UDPServer {

        /// <summary>
        /// Socket for underlying communication
        /// </summary>
        protected UdpClient Socket { get; set; }
        
        /// <summary>
        /// Thread to listen for messages
        /// </summary>
        protected Thread ListenThread { get; set; }

        /// <summary>
        /// Default contructor
        /// </summary>
        public UDPServer(ushort port, IPAddress address = null) {
            
            if (address == null) {
                address = IPAddress.Loopback;
            }

            Socket = new UdpClient(port);

        }

        /// <summary>
        /// Listen
        /// </summary>
        public void Listen() {
            
            //byte[] all = Socket.Receive(); // Kein slicing geil!
        }

    }

}
