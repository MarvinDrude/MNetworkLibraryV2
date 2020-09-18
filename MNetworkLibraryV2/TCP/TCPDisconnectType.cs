using System;
using System.Collections.Generic;
using System.Text;

namespace MNetworkLib.TCP {

    public enum TCPDisconnectType {

        NoHandshake = 1,
        Disconnect = 2,
        Timeout = 3,
        Kick = 4

    }
    
}
