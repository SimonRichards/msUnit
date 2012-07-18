using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace msUnit {
    static class Extensions {
        internal static IEnumerable<T> SelectWithAttribute<T>(this IEnumerable<T> searchable, Type attribute)
            where T : ICustomAttributeProvider {
            return from item in searchable
                   where item.GetCustomAttributes(typeof (TestMethod), true).Any()
                   select item;
        }
    }
}
