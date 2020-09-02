using System;
using System.Collections;
using System.Collections.Generic;

namespace Rigger.ManagedTypes.Features
{
    /// <summary>
    /// A Dictionary that will always return a value for any key passed
    /// in equal to the key. Add is a NOP and all enumerators will
    /// return blank enumerators.
    ///
    /// Any attempts to set a key value pair will throw an exception.
    /// This class was created to use any string as a constant in Linq.Dynamic
    /// </summary>
    public class InfiniteDictionary : IDictionary<string, object>
    {
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new List<KeyValuePair<string, object>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            // nothin
        }

        public void Clear()
        {
            // nothin
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return true;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            // nothing
        }

        public bool Remove(KeyValuePair<string, object> item)
        {

            return true;
        }

        public int Count { get; } = 1;
        public bool IsReadOnly { get; } = true;
        public void Add(string key, object value)
        {
            // nothing
        }

        public bool ContainsKey(string key)
        {
            return true;
        }

        public bool Remove(string key)
        {
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            value = key;

            return true;
        }

        public object this[string key]
        {
            get => key;
            set => throw new NotImplementedException();
        }

        public ICollection<string> Keys { get; } = new List<string>();
        public ICollection<object> Values { get; } = new List<object>();
    }
}