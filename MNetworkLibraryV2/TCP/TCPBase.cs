using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace MNetworkLib.TCP {

    /// <summary>
    /// TCP Base which is used by client and server
    /// </summary>
    public abstract class TCPBase {

        private class PropertySearch {

            public int Index;

            public PropertyInfo Info;

        }

        public static Dictionary<Type, List<PropertyInfo>> Ordering { get; set; } = new Dictionary<Type, List<PropertyInfo>>();

        public static void AddMessageObject(Type type) {

            if(Ordering.ContainsKey(type)) {
                return;
            }

            List<PropertySearch> infos = new List<PropertySearch>();

            foreach(PropertyInfo prop in type.GetProperties()) {

                if(Attribute.IsDefined(prop, typeof(TCPSerializable))) {

                    TCPSerializable attr = prop.GetCustomAttribute<TCPSerializable>();

                    infos.Add(new PropertySearch() {
                        Index = attr.Index,
                        Info = prop
                    });

                }

            }

            var res = infos.OrderBy(o => o.Index).ToList();
            List<PropertyInfo> ret = new List<PropertyInfo>();

            foreach (PropertySearch search in res) {

                ret.Add(search.Info);

            }

            Ordering[type] = ret;

        }

        /// <summary>
        /// Socket handling underlying communication
        /// </summary>
        public Socket Socket { get; protected set; }

        /// <summary>
        /// Whether its currently running or not
        /// </summary>
        public bool Running { get; protected set; }

        /// <summary>
        /// IP Address to use
        /// </summary>
        public IPAddress Address { get; protected set; }

        /// <summary>
        /// Input IP Adress as string
        /// </summary>
        public string AddressString { get; protected set; }

        /// <summary>
        /// Port to use
        /// </summary>
        public ushort Port { get; protected set; }

        /// <summary>
        /// Thread to listen
        /// </summary>
        public Thread ListenThread { get; protected set; }
        
        /// <summary>
        /// Certificate to use for SSL
        /// </summary>
        public X509Certificate2 SSL { get; protected set; }

        /// <summary>
        /// Certificate protocol
        /// </summary>
        public SslProtocols SSLProtocol { get; set; } = SslProtocols.Tls12;

    }

}
