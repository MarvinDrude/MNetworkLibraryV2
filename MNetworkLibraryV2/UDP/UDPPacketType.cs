using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNetworkLib.UDP {

    public enum UDPPacketType : byte {

        Ping = 1,
        Pong = 2,
        Auth = 3,
        Message = 4

    }

}
