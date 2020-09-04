using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rigger.Extensions
{
    public static class CollectionExtensions
    {
        
        /// <summary>
        /// Combine two enumerations into one.
        /// </summary>
        /// <typeparam name="TCombType">The type that will be combined</typeparam>
        /// <param name="list">An enumeration of enumerations that will be combined into one series.</param>
        /// <returns>The combined enumeration.</returns>
        public static IEnumerable<TCombType> Combine<TCombType>(this IEnumerable<IEnumerable<TCombType>> list)
        {
            return list.SelectMany(o => o, (types, type) => type);
        }

        /// <summary>
        /// ForEach is only available on lists, not IEnumerable enumerations.
        /// </summary>
        /// <typeparam name="TCollectionType"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="forEachExpression"></param>
        /// <returns></returns>
        public static void ForEach<TCollectionType>(this IEnumerable<TCollectionType> enumerable, Action<TCollectionType> forEachExpression)
        {
            foreach (var el in enumerable)
            {
                forEachExpression.Invoke(el);
            }
        }

        /// <summary>
        ///  Method to allow a list to work like a stack.
        /// </summary>
        /// <typeparam name="TCollectionType"></typeparam>
        /// <param name="listToPop"></param>
        /// <returns></returns>
        public static TCollectionType Pop<TCollectionType>(this List<TCollectionType> listToPop)
        {
            if (listToPop.Count == 0)
            {
                throw new OverflowException("List of size 0 was popped.");
            }

            TCollectionType itemAtTop = listToPop[0];

            listToPop.RemoveAt(0);

            return itemAtTop;
        }

        /// <summary>
        /// Add FindAll to arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T[] FindAll<T>(this T[] items, Predicate<T> predicate)
        {
            return Array.FindAll(items, predicate);
        }

        /// <summary>
        /// Add Find() to arrays.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T Find<T>(this T[] items, Predicate<T> predicate)
        {
            return Array.Find(items, predicate);
        }

        /// <summary>
        /// Add a simple method to Get a value from a map or populate the map with the value if that is not found.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="actionToPut"></param>
        /// <returns></returns>
        public static TValue GetOrPut<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> actionToPut)
        {
        
            if (!dictionary.ContainsKey(key))
            {
                var v = actionToPut();

                dictionary.Add(key, v);

                return v;
            }

            dictionary.TryGetValue(key, out var value);

            return value;
        }


     
        /// <summary>
        /// Return the first value of an enumeration or an alternate if the enumeration is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The enumeration</param>
        /// <param name="alternate">The value to return</param>
        /// <returns></returns>
        public static T FirstOr<T>(this IEnumerable<T> source, T alternate)
            => source.DefaultIfEmpty(alternate).First();

        /// <summary>
        /// Return the first value of a Where application to an enumeration or an alternate if the enumeration is empty.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="source">The enumeration to apply the predicate to.</param>
        /// <param name="predicate">The predicate to apply to the enumeration</param>
        /// <param name="alternate">The alternate to return if the predicate returns no values</param>
        /// <returns>The first result of a predicate or an alternately passed in object.</returns>
        public static T FirstOr<T>(this IEnumerable<T> source,  Func<T, bool> predicate, T alternate)
            => source.Where(predicate).FirstOr(alternate);

        /// <summary>
        /// Returns the only distinct element of a sequence.
        /// Logical Equivalent of IEnumerable.Distinct().Single/SingleOrDefault(), but more configurable & performant.
        /// Logical Complement to SQL GROUP BY, aggregating instead of grouping by superfluous grouping fields.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A Generic IEnumerable of which to return the single element.</param>
        /// <param name="defaultIfZero">Whether to return default or throw an InvalidOperationException when the input sequence is empty.</param>
        /// <param name="defaultIfMany">Whether to return default or throw an InvalidOperationException when the input sequence contains more than one distinct element.</param>
        /// <param name="ignoreNulls">Whether nulls should be ignored in the input sequence.</param>
        /// <exception cref="ArgumentNullException">source is null</exception>
        /// <exception cref="InvalidOperationException">The input sequence contains more than one distinct element.-or-The input sequence is empty.</exception>
        /// <returns>The single distinct element of the input sequence.</returns>
        public static TSource Only<TSource>(
            this IEnumerable<TSource> source,
            bool defaultIfZero,
            bool defaultIfMany,
            bool ignoreNulls
            // TODO: Func<bool, TSource> predicate = null,
            // TODO: IEqualityComparer<TSource> comparer = null,
        ) {
            // ReSharper disable 3 PossibleMultipleEnumeration
            // While we can avoid enumerating multiple times by writing the loops by hand, using built-in LINQ methods is much cleaner.

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (ignoreNulls
                && default(TSource) == null // Type is Reference or NullableValue
            ) {
                source = source.Where(x => x != null);
            }

            TSource first = defaultIfZero
                ? source.FirstOrDefault()
                : source.First();

            // When empty:
            // All() returns true
            // Any() returns false
            if (source.Skip(1).All(x => Equals(x, first)))
            {
                return first;
            }
            else
            {
                if (defaultIfMany)
                {
                    return default;
                }
                else
                {
                    throw new InvalidOperationException("Sequence contains more than one distinct element");
                }
            }
        }
    }
}