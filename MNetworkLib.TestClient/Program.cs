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

            //for(int e = 0; e < 300; e++)
            new Thread(() => {

                Random rand = new Random();

                try {

                    TCPClient client = new TCPClient("127.0.0.1", 27789);
                    client.Connect();

                    client.OnHandshake += () => {

                        using (IOStream stream = new IOStream()) {

                            float numb = (float)rand.NextDouble() * 999;
                            Console.WriteLine("Client sending number: " + numb);

                            stream.WriteFloat(numb);

                            client.Send(new TCPMessage() {
                                Content = stream.ToArray()
                            });

                        }

                    };

                    client.OnMessage += (mes) => {

                        using (IOStream stream = new IOStream(mes.Content)) {

                            float fl;
                            stream.ReadFloat(out fl);

                            Console.WriteLine("Received from Server: " + fl);

                        }

                        using (IOStream stream = new IOStream()) {

                            float numb = (float)rand.NextDouble() * 999;
                            Console.WriteLine("Client sending number: " + numb);

                            stream.WriteFloat(numb);

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
