using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MNetworkLib.Common {

    /// <summary>
    /// Logger class in order to log information
    /// </summary>
    public static class Logger {

        /// <summary>
        /// Object to lock console write operation
        /// </summary>
        private static readonly object ConsoleWriteLock = new object();

        /// <summary>
        /// Log Event Handler
        /// </summary>
        /// <param name="time"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        public delegate void LogEventHandler(DateTime time, string category, string message);

        /// <summary>
        /// Log Event, called when a new log entry is on the stack
        /// </summary>
        public static event LogEventHandler OnLog;

        /// <summary>
        /// Log new logging entry
        /// </summary>
        /// <param name="time"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        public static void Write(string category, string message) {
            OnLog?.Invoke(DateTime.Now, category, message);
        }

        /// <summary>
        /// Write new logging entry with error attached
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public static void Write(string category, string message, Exception e) {
            Write(category, message + ": " + Regex.Replace((e.Message + ", " + e.StackTrace), @"\t|\n|\r", ""));
        }

        public static void AddDefaultConsoleLogging() {

            OnLog += (time, cat, mes) => {

                lock(ConsoleWriteLock) {

                    string message = "[" + time.ToString() + "]";
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(message);

                    message = "[" + cat + "]";

                    if (cat == "SUCCESS") {
                        Console.ForegroundColor = ConsoleColor.Green;
                    } else if (cat == "FAILED") {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    } else if (cat == "INFO" || cat == "INIT") {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                    } else if (cat == "REGION") {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    Console.Write(message);

                    message = " " + mes;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(message);
                    Console.WriteLine();

                }

            };

        }

    }

}
