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

using System;
using System.Threading;

public sealed class LinkedCancellationTokenSourceCache : IDisposable
{
    private CancellationTokenSource? _cachedCts;

    private CancellationToken _token1;
    private CancellationToken _token2;
    private CancellationToken _token3;

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    /// <summary>
    /// 获取（或创建）Linked CTS
    /// </summary>
    public CancellationTokenSource GetLinkedTokenSource(
        CancellationToken token1,
        CancellationToken token2,
        CancellationToken token3 = default)
    {
        lock (_lock)
        {
            // 是否相同 token
            bool sameTokens =
                _token1.Equals(token1) &&
                _token2.Equals(token2) &&
                _token3.Equals(token3);

            if (!sameTokens || _cachedCts == null || _cachedCts.IsCancellationRequested)
            {
                _cachedCts?.Dispose();

                _cachedCts = CancellationTokenSource.CreateLinkedTokenSource(token1, token2, token3);

                _token1 = token1;
                _token2 = token2;
                _token3 = token3;
            }

            return _cachedCts;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _cachedCts?.Dispose();
            _cachedCts = null;
        }
        GC.SuppressFinalize(this);
    }
}