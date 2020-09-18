using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Helper to easily read from stream
    /// </summary>
    public class TCPReader {

        /// <summary>
        /// Stream to read from
        /// </summary>
        public Stream Stream { get; protected set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="stream"></param>
        public TCPReader(Stream stream) {

            Stream = stream;

        }

        /// <summary>
        /// Read tcp message off the stream
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public TCPMessage Read(Socket socket) {

            byte first;

            try {

                first = (byte)Stream.ReadByte();

            } catch (Exception e) {

                return null;

            }

            if (!IsClientConnected(socket)) {
                return null;
            }

            byte second = (byte)Stream.ReadByte();
            uint len = ReadLength(second);

            if (len != 0) {

                // solve encoded bytes xor

                byte[] key = TCPReaderWriter.Read(Stream, 4);
                byte[] encoded = TCPReaderWriter.Read(Stream, len);
                byte[] decoded = new byte[len];

                for (int e = 0; e < encoded.Length; e++) {

                    decoded[e] = (byte)(encoded[e] ^ key[e % 4]);

                }

                return new TCPMessage() {
                    Client = null,
                    Code = (TCPMessageCode)first,
                    Content = decoded,
                    Sent = DateTime.Now
                };

            }

            return null;

        }

        /// <summary>
        /// Read tcp message off the stream
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public TCPMessage Read(TCPServerClient client) {

            byte first;

            try {

                first = (byte)Stream.ReadByte();
                
            } catch(Exception e) {

                return null;

            }

            if(!IsClientConnected(client)) {
                return null;
            }

            byte second = (byte)Stream.ReadByte();
            uint len = ReadLength(second);

            if(len != 0) {
                
                // solve encoded bytes xor

                byte[] key = TCPReaderWriter.Read(Stream, 4);
                byte[] encoded = TCPReaderWriter.Read(Stream, len);
                byte[] decoded = new byte[len];

                for (int e = 0; e < encoded.Length; e++) {

                    decoded[e] = (byte)(encoded[e] ^ key[e % 4]);

                }

                return new TCPMessage() {
                    Client = client,
                    Code = (TCPMessageCode)first,
                    Content = decoded,
                    Sent = DateTime.Now
                };

            }

            return null;

        }

        /// <summary>
        /// Read length off the stream
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public uint ReadLength(byte second) {

            uint len = 0;

            if(second == 126) {

                bool error;
                len = TCPReaderWriter.ReadUShort(Stream, false, out error);

            } else if(second == 127) {

                bool error;
                len = (uint)TCPReaderWriter.ReadULong(Stream, false, out error);
                
                if(len < 0 || len > TCPServer.MaxPackageLength) {

                    return 0;

                }

            } else if(second > 0 && second < 126) {

                len = second;

            }

            return len;

        }

        /// <summary>
        /// Check if client is still connected
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool IsClientConnected(TCPServerClient client) {

            return IsClientConnected(client.Socket);

        }

        /// <summary>
        /// Check if client is still connected
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool IsClientConnected(Socket socket) {

            try {

                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

            } catch (Exception e) {

                return false;

            }

        }

    }

}
