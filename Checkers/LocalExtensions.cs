using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public static class LocalExtensions
    {
        public static string BuildString<T>(this IEnumerable<T> collection)
        {
            var result = new StringBuilder();
            foreach(T item in collection)
            {
                result.AppendLine(item.ToString());
            }
            return result.ToString();
        }

        public static void RemoveEmptyItems(this IList<string> collection)
        {
            for(int i = 0; i < collection.Count;)
            {
                if(collection[i] == string.Empty)
                {
                    collection.RemoveAt(i);
                }
                else ++i;
            }
        }
    }
}
