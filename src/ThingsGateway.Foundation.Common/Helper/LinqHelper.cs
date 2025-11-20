using ThingsGateway.Foundation.Common.DictionaryExtensions;

namespace ThingsGateway.Foundation.Common.LinqExtension;

public static class LinqHelper
{
    #region IEnumerableT
    /// <summary>
    /// 循环遍历每个元素，执行Action动作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">要遍历的集合</param>
    /// <param name="action">对每个元素执行的动作</param>
    /// <returns>返回原始集合</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        // 遍历集合中的每个元素
        foreach (var item in values)
        {
            // 对每个元素执行指定的动作
            action.Invoke(item);
        }

        // 返回原始集合
        return values;
    }

    /// <summary>
    /// 循环遍历每个元素，执行异步动作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">要遍历的集合</param>
    /// <param name="func">对每个元素执行的异步操作</param>
    /// <returns>返回原始集合</returns>
    public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> values, Func<T, Task> func)
    {
        // 遍历集合中的每个元素
        foreach (var item in values)
        {
            // 执行指定的异步操作
            await func.Invoke(item).ConfigureAwait(false);
        }

        // 返回原始集合
        return values;
    }
    #endregion

    public static IEnumerable<T> WhereIF<T>(this IEnumerable<T> thisValue, bool isOk, Func<T, bool> predicate)
    {
        if (isOk)
        {
            return thisValue.Where(predicate);
        }
        else
        {
            return thisValue;
        }
    }


    /// <summary>
    /// 将序列分批，每批固定数量
    /// </summary>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));

        List<T> batch = new List<T>(batchSize);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        // 剩余不足 batchSize 的最后一批
        if (batch.Count > 0)
            yield return batch;
    }

    /// <inheritdoc/>
    public static ICollection<T> AddIF<T>(this ICollection<T> thisValue, bool isOk, Func<T> predicate)
    {
        if (isOk)
        {
            thisValue.Add(predicate());
        }

        return thisValue;
    }

    /// <inheritdoc/>
    public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
    {
        var del = new List<T>();
        foreach (var obj in @this.Where(where))
        {
            del.Add(obj);
        }
        foreach (var obj in del)
        {
            @this.Remove(obj);
        }
    }
    /// <inheritdoc/>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> thisValue, bool isOk, Func<T, bool> predicate)
    {
        if (isOk)
        {
            thisValue = thisValue.Where(predicate);
        }
        return thisValue;
    }

    /// <inheritdoc/>
    public static void AddRange<TKey, TItem>(this Dictionary<TKey, TItem> @this, IEnumerable<KeyValuePair<TKey, TItem>> values)
    {
        foreach (var value in values)
        {
            @this.TryAdd(value.Key, value.Value);
        }
    }

    /// <inheritdoc/>
    public static void AddRange<T>(this ICollection<T> @this, IEnumerable<T> values)
    {
        foreach (T value in values)
        {
            @this.Add(value);
        }
    }
    /// <inheritdoc/>
    public static void AddRange<T>(this ICollection<T> @this, params T[] values)
    {
        foreach (T item in values)
        {
            @this.Add(item);
        }
    }

}