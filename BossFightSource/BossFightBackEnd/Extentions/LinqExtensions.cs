using System;
using System.Collections.Generic;
using System.Linq;

namespace BossFight.Extentions
{
    public static class IEnumerableExtensions
    {
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
