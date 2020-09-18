using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Representing a client of a server
    /// </summary>
    public class TCPServerClient {

        /// <summary>
        /// Underlying socket to handle communication
        /// </summary>
        public Socket Socket { get; protected set; }

        /// <summary>
        /// Stream to write to and read from, could be null
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// UID to uniquely identify a client
        /// </summary>
        public string UID { get; protected set; }

        /// <summary>
        /// Thread to listen to client messages
        /// </summary>
        public Thread Thread { get; set; }

        /// <summary>
        /// Writer for stream operations
        /// </summary>
        public TCPWriter Writer { get; set; }

        /// <summary>
        /// Reader for stream operations
        /// </summary>
        public TCPReader Reader { get; set; }

        /// <summary>
        /// Whether or not the client did the handshake
        /// </summary>
        public bool DoneHandshake { get; set; } = false;

        /// <summary>
        /// Time the client connected before handshake
        /// </summary>
        public DateTime Joined { get; set; }

        /// <summary>
        /// Round Trip Time
        /// </summary>
        public double RTT { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="uid"></param>
        public TCPServerClient(Socket socket, string uid) {

            Socket = socket;
            UID = uid;

        }

        /// <summary>
        /// Sends message to the client
        /// </summary>
        /// <param name="message"></param>
        public void Send(TCPMessage message) {

            Writer.Write(message);

        }

    }

}
