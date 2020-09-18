using MNetworkLib.TCP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MNetworkLib.Common {

    /// <summary>
    /// Helper class for random generation
    /// </summary>
    public static class RandomGen {

        /// <summary>
        /// Used to generate non-secruity-important random values
        /// </summary>
        public static Random Random { get; set; } = new Random();

        /// <summary>
        /// Contains all characters available for generation of random uid
        /// </summary>
        public static string RandomUIDPool { get; set; } = "abcdefghijklmnopqestuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Generates uid given dictionary and len
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GenRandomUID(IDictionary dict, uint len) {

            string result = null;

            do {

                result = "";

                for(int e = (int)len - 1; e >= 0; e--) {

                    result += RandomUIDPool[Random.Next(0, RandomUIDPool.Length)];

                }

            } while (dict.Contains(result));

            return result;

        }

    }

}
