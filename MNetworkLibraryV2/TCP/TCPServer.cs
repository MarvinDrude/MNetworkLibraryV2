using MNetworkLib.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Multithreaded TCP Server
    /// </summary>
    public class TCPServer : TCPBase {

        /// <summary>
        /// Max Package length, default 100MB
        /// </summary>
        public static int MaxPackageLength { get; set; } = 107374182;

        /// <summary>
        /// List of all clients currently connected to the server
        /// </summary>
        public List<TCPServerClient> ClientsList { get; protected set; } = new List<TCPServerClient>();

        /// <summary>
        /// Backlog to use, only change before start server
        /// </summary>
        public int Backlog { get; set; } = 500;

        /// <summary>
        /// Size of the uid of a client
        /// </summary>
        public uint UIDLength { get; set; } = 12;

        /// <summary>
        /// Whether clients need to make a initial handshake
        /// </summary>
        public bool RequireHandshake { get; set; } = true;

        /// <summary>
        /// Is logging enabled
        /// </summary>
        public bool Logging { get; set; } = true;

        /// <summary>
        /// Whether pinging is enabled
        /// </summary>
        public bool Pinging { get; set; } = true;

        /// <summary>
        /// Thread to handle management such as kick clients with no handshake
        /// </summary>
        public Thread ManagementThread { get; private set; }

        /// <summary>
        /// Thread to handle pinging and rtt
        /// </summary>
        public Thread PingThread { get; private set; }

        /// <summary>
        /// Management sleep time in ms
        /// </summary>
        public int ManagementSleep { get; set; } = 20000;

        /// <summary>
        /// Ping sleep time in ms
        /// </summary>
        public int PingSleep { get; set; } = 5000;

        /// <summary>
        /// Time span until clients are kicked without handshake
        /// </summary>
        public TimeSpan HandshakeTimeout { get; set; } = new TimeSpan(0, 0, 40);

        /// <summary>
        /// Dictionary containing all clients identified by their uid
        /// </summary>
        public Dictionary<string, TCPServerClient> ClientsDict { get; protected set; } = new Dictionary<string, TCPServerClient>();

        /// <summary>
        /// Message Event Handler
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public delegate void MessageEventHandler(TCPServerClient client, TCPMessage message);

        /// <summary>
        /// Message Event, called if a client sent a message
        /// </summary>
        public event MessageEventHandler OnMessage;

        /// <summary>
        /// Connected Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void ConnectedEventHandler(TCPServerClient client);

        /// <summary>
        /// Connected Event, called if a new client is successfully connected
        /// </summary>
        public event ConnectedEventHandler OnConnected;

        /// <summary>
        /// Disconnected Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void DisconnectedEventHandler(TCPServerClient client);

        /// <summary>
        /// Disconnected Event, called if a client is disconnected
        /// </summary>
        public event DisconnectedEventHandler OnDisconnected;

        /// <summary>
        /// No Handshaked Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void NoHandshakeEventHandler(TCPServerClient client);

        /// <summary>
        /// No Handshake Event, called if client fails to provide correct init package
        /// </summary>
        public event NoHandshakeEventHandler OnNoHandshake;

        /// <summary>
        /// Timeout Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void TimeoutEventHandler(TCPServerClient client);

        /// <summary>
        /// Timeout Event, called if client is timed out
        /// </summary>
        public event TimeoutEventHandler OnTimeout;

        /// <summary>
        /// Kick Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void KickEventHandler(TCPServerClient client);

        /// <summary>
        /// Kick Event, called if client was kicked
        /// </summary>
        public event KickEventHandler OnKick;

        /// <summary>
        /// Handshake Event Handler
        /// </summary>
        /// <param name="client"></param>
        public delegate void HandshakeHandler(TCPServerClient client);

        /// <summary>
        /// Handshake Event
        /// </summary>
        public event HandshakeHandler OnHandshake;

        /// <summary>
        /// Default constructor, default uses ipv4 address
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ssl"></param>
        public TCPServer(ushort port = 27789, X509Certificate2 ssl = null, IPAddress address = null) {

            if (Logging)
                Logger.Write("REGION", "TCP Server Constructor");

            SSL = ssl;
            Port = port;

            if (Logging) {
                Logger.Write("INIT", "Setting Port: " + Port);
                Logger.Write("INIT", "Setting SSL: " + SSL);
            }

            if (address == null) {

                var host = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress adr in host.AddressList) {
                    if (adr.AddressFamily == AddressFamily.InterNetwork) {
                        Address = adr;
                        break;
                    }
                }

                if (Address == null) {
                    Address = host.AddressList[0];
                }

            } else {
                Address = address;
            }

            if (Logging) {
                Logger.Write("INIT", "Using Address: " + Enum.GetName(typeof(AddressFamily), Address.AddressFamily) + "//" + Address.ToString());
            }

            Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            Socket.Bind(new IPEndPoint(Address, Port));

            Running = false;

        }
        
        /// <summary>
        /// Kicks user
        /// </summary>
        /// <param name="client"></param>
        public void Kick(TCPServerClient client) {

            RemoveClient(client, TCPDisconnectType.Kick);

        }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <returns></returns>
        public bool Start() {

            if (Logging) {
                Logger.Write("REGION", "Method [Start]");
            }

            if ((ListenThread == null || !ListenThread.IsAlive) && !Running) {

                if (Logging) {
                    Logger.Write("SUCCESS", "Starting server");
                }

                ListenThread = new Thread(() => Listen());
                ManagementThread = new Thread(Management);
                PingThread = new Thread(Ping);

                Running = true;

                ListenThread.Start();
                ManagementThread.Start();
                PingThread.Start();

                return true;

            }

            if (Logging) {
                Logger.Write("FAILED", "Starting server");
            }

            return false;

        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop() {

            if (Logging) {
                Logger.Write("REGION", "Method [Stop]");
            }

            if (Logging) {
                Logger.Write("INFO", "Stopping server");
            }

            Running = false;
            
            lock (ClientsList)
            lock(ClientsDict) {

                for(int e = ClientsList.Count - 1; e >= 0; e--) {

                    TCPServerClient client = ClientsList[e];

                    RemoveClient(client, TCPDisconnectType.Disconnect, e);

                }

            }

            try {

                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();

            } catch(Exception er) {

            }

            ManagementThread.Join();
            ListenThread.Join();

            ListenThread = new Thread(() => Listen());
            ManagementThread = new Thread(Management);

            Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            Socket.Bind(new IPEndPoint(Address, Port));
            
            if (Logging) {
                Logger.Write("INFO", "Stopped server");
            }

        }

        /// <summary>
        /// Removes a client from the server
        /// </summary>
        /// <param name="client"></param>
        /// <param name="type"></param>
        public void RemoveClient(TCPServerClient client, TCPDisconnectType type = TCPDisconnectType.Disconnect, int pos = -1) {

            if (Logging) {
                Logger.Write("REGION", "Method [RemoveClient]");
            }

            if (type == TCPDisconnectType.NoHandshake) {

                if (Logging) {
                    Logger.Write("INFO", "Client no handshake: " + client.UID);
                }
                OnNoHandshake?.Invoke(client);

            } else if (type == TCPDisconnectType.Disconnect) {

                if (Logging) {
                    Logger.Write("INFO", "Client disconnect: " + client.UID);
                }
                OnDisconnected?.Invoke(client);

            } else if (type == TCPDisconnectType.Timeout) {

                if (Logging) {
                    Logger.Write("INFO", "Client timeout: " + client.UID);
                }
                OnTimeout?.Invoke(client);

            } else if (type == TCPDisconnectType.Kick) {

                if (Logging) {
                    Logger.Write("INFO", "Client kick: " + client.UID);
                }
                OnKick?.Invoke(client);

            }

            lock (ClientsDict) ClientsDict.Remove(client.UID);
            lock (ClientsList) {

                if(pos >= 0) {

                    ClientsList.RemoveAt(pos);

                } else {

                    for (int e = ClientsList.Count - 1; e >= 0; e--) {

                        if (ClientsList[e].UID == client.UID) {
                            if (Logging) {
                                Logger.Write("INFO", "Client found in ClientsList: " + client.UID);
                            }
                            ClientsList.RemoveAt(e);
                            break;
                        }

                    }

                }

            }

            try {

                client.Socket.Shutdown(SocketShutdown.Both);
                client.Socket.Close();

            } catch (Exception e) {

                if (Logging) {
                    Logger.Write("FAILED", "Socket shutdown/close", e);
                }

            }

        }

        protected void Ping() {

            while(Running && Pinging) {

                Thread.Sleep(PingSleep);

                lock(ClientsList) {
                    lock(ClientsDict) {

                         for (int e = ClientsList.Count - 1; e >= 0; e--) {

                            TCPServerClient client = ClientsList[e];

                            try {

                                using (IOStream stream = new IOStream()) {

                                    stream.WriteDouble(client.RTT);
                                    stream.WriteString(DateTime.UtcNow.ToString("O"));

                                    byte[] arr = stream.ToArray();

                                    client.Send(new TCPMessage() {
                                        Code = TCPMessageCode.Ping,
                                        Content = arr
                                    });

                                }

                            } catch (Exception er) {

                                RemoveClient(client, TCPDisconnectType.Timeout);

                            }

                        }

                    }
                }

            }

        }

        /// <summary>
        /// Management to handle automated kicks etc
        /// </summary>
        protected void Management() {

            while (Running) {

                Thread.Sleep(ManagementSleep);

                lock (ClientsList) {
                    lock (ClientsDict) {

                        for (int e = ClientsList.Count - 1; e >= 0; e--) {

                            TCPServerClient c = ClientsList[e];

                            if ((DateTime.Now - c.Joined) > HandshakeTimeout
                                && RequireHandshake && !c.DoneHandshake) {

                                RemoveClient(c, TCPDisconnectType.NoHandshake);

                            }

                        }
                    }
                }

            }

        }

        /// <summary>
        /// Listen for new connections
        /// </summary>
        protected void Listen() {

            if (Logging) {
                Logger.Write("REGION", "Method [Listen]");
            }

            Socket.Listen(Backlog);

            if (Logging) {
                Logger.Write("INFO", "Start listening for clients");
            }

            while (Running) {

                Socket socket = Socket.Accept();

                if (Logging) {
                    Logger.Write("INFO", "New socket connected");
                }

                TCPServerClient client = new TCPServerClient(
                    socket, RandomGen.GenRandomUID(ClientsDict, UIDLength));

                client.Joined = DateTime.Now;

                Thread clientThread = new Thread(() => ListenClient(client));
                client.Thread = clientThread;

                clientThread.Start();

                if (Logging) {
                    Logger.Write("INFO", "Created client and started thread");
                }

            }

        }

        /// <summary>
        /// Listen for new messages of individual clients
        /// </summary>
        /// <param name="client"></param>
        protected void ListenClient(TCPServerClient client) {

            if (Logging) {
                Logger.Write("REGION", "Method [ListenClient]");
            }

            using (Stream ns = GetStream(client)) {

                client.Stream = ns;

                client.Writer = new TCPWriter(ns);
                client.Reader = new TCPReader(ns);

                if (Logging) {
                    Logger.Write("INFO", "Created stream, writer and reader for client: " + client.UID);
                }

                lock (ClientsList) ClientsList.Add(client);
                lock(ClientsDict) ClientsDict.Add(client.UID, client);
                OnConnected?.Invoke(client);

                if (RequireHandshake) {

                    TCPMessage message = client.Reader.Read(client);

                    if (message.Code != TCPMessageCode.Init
                        || message.Content.Length > 10) {
                        RemoveClient(client, TCPDisconnectType.NoHandshake);
                        return;
                    }

                    if (Logging) {
                        Logger.Write("SUCCESS", "Handshake: " + client.UID);
                    }

                    client.DoneHandshake = true;

                    client.Send(new TCPMessage() {
                        Code = TCPMessageCode.Init,
                        Content = new byte[] { 0, 1, 0 }
                    });

                    OnHandshake?.Invoke(client);

                }

                while (Running && ClientsDict.ContainsKey(client.UID)) {

                    TCPMessage message = client.Reader.Read(client);

                    if(message == null) {
                        RemoveClient(client, TCPDisconnectType.Timeout);
                        return;
                    }

                    if (Logging) {
                        Logger.Write("INFO", "New message " + Enum.GetName(typeof(TCPMessageCode), message.Code) + " from user: " + client.UID);
                    }

                    if (message.Code == TCPMessageCode.Close) {
                        RemoveClient(client, TCPDisconnectType.Disconnect);
                    }

                    if(message.Code == TCPMessageCode.Pong) {
                        HandlePong(message);
                        continue;
                    }

                    if(message.Code == TCPMessageCode.Message)
                        OnMessage?.Invoke(client, message);

                }

            }

        }

        /// <summary>
        /// Handle pong and rtt
        /// </summary>
        /// <param name="message"></param>
        protected void HandlePong(TCPMessage message) {

            try {

                string strDate = Encoding.UTF8.GetString(message.Content);
                DateTime time = DateTime.Parse(strDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                message.Client.RTT = (DateTime.UtcNow.Ticks - time.Ticks) / 10000;

            } catch(Exception er) {

                if (Logging) {
                    Logger.Write("FAILED", "Socket RTT failed", er);
                }

            }

        }

        /// <summary>
        /// Get appropiate stream of socket
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected Stream GetStream(TCPServerClient client) {

            Stream stream = new NetworkStream(client.Socket);

            if (SSL == null) {

                return stream;

            }

            try {

                SslStream sslStream = new SslStream(stream, false);
                var task = sslStream.AuthenticateAsServerAsync(SSL, false, SSLProtocol, true);
                task.Start();
                task.Wait();

                return sslStream;

            } catch (Exception e) {

                return null;

            }

        }

    }

}
