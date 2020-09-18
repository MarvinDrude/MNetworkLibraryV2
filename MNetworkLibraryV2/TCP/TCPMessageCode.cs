using System;
using System.Collections.Generic;
using System.Text;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Identify differend messages
    /// </summary>
    public enum TCPMessageCode {

        Message = 1,
        Close = 2,
        Ping = 3,
        Pong = 4,
        Init = 5,

    }
}
