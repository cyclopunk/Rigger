using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Rigger.Utility;

namespace Rigger.Extensions
{
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serialize an object  into a byte array. Note: this is a
        /// brittle way of object serialization.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>A byte array that is a representation of the object</returns>
        public static byte[] Serialize
        (
            this object obj
        )
        {
            IFormatter formatter = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialize an object from a byte array.
        /// </summary>
        /// <typeparam name="TOutput">The type of the object to return</typeparam>
        /// <param name="byteArray">The byte array containing the object information</param>
        /// <returns>The deserialized object.</returns>
        public static TOutput Deserialize<TOutput>
        (
            this Byte[] byteArray
        )
        {
            IFormatter formatter = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                TOutput output = (TOutput) formatter.Deserialize(ms);

                return output;
            }
        }
        /// <summary>
        /// Helper method to convert an object into JSON. Uses Newtonsoft.Json as a
        /// default provider.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        /// <summary>
        /// Helper extension to convert an object from a JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string o)
        {
            return JsonConvert.DeserializeObject<T>(o);
        }

        /// <summary>
        /// Experemental support for the monad wrapper for types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T Json<T>(this A<T> t, string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}