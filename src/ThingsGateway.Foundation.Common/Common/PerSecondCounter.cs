//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Common;

public class PerSecondCounter
{
    private readonly int _windowSize;      // 滑动窗口大小，单位秒
    private readonly long[] _slots;        // 每个槽记录对应秒的采集次数
    private int _currentIndex;             // 当前秒对应的槽
    private long _lastTickSeconds;         // 上一次滑动窗口更新的时间（秒）
    public PerSecondCounter(int windowSize = 5)
    {
        if (windowSize <= 0) throw new ArgumentOutOfRangeException(nameof(windowSize));
        _windowSize = windowSize;
        _slots = new long[_windowSize];
        _currentIndex = 0;
        _lastTickSeconds = GetCurrentSeconds();
    }

    // 采集事件调用
    public void Increment()
    {
        TickIfNeeded();
        Interlocked.Increment(ref _slots[_currentIndex]);
    }

    // 获取最近一秒的采集次数
    public long LastSecondCount
    {
        get
        {
            TickIfNeeded();
            return _slots[_currentIndex];
        }
    }

    /// <summary>
    /// 每秒采集次数的最大值
    /// </summary>
    public long MaxPerSecondCount
    {
        get
        {
            TickIfNeeded();
            return _slots.Max();
        }
    }
    // 获取最近 N 秒总采集次数
    public long GetTotalCount()
    {
        TickIfNeeded();
        long sum = 0;
        for (int i = 0; i < _windowSize; i++)
        {
            sum += Interlocked.Read(ref _slots[i]);
        }
        return sum;
    }

    // 每秒滑动一次窗口
    private void TickIfNeeded()
    {
        long nowSeconds = GetCurrentSeconds();
        long delta = nowSeconds - Interlocked.Read(ref _lastTickSeconds);
        if (delta <= 0) return;

        // 可能一次跳过多秒，逐个清零
        for (long i = 0; i < delta; i++)
        {
            int nextIndex = (_currentIndex + 1) % _windowSize;
            Interlocked.Exchange(ref _slots[nextIndex], 0);
            _currentIndex = nextIndex;
        }

        Interlocked.Exchange(ref _lastTickSeconds, nowSeconds);
    }

    private static long GetCurrentSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
