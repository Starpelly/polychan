namespace Polychan.GUI.Framework.Extensions
{
    /// <summary>
    /// This class holds extension methods for various purposes and should not be used explicitly, ever.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Adds the given item to the list according to standard sorting rules. Do not use on unsorted lists.
        /// </summary>
        /// <param name="list">The list to take values</param>
        /// <param name="item">The item that should be added.</param>
        /// <returns>The index in the list where the item was inserted.</returns>
        public static int AddInPlace<T>(this List<T> list, T item)
        {
            int index = list.BinarySearch(item);
            if (index < 0) index = ~index; // BinarySearch hacks multiple return values with 2's complement.
            list.Insert(index, item);
            return index;
        }

        /// <summary>
        /// Adds the given item to the list according to the comparers sorting rules. Do not use on unsorted lists.
        /// </summary>
        /// <param name="list">The list to take values</param>
        /// <param name="item">The item that should be added.</param>
        /// <param name="comparer">The comparer that should be used for sorting.</param>
        /// <returns>The index in the list where the item was inserted.</returns>
        public static int AddInPlace<T>(this List<T> list, T item, IComparer<T> comparer)
        {
            int index = list.BinarySearch(item, comparer);
            if (index < 0) index = ~index; // BinarySearch hacks multiple return values with 2's complement.
            list.Insert(index, item);
            return index;
        }
    }
}
