using MNetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNetworkLib.Common {

    public class IOStream : MemoryStream {

        public IOStream()
            : base() {

        }

        public IOStream(byte[] arr)
            : base(arr) {

        }

        public void WriteObject<T>(T ob) where T : new() => TCPReaderWriter.WriteObject<T>(this, ob, false);

        public void WriteFloat(float number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteDouble(double number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteInt(int number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteUInt(uint number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteLong(long number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteULong(ulong number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteUShort(ushort number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteShort(short number) => TCPReaderWriter.WriteNumber(this, number, false);

        public void WriteString(string text, bool prependLength = true) => TCPReaderWriter.WriteString(this, text, prependLength, false);

        public bool ReadObject<T>(out T ob) where T : new() {

            bool error;
            ob = TCPReaderWriter.ReadObject<T>(this, false, out error);

            return error;

        }

        public bool ReadFloat(out float val) {

            bool error;
            val = TCPReaderWriter.ReadFloat(this, false, out error);

            return error;

        }

        public bool ReadDouble(out double val) {

            bool error;
            val = TCPReaderWriter.ReadDouble(this, false, out error);

            return error;

        }

        public bool ReadInt(out int val) {

            bool error;
            val = TCPReaderWriter.ReadInt(this, false, out error);

            return error;

        }

        public bool ReadUInt(out uint val) {

            bool error;
            val = TCPReaderWriter.ReadUInt(this, false, out error);

            return error;

        }

        public bool ReadLong(out long val) {

            bool error;
            val = TCPReaderWriter.ReadLong(this, false, out error);

            return error;

        }

        public bool ReadULong(out ulong val) {

            bool error;
            val = TCPReaderWriter.ReadULong(this, false, out error);

            return error;

        }

        public bool ReadUShort(out ushort val) {

            bool error;
            val = TCPReaderWriter.ReadUShort(this, false, out error);

            return error;

        }

        public bool ReadShort(out short val) {

            bool error;
            val = TCPReaderWriter.ReadShort(this, false, out error);

            return error;

        }

        public bool ReadString(uint len, out string val) {

            bool error;
            val = TCPReaderWriter.ReadString(this, len, false, out error);

            return error;

        }

        public bool ReadString(out string val) {

            bool error;
            val = TCPReaderWriter.ReadString(this, false, out error);

            return error;

        }

    }

}
