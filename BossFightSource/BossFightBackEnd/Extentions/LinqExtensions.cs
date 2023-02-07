using System;
using System.Collections.Generic;
using System.Linq;

namespace BossFight.Extentions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Removes the FIRST occurrence
        /// </summary>
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> source, Func<T, bool> pred)
        {
            var result = source;
            if (source.NullSafeAny())
            {
                var toBeRemoved = source.FirstOrDefault(pred);
                var ls = source.ToList();
                ls.RemoveAt(ls.IndexOf(toBeRemoved));
                result = ls;
            }
            return result;
        }

        public static bool NullSafeAny<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        public static bool NullSafeAny<T>(this IEnumerable<T> source, Func<T, bool> pred)
        {
            return source != null && source.Any(pred);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach(T item in source) action(item);
        }
    }
}