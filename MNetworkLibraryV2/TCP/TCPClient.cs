using MNetworkLib.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace MNetworkLib.TCP {

    /// <summary>
    /// TCPClient used to conenct to and communicate with tcp server
    /// </summary>
    public class TCPClient : TCPBase {

        /// <summary>
        /// Is logging enabled
        /// </summary>
        public bool Logging { get; set; } = true;

        /// <summary>
        /// Time until next reconnect try in ms
        /// </summary>
        public int ReconnectSleep { get; set; } = 2000;

        /// <summary>
        /// Whether or not a handshake is required
        /// </summary>
        public bool RequireHandshake { get; set; } = true;

        /// <summary>
        /// Enabled raw communcation, only use if you know what you are doing
        /// </summary>
        public bool RawSocket { get; set; } = false;

        /// <summary>
        /// Stream of the socket
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// Writer to the stream
        /// </summary>
        public TCPWriter Writer { get; private set; }

        /// <summary>
        /// Reader to the stream
        /// </summary>
        public TCPReader Reader { get; private set; }

        /// <summary>
        /// RTT of connection
        /// </summary>
        public double RTT { get; private set; }

        /// <summary>
        /// Message Event Handler
        /// </summary>
        /// <param name="message"></param>
        public delegate void MessageEventHandler(TCPMessage message);

        /// <summary>
        /// Message Event, called if the server sent a message
        /// </summary>
        public event MessageEventHandler OnMessage;

        /// <summary>
        /// Connected Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void ConnectedEventHandler();

        /// <summary>
        /// Connected Event, called if the client connected to the server
        /// </summary>
        public event ConnectedEventHandler OnConnected;

        /// <summary>
        /// Handshake Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void HandshakeEventHandler();

        /// <summary>
        /// Handshake Event, called if the client successfully done the handshake
        /// </summary>
        public event HandshakeEventHandler OnHandshake;

        /// <summary>
        /// Disconnected Event Handler
        /// </summary>
        public delegate void DisconnectedEventHandler();

        /// <summary>
        /// Disconnected Event, called if client was disconnected 
        /// </summary>
        public event DisconnectedEventHandler OnDisconnected;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="ssl"></param>
        public TCPClient(string address = "localhost", ushort port = 27789, bool logging = true) {

            Logging = logging;

            if(Logging) {
                Logger.Write("REGION", "TCP Client Constructor");
            }

            IPAddress adr = null;

            if(!IPAddress.TryParse(address, out adr)) {
                throw new Exception("IPAddress not recognizable");
            }

            Address = adr;
            AddressString = address;
            Port = port;

            if (Logging) {
                Logger.Write("INIT", "Setting Port: " + Port);
                Logger.Write("INIT", "Setting SSL: " + SSL);
                Logger.Write("INIT", "Using Address: " + Enum.GetName(typeof(AddressFamily), Address.AddressFamily) + "//" + Address.ToString());
            }

            Running = false;
            InitHandlers();

        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void Send(TCPMessage message) {

            Writer.Write(message);

        }

        /// <summary>
        /// Listen for incomming messages
        /// </summary>
        protected void Listen() {

            using(Stream = GetStream()) {

                Writer = new TCPWriter(Stream);
                Reader = new TCPReader(Stream);

                if (Logging) {
                    Logger.Write("SUCCESS", "Connected to the server");
                }

                OnConnected?.Invoke();

                if (RequireHandshake) {

                    byte[] rand = new byte[10];
                    RandomGen.Random.NextBytes(rand);

                    TCPMessage init = new TCPMessage() {
                        Code = TCPMessageCode.Init,
                        Content = rand
                    };

                    if (Logging) {
                        Logger.Write("INFO", "Sending handshake");
                    }

                    Send(init);

                }

                while (Running) {

                    TCPMessage message = Reader.Read(Socket);

                    if(message == null) {
                        Running = false;
                        OnDisconnected?.Invoke();
                        continue;
                    }

                    if (message.Code == TCPMessageCode.Init) {
                        if (Logging) {
                            Logger.Write("SUCCESS", "Successful handshake");
                        }
                        OnHandshake?.Invoke();
                    } else if (message.Code == TCPMessageCode.Ping) {

                        OnPingMessage(message);

                    } else if (message.Code == TCPMessageCode.Message) {

                        OnMessage?.Invoke(message);

                    }

                }

            }
        }

        protected void OnPingMessage(TCPMessage message) {

            using (IOStream stream = new IOStream(message.Content)) {

                double rtt = 0;
                bool error = stream.ReadDouble(out rtt);

                string dateStr = null;
                error = stream.ReadString(out dateStr);

                RTT = rtt;

                try {

                    if(error) {
                        return;
                    }

                    DateTime sent = DateTime.Parse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                    Send(new TCPMessage() {
                        Code = TCPMessageCode.Pong,
                        Content = Encoding.UTF8.GetBytes(sent.ToString("O"))
                    });

                } catch(Exception er) {
                    Console.WriteLine("Error");
                }

            }

        }

        /// <summary>
        /// Initialize the handlers
        /// </summary>
        protected void InitHandlers() {

            OnMessage += (message) => {
                
            };

        }

        /// <summary>
        /// Get appropiate stream of socket
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected Stream GetStream() {

            Stream stream = new NetworkStream(Socket);

            if (SSL == null) {

                return stream;

            }

            try {

                SslStream sslStream = new SslStream(stream, false);
                sslStream.AuthenticateAsClient(Address.ToString());

                return sslStream;

            } catch (Exception e) {

                return null;

            }

        }

        /// <summary>
        /// Init new Socket instance
        /// </summary>
        protected void InitSocket() {

            Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

        }

        /// <summary>
        /// Disconnectes from Server
        /// </summary>
        public void Disconnect() {

            if(Running) {

                Send(new TCPMessage() {
                    Code = TCPMessageCode.Close,
                    Content = new byte[2] { 0, 1 }
                });

                try {

                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Disconnect(true);

                } catch(Exception er) {

                }

            }

        }

        /// <summary>
        /// Connect to server
        /// </summary>
        public void Connect() {

            if (Logging) {
                Logger.Write("REGION", "Method [Connect]");
            }

            bool connected = false;

            while(!connected) {

                if(Logging) {
                    Logger.Write("INFO", "Trying to connect...");
                }

                InitSocket();

                try {

                    Socket.Connect(new IPEndPoint(Address, Port));
                    
                    Running = connected = true;

                    ListenThread = new Thread(Listen);
                    ListenThread.Start();

                } catch (Exception e) {

                    if (Logging) {
                        Logger.Write("FAILED", "Failed to connect");
                    }

                    Running = connected = false;

                    if (Logging) {
                        Logger.Write("INFO", "Sleep for " + ReconnectSleep + "ms");
                    }

                    Thread.Sleep(ReconnectSleep);

                }

            }

        }

    }

}
