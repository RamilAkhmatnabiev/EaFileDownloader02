using System.Collections.Generic;


namespace EaFileDownloader02
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Преобразует коллекцию в HashSet
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="source">Источник</param>
        /// <param name="comparer">Компаратор</param>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return comparer == null
                ? new HashSet<T>(source)
                : new HashSet<T>(source, comparer);
        }
    }
}
