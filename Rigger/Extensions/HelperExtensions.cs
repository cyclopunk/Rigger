using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Rigger.Extensions
{
    public static class HelperExtensions
    {
       /// <summary>
       /// Perform an action a number of times.
       /// </summary>
       /// <param name="numberOfTimes"></param>
       /// <param name="action"></param>
       /// <returns></returns>
        public static T[] Times<T>(this int numberOfTimes, Func<T> action)
        {
            var objs = new T[numberOfTimes];

            for (int i = 0; i < numberOfTimes; i++)
            {
                objs[i] = action();
            }

            return objs;
        }

        public static void Times(this int numberOfTimes, Action action)
        {
          
            for (int i = 0; i < numberOfTimes; i++)
            {
                action();
            }

        }

        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(this byte[] data)
        {
            if (data == null)
                return default(T);

            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

    }
}