using MNetworkLib.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Helper to easily write to stream
    /// </summary>
    public class TCPWriter {

        /// <summary>
        /// Stream to write to
        /// </summary>
        public Stream Stream { get; protected set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="stream"></param>
        public TCPWriter(Stream stream) {

            Stream = stream;

        }

        /// <summary>
        /// Write message to stream
        /// </summary>
        /// <param name="message"></param>
        public void Write(TCPMessage message) {

            Write(message.Code, message.Content);

        }

        /// <summary>
        /// Write message to stream
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        public void Write(TCPMessageCode code, byte[] data) {

            using (MemoryStream ms = new MemoryStream()) {

                ms.WriteByte((byte)code);

                byte second;

                if(data.Length <= 125) {

                    second = (byte)data.Length;
                    ms.WriteByte(second);

                } else if(data.Length <= 65535) {

                    second = 126;
                    ms.WriteByte(second);
                    TCPReaderWriter.WriteNumber(ms, (ushort)data.Length, false);

                } else {

                    second = 127;
                    ms.WriteByte(second);
                    TCPReaderWriter.WriteNumber(ms, (ulong)data.Length, false);

                }
                
                // encode data xor

                byte[] key = new byte[4];
                byte[] encoded = new byte[data.Length];
                RandomGen.Random.NextBytes(key);

                for (int e = 0; e < encoded.Length; e++) {

                    encoded[e] = (byte)(data[e] ^ key[e % 4]);

                }

                ms.Write(key, 0, key.Length);
                ms.Write(encoded, 0, encoded.Length);

                byte[] buffer = ms.ToArray();
                Stream.Write(buffer, 0, buffer.Length);

            }

        }

    }

}
