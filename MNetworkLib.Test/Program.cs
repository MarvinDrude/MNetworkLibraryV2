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

            //Logger.AddDefaultConsoleLogging();

            new Thread(() => {

                Random rand = new Random();

                TCPServer server = new TCPServer(27789, null, IPAddress.Any);
                server.Start();

                server.OnMessage += (cl, mess) => {

                    using (IOStream stream = new IOStream(mess.Content)) {

                        float fl;
                        stream.ReadFloat(out fl);

                        Console.WriteLine("Received from Client: " + fl);

                    }

                    using (IOStream stream = new IOStream()) {

                        float numb = (float)rand.NextDouble() * 999;
                        Console.WriteLine("Server sending number: " + numb);

                        stream.WriteFloat(numb);

                        cl.Send(new TCPMessage() {
                            Content = stream.ToArray()
                        });

                    }

                };

            }).Start();

        }

    }

}
