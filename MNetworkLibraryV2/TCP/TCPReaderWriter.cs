using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MNetworkLib.TCP {

    /// <summary>
    /// Helper to write and read stuff off a stream
    /// </summary>
    public static class TCPReaderWriter {

        public static Type SHORT_TYPE = typeof(ushort);

        public static void WriteObject<T>(Stream stream, T ob, bool littleEndian) {

            if (!TCPBase.Ordering.ContainsKey(typeof(T))) {
                TCPBase.AddMessageObject(typeof(T));
            }

            var list = TCPBase.Ordering[typeof(T)];

            foreach(var prop in list) {

                Type curr = prop.PropertyType;

                if (curr == typeof(double)) {

                    WriteNumber(stream, (double)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(float)) {

                    WriteNumber(stream, (float)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(ushort)) {

                    WriteNumber(stream, (ushort)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(short)) {

                    WriteNumber(stream, (short)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(int)) {

                    WriteNumber(stream, (int)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(uint)) {

                    WriteNumber(stream, (uint)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(long)) {

                    WriteNumber(stream, (long)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(ulong)) {

                    WriteNumber(stream, (ulong)prop.GetValue(ob, null), littleEndian);

                } else if (curr == typeof(byte)) {

                    stream.Write(new byte[] { (byte)prop.GetValue(ob, null) }, 0, 1);

                }

            }

            string w = "";

        }

        public static T ReadObject<T>(Stream stream, bool littleEndian, out bool error) where T : new() {

            T ob = new T();

            if(!TCPBase.Ordering.ContainsKey(typeof(T))) {
                TCPBase.AddMessageObject(typeof(T));
            }

            var list = TCPBase.Ordering[typeof(T)];

            foreach(var prop in list) {

                Type curr = prop.PropertyType;

                if (curr == typeof(double)) {

                    double val = ReadDouble(stream, littleEndian, out error);

                    if(error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(float)) {

                    float val = ReadFloat(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(ushort)) {

                    ushort val = ReadUShort(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(short)) {

                    short val = ReadShort(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(int)) {

                    int val = ReadInt(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(uint)) {

                    uint val = ReadUInt(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(long)) {

                    long val = ReadLong(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(ulong)) {

                    ulong val = ReadULong(stream, littleEndian, out error);

                    if (error) {
                        return default(T);
                    }

                    prop.SetValue(ob, val);

                } else if (curr == typeof(byte)) {

                    byte[] val = Read(stream, 1);

                    if (val == null) {
                        error = true;
                        return default(T);
                    }

                    prop.SetValue(ob, val[0]);

                }

            }

            error = false;
            return ob;

        }

        public static void WriteNumber(Stream stream, double number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, float number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, int number, bool littleEndian) {
            
            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, uint number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, ushort number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, short number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, long number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteNumber(Stream stream, ulong number, bool littleEndian) {

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static void WriteString(Stream stream, string text, bool prependLength, bool littleEndian) {

            byte[] buffer = Encoding.UTF8.GetBytes(text);

            if(prependLength)
                WriteNumber(stream, (uint)buffer.Length, littleEndian);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            stream.Write(buffer, 0, buffer.Length);

        }

        public static double ReadDouble(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 8);

            if(buffer == null) {
                error = true;
                return -1;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToDouble(buffer, 0);

        }

        public static float ReadFloat(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 4);

            if (buffer == null) {
                error = true;
                return -1;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToSingle(buffer, 0);

        }

        public static ushort ReadUShort(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 2);

            if (buffer == null) {
                error = true;
                return 0;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToUInt16(buffer, 0);

        }

        public static short ReadShort(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 2);

            if (buffer == null) {
                error = true;
                return -1;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToInt16(buffer, 0);

        }

        public static uint ReadUInt(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 4);

            if (buffer == null) {
                error = true;
                return 0;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToUInt32(buffer, 0);

        }

        public static int ReadInt(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 4);

            if (buffer == null) {
                error = true;
                return -1;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToInt32(buffer, 0);

        }

        public static ulong ReadULong(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 8);

            if (buffer == null) {
                error = true;
                return 0;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToUInt64(buffer, 0);

        }

        public static long ReadLong(Stream stream, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, 8);

            if (buffer == null) {
                error = true;
                return -1;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return BitConverter.ToInt64(buffer, 0);

        }

        public static string ReadString(Stream stream, uint len, bool littleEndian, out bool error) {

            byte[] buffer = Read(stream, len);

            if (buffer == null) {
                error = true;
                return null;
            }

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            error = false;
            return Encoding.UTF8.GetString(buffer);

        }

        public static string ReadString(Stream stream, bool littleEndian, out bool error) {

            bool err;
            uint len = ReadUInt(stream, littleEndian, out err);

            if(err) {
                error = true;
                return null;
            }

            return ReadString(stream, len, littleEndian, out error);

        }

        public static byte[] Read(Stream stream, uint len) {

            byte[] buffer = new byte[len];
            int read = 0;

            read = stream.Read(buffer, 0, buffer.Length);

            if (read < len) {

                return null;

            }

            return buffer;

        }

    }
}
