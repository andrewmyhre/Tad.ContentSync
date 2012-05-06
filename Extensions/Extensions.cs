using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ContentSync.Extensions {
    public static class Extensions {
        public static IEnumerable<KeyValuePair<string, string>> ToPairs(this NameValueCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            return collection.Cast<string>().Select(key => new KeyValuePair<string, string>(key, collection[key]));
        }
    }
}