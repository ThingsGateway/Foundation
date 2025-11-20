using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Common.DictionaryExtensions;

/// <summary>并发字典扩展</summary>
public static class DictionaryHelper
{

    /// <summary>集合转为数组</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IList<TKey> ToKeyArray<TKey, TValue>(this IDictionary<TKey, TValue> collection, Int32 index = 0) where TKey : notnull
    {
        //if (collection == null) return null;

        if (collection is NonBlockingDictionary<TKey, TValue> cdiv && cdiv.Keys is IList<TKey> list) return list;

        if (collection.Count == 0) return [];
        lock (collection)
        {
            var arr = new TKey[collection.Count - index];
            collection.Keys.CopyTo(arr, index);
            return arr;
        }
    }

    /// <summary>集合转为数组</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IList<TValue> ToValueArray<TKey, TValue>(this IDictionary<TKey, TValue> collection, Int32 index = 0) where TKey : notnull
    {
        //if (collection == null) return null;

        //if (collection is NonBlockingDictionary<TKey, TValue> cdiv) return cdiv.Values as IList<TValue>;
        if (collection is NonBlockingDictionary<TKey, TValue> cdiv && cdiv.Values is IList<TValue> list) return list;

        if (collection.Count == 0) return [];
        lock (collection)
        {
            var arr = new TValue[collection.Count - index];
            collection.Values.CopyTo(arr, index);
            return arr;
        }
    }




    /// <summary>合并字典参数</summary>
    /// <param name="dic">字典</param>
    /// <param name="target">目标对象</param>
    /// <param name="overwrite">是否覆盖同名参数</param>
    /// <param name="excludes">排除项</param>
    /// <returns></returns>
    public static IDictionary<String, Object?> Merge(this IDictionary<String, Object?> dic, Object target, Boolean overwrite = true, String[]? excludes = null)
    {
        if (target?.GetType().IsBaseType() != false) return dic;

        var exs = excludes != null ? new HashSet<String>(excludes, StringComparer.OrdinalIgnoreCase) : null;
        foreach (var item in target.ToDictionary())
        {
            if (exs?.Contains(item.Key) != true)
            {
                if (overwrite || !dic.ContainsKey(item.Key)) dic[item.Key] = item.Value;
            }
        }

        return dic;
    }

    /// <summary>转为可空字典</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ToNullable<TKey, TValue>(this IDictionary<TKey, TValue> collection, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        //if (collection == null) return null;

        if (collection is NullableDictionary<TKey, TValue> dic && (comparer == null || dic.Comparer == comparer)) return dic;

        if (comparer == null)
            return new NullableDictionary<TKey, TValue>(collection);
        else
            return new NullableDictionary<TKey, TValue>(collection, comparer);
    }


#if NETFRAMEWORK || NETSTANDARD2_0
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> pairs, TKey key, TValue value)
    {
        if (!pairs.ContainsKey(key))
        {
            pairs.Add(key, value);
            return true;
        }
        return false;
    }
#endif

    /// <inheritdoc/>
    public static int RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> pairs, Func<KeyValuePair<TKey, TValue>, bool> func)
    {
        // 存储需要移除的键的列表，以便之后统一移除
        var list = new List<TKey>();
        foreach (var item in pairs)
        {
            // 使用提供的函数判断当前项目是否应该被移除
            if (func?.Invoke(item) == true)
            {
                list.Add(item.Key);
            }
        }

        // 记录成功移除的项目数量
        var count = 0;
        foreach (var item in list)
        {
            // 尝试移除项目，如果成功则增加计数
            if (pairs.Remove(item))
            {
                count++;
            }
        }
        // 返回成功移除的项目数量
        return count;
    }

    /// <summary>
    /// 根据指定的一组 key，批量从字典中筛选对应的键值对。
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam>
    /// <typeparam name="TValue">字典值类型</typeparam>
    /// <param name="dictionary">源字典</param>
    /// <param name="keys">要筛选的 key 集合</param>
    /// <returns>匹配到的键值对序列</returns>
    public static IEnumerable<KeyValuePair<TKey, TValue>> FilterByKeys<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        IEnumerable<TKey> keys)
    {
        if (keys == null) yield break;
        foreach (var key in keys)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }
    }

    /// <summary>
    /// 批量出队
    /// </summary>
    public static List<T> ToListWithDequeue<TKEY, T>(this NonBlockingDictionary<TKEY, T> values, int maxCount = 0)
    {
        if (maxCount <= 0)
        {
            maxCount = values.Count;
        }
        else
        {
            maxCount = Math.Min(maxCount, values.Count);
        }

        var list = new List<T>(maxCount);
        if (maxCount == 0) return list;
        var keys = values.Keys;
        foreach (var key in keys)
        {
            if (maxCount-- <= 0) break;
            if (values.TryRemove(key, out var result))
            {
                list.Add(result);
            }
        }
        return list;
    }

    /// <summary>
    /// 批量出队
    /// </summary>
    public static Dictionary<TKEY, T> ToDictWithDequeue<TKEY, T>(this NonBlockingDictionary<TKEY, T> values, int maxCount = 0)
    {
        if (maxCount <= 0)
        {
            maxCount = values.Count;
        }
        else
        {
            maxCount = Math.Min(maxCount, values.Count);
        }

        var dict = new Dictionary<TKEY, T>(maxCount);

        if (maxCount == 0) return dict;

        var keys = values.Keys;
        foreach (var key in keys)
        {
            if (maxCount-- <= 0) break;
            if (values.TryRemove(key, out var result))
            {
                dict.Add(key, result);
            }
        }
        return dict;
    }

    /// <summary>
    /// 批量出队
    /// </summary>
    public static IEnumerable<KeyValuePair<TKEY, T>> ToIEnumerableKVWithDequeue<TKEY, T>(this NonBlockingDictionary<TKEY, T> values, int maxCount = 0)
    {
        if (values.IsEmpty) yield break;

        if (maxCount <= 0)
        {
            maxCount = values.Count;
        }
        else
        {
            maxCount = Math.Min(maxCount, values.Count);
        }

        var keys = values.Keys;
        foreach (var key in keys)
        {
            if (maxCount-- <= 0) break;
            if (values.TryRemove(key, out var result))
            {
                yield return new(key, result);
            }
        }
    }




}