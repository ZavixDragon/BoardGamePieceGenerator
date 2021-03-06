﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleSheetsToJsonConvertor.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action) => items.ToList().ForEach(action);
    }
}
