using MNetworkLib.Common;
using MNetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNetworkLibraryV2.TCP {

    public class TCPContent {

        public void WriteStream<T>(IOStream stream) where T : TCPContent, new() {

            stream.WriteObject((T)this);

        }

        public static bool ReadStream<T>(IOStream stream, out T ob) where T : new() {

            return stream.ReadObject<T>(out ob);

        }

    }

}
