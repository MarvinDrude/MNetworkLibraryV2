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
    /// UDP Client 
    /// </summary>
    public class UDPClient {

        /// <summary>
        /// Whether client already authed to server
        /// </summary>
        public bool Authed { get; set; }

        /// <summary>
        /// If running
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Key to auth to server
        /// </summary>
        public string AuthKey { get; set; }

        /// <summary>
        /// Determines if last ping arrived
        /// </summary>
        protected bool Pinged { get; set; }

        /// <summary>
        /// Socket for underlying communication
        /// </summary>
        protected UdpClient Socket { get; set; }

        /// <summary>
        /// EndPoint of server
        /// </summary>
        protected IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// Thread to auth and ping
        /// </summary>
        protected Thread ManagementThread { get; set; }

        /// <summary>
        /// Message Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void MessageEventHandler(UDPPacket packet);

        /// <summary>
        /// Message Event, called on new message from server
        /// </summary>
        public MessageEventHandler OnMessage;

        /// <summary>
        /// Authed Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void AuthedEventHandler();

        /// <summary>
        /// Authed Event, called when client authed to server
        /// </summary>
        public AuthedEventHandler OnAuthed;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public UDPClient(string address, ushort port, string authKey) {

            EndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Authed = false;
            Running = false;
            Pinged = false;

        }

        /// <summary>
        /// Connects to server with automatic port assignment
        /// </summary>
        public void Connect() {

            if (Authed || (ManagementThread != null && ManagementThread.IsAlive))

            Running = true;

            Socket = new UdpClient();

            Socket.Connect(EndPoint);
            Socket.BeginReceive(ReceiveCallback, null);

            ManagementThread = new Thread(Auth);
            ManagementThread.Start();

        }

        /// <summary>
        /// Thread to auth this client
        /// </summary>
        private void Auth() {

            while(!Authed) {

                Send(new UDPPacket() {
                    Data = Encoding.UTF8.GetBytes(AuthKey),
                    Type = UDPPacketType.Auth
                });

                Thread.Sleep(2000);

            }



        }

        /// <summary>
        /// Send message to server
        /// </summary>
        /// <param name="message"></param>
        public void Send(UDPPacket message) {

            if(Socket == null) {
                return;
            }

            try {

                byte[] data = message.GetRawContent();

                Socket.BeginSend(data, data.Length, null, null);

            } catch(Exception er) {
                
            }

        }

        /// <summary>
        /// Receive Callback
        /// </summary>
        /// <param name="res"></param>
        private void ReceiveCallback(IAsyncResult res) {

            try {

                IPEndPoint endPoint = EndPoint;

                byte[] buffer = Socket.EndReceive(res, ref endPoint);
                Socket.BeginReceive(ReceiveCallback, null);

                if(buffer.Length < 2) {

                    return;

                }

                Handle(buffer);

            } catch(Exception er) {

            }

        }

        /// <summary>
        /// Handle received data
        /// </summary>
        /// <param name="data"></param>
        private void Handle(byte[] data) {

            UDPPacket packet = UDPPacket.GetPacket(data);
            
            if(packet != null) {

                switch(packet.Type) {

                    case UDPPacketType.Message:

                        OnMessage?.Invoke(packet);
                        break;

                    case UDPPacketType.Auth:

                        Authed = true;
                        OnAuthed?.Invoke();
                        break;

                    case UDPPacketType.Ping:

                        Send(new UDPPacket(
                            new byte[] { 5 },
                            UDPPacketType.Pong));
                        break;

                    default:
                        return;

                }

            }

        }

    }

}
