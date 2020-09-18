using MNetworkLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNetworkLib.UDP {

    public class UDPPacket {

        public byte[] Data { get; set; }

        public UDPPacketType Type { get; set; }

        public UDPPacket(UDPPacketType type = UDPPacketType.Message) {
            Type = type;
        }

        public UDPPacket(byte[] data, UDPPacketType type = UDPPacketType.Message) {
            Type = type;
            Data = data;
        }

        public byte[] GetRawContent() {

            using(IOStream stream = new IOStream()) {

                stream.WriteByte((byte)Type);
                stream.Write(Data, 0, Data.Length);

                return stream.ToArray();

            }

        }

        public static UDPPacket GetPacket(byte[] arr) {

            if(!Enum.IsDefined(typeof(UDPPacketType), arr[0])) {
                return null;
            }

            byte[] content = new byte[arr.Length - 1];
            System.Buffer.BlockCopy(arr, 1, content, 0, content.Length);

            return new UDPPacket(content, (UDPPacketType)arr[0]);

        }

    }

}
