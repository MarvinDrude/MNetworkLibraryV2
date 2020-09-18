using System;
using System.Collections.Generic;
using System.Text;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Representing a tcp message received or sent
    /// </summary>
    public class TCPMessage {

        /// <summary>
        /// Client the message came from or was sent to
        /// </summary>
        public TCPServerClient Client { get; set; }

        /// <summary>
        /// Message sent over tcp
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// When the message was received or sent
        /// </summary>
        public DateTime Sent { get; set; }

        /// <summary>
        /// Code of the message
        /// </summary>
        public TCPMessageCode Code { get; set; } = TCPMessageCode.Message;
        
        /// <summary>
        /// Get string of content byte array utf8
        /// </summary>
        /// <returns></returns>
        public string ToStringContent() {

            if(Content == null || Content.Length == 0) {
                return "";
            }

            return Encoding.UTF8.GetString(Content);

        }

    }

}
