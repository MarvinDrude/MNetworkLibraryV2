using MNetworkLib.Common;
using MNetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MNetworkLib.TestClient {

    class Program {

        static void Main(string[] args) {

            Logger.AddDefaultConsoleLogging();

            new Thread(() => {

                try {

                    TCPClient client = new TCPClient("127.0.0.1", 27789);
                    client.Connect();

                    client.OnHandshake += () => {

                        

                    };

                    client.OnMessage += (mes) => {

                        Console.WriteLine("New Message");

                        using (IOStream stream = new IOStream()) {

                            stream.WriteFloat(20.4f);

                            client.Send(new TCPMessage() {
                                Content = stream.ToArray()
                            });

                        }

                    };

                    client.OnDisconnected += () => {

                        string w = "";

                    };

                } catch(Exception er) {

                    Console.WriteLine(er);

                }

            }).Start();

        }
    }
}
