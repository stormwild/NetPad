using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetPad.Utilities;

public static class CollectionUtil
{
    public static Task ForEachAsync<T>(this IEnumerable<T> collection, int degreeOfParallelism, Func<T, Task> asyncAction)
    {
        return Task.WhenAll(
            from partition in Partitioner.Create(collection).GetPartitions(degreeOfParallelism)
            select Task.Run(async delegate
            {
                using (partition)
                    while (partition.MoveNext())
                        await asyncAction(partition.Current);
            })
        );
    }

    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items) where T : class
    {
        foreach (var item in items)
        {
            hashSet.Add(item);
        }
    }
}
