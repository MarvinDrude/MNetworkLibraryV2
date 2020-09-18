using MNetworkLib.Common;
using MNetworkLib.TCP;
using MNetworkLibraryV2.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MNetworkLib.Test {

    class Program {

        static void Main(string[] args) {

            Logger.AddDefaultConsoleLogging();

            new Thread(() => {

                TCPServer server = new TCPServer(27789, null, IPAddress.Any);
                server.Start();

                server.OnMessage += (cl, mess) => {

                    using (IOStream stream = new IOStream(mess.Content)) {

                        float fl;
                        stream.ReadFloat(out fl);

                        Console.WriteLine(fl);

                    }

                };

            }).Start();

        }

    }

}
