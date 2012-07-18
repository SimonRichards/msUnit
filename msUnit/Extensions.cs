using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msUnit {
    static class Extensions {
        internal static IEnumerable<T> SelectWithAttribute<T>(this IEnumerable<T> searchable, Type attribute)
            where T : ICustomAttributeProvider {
            return from item in searchable
                   where item.GetCustomAttributes(typeof (TestMethodAttribute), true).Any()
                   select item;
        }
    }
}
