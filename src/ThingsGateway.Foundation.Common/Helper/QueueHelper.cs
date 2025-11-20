using System.Collections.Concurrent;

namespace System.Collections.Generic;

public static class QueueHelper
{



    /// <summary>从队列里面获取指定个数元素</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">消费集合</param>
    /// <param name="count">元素个数</param>
    /// <returns></returns>
    public static IEnumerable<T> Take<T>(this Queue<T> collection, Int32 count)
    {
        if (collection == null) yield break;

        while (count-- > 0 && collection.Count > 0)
        {
            yield return collection.Dequeue();
        }
    }

    /// <summary>从消费集合里面获取指定个数元素</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">消费集合</param>
    /// <param name="count">元素个数</param>
    /// <returns></returns>
    public static IEnumerable<T> Take<T>(this IProducerConsumerCollection<T> collection, Int32 count)
    {
        if (collection == null) yield break;

        while (count-- > 0 && collection.TryTake(out var item))
        {
            yield return item;
        }
    }




    /// <summary>
    /// 批量出队
    /// </summary>
    public static List<T> ToListWithDequeue<T>(this ConcurrentQueue<T> values, int maxCount = 0)
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
        while (maxCount-- > 0 && values.TryDequeue(out var result))
        {
            list.Add(result);
        }
        return list;
    }


#if NETFRAMEWORK || NETSTANDARD2_0
    public static bool TryDequeue<T>(this Queue<T> queue, [MaybeNullWhen(false)] out T result)
    {
        try
        {
            if (queue.Count > 0)
            {
                result = queue.Dequeue();
                return true;
            }
            result = default;
            return false;
        }
        catch (Exception)
        {
            result = default;
            return false;
        }
    }



#endif
    extension<T>(Queue<T> queue)
    {
        public bool IsEmpty
        {
            get => queue.Count == 0;
        }
    }
}