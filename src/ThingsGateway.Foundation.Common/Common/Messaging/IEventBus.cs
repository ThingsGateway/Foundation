using System.Collections.Concurrent;

using ThingsGateway.Foundation.Common.Collections;
using ThingsGateway.Foundation.Common.Data;

#if !NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace ThingsGateway.Foundation.Common.Messaging;

/// <summary>事件总线</summary>
public interface IEventBus<TEvent>
{
    /// <summary>发布事件</summary>
    /// <param name="event">事件</param>
    /// <param name="context">上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Int32> PublishAsync(TEvent @event, IEventContext<TEvent>? context = null, CancellationToken cancellationToken = default);

    /// <summary>订阅事件</summary>
    /// <param name="handler">事件处理器</param>
    /// <param name="clientId">客户标识。每个客户只能订阅一次，重复订阅将会挤掉前一次订阅</param>
    Boolean Subscribe(IEventHandler<TEvent> handler, String clientId = "");

    /// <summary>取消订阅</summary>
    /// <param name="clientId">客户标识。订阅时使用的标识</param>
    Boolean Unsubscribe(String clientId = "");
}

/// <summary>事件处理器</summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<TEvent>
{
    /// <summary>处理事件</summary>
    /// <param name="event">事件</param>
    /// <param name="context">上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task HandleAsync(TEvent @event, IEventContext<TEvent>? context, CancellationToken cancellationToken);
}

/// <summary>默认事件总线。即时分发消息，不存储</summary>
/// <remarks>
/// 即时分发消息，意味着不在线的订阅者将无法收到消息。
/// </remarks>
public class EventBus<TEvent> : DisposeBase, IEventBus<TEvent>
{
    private readonly NonBlockingDictionary<String, IEventHandler<TEvent>> _handlers = [];
    /// <summary>已订阅的事件处理器集合</summary>
    public IDictionary<String, IEventHandler<TEvent>> Handlers => _handlers;

    private readonly Pool<EventContext<TEvent>> _pool = new();
    /// <summary>发布事件</summary>
    /// <param name="event">事件</param>
    /// <param name="context">上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    public virtual Task<Int32> PublishAsync(TEvent @event, IEventContext<TEvent>? context = null, CancellationToken cancellationToken = default) => DispatchAsync(@event, context, cancellationToken);

    /// <summary>分发事件给各个处理器。进程内分发</summary>
    /// <param name="event"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<Int32> DispatchAsync(TEvent @event, IEventContext<TEvent>? context, CancellationToken cancellationToken)
    {
        var rs = 0;

        // 创建上下文，循环调用处理器
        EventContext<TEvent>? ctx = null;
        if (context == null)
        {
            // 从对象池中获取上下文
            ctx = _pool.Get();
            ctx.EventBus = this;
            context = ctx;
        }
        //context ??= new EventContext<TEvent>(this);
        foreach (var item in _handlers)
        {
            var handler = item.Value;
            await handler.HandleAsync(@event, context, cancellationToken).ConfigureAwait(false);
            rs++;
        }
        if (ctx != null)
        {
            ctx.Reset();
            _pool.Return(ctx);
        }
        return rs;
    }

    /// <summary>订阅消息</summary>
    /// <param name="handler">处理器</param>
    /// <param name="clientId">客户标识。每个客户只能订阅一次，重复订阅将会挤掉前一次订阅</param>
    public virtual Boolean Subscribe(IEventHandler<TEvent> handler, String clientId = "")
    {
        _handlers[clientId] = handler;

        return true;
    }

    /// <summary>取消订阅</summary>
    /// <param name="clientId">客户标识。订阅时使用的标识</param>
    public virtual Boolean Unsubscribe(String clientId = "") => _handlers.TryRemove(clientId, out _);
}

/// <summary>事件总线扩展</summary>
public static class EventBusExtensions
{
    /// <summary>订阅事件</summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="bus">事件总线</param>
    /// <param name="action">事件处理方法</param>
    /// <param name="clientId">客户标识。每个客户只能订阅一次，重复订阅将会挤掉前一次订阅</param>
    public static void Subscribe<TEvent>(this IEventBus<TEvent> bus, Action<TEvent> action, String clientId = "") => bus.Subscribe(new DelegateEventHandler<TEvent>(action), clientId);

    /// <summary>订阅事件</summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="bus">事件总线</param>
    /// <param name="action">事件处理方法</param>
    /// <param name="clientId">客户标识。每个客户只能订阅一次，重复订阅将会挤掉前一次订阅</param>
    public static void Subscribe<TEvent>(this IEventBus<TEvent> bus, Action<TEvent, IEventContext<TEvent>> action, String clientId = "") => bus.Subscribe(new DelegateEventHandler<TEvent>(action), clientId);

    /// <summary>订阅事件</summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="bus">事件总线</param>
    /// <param name="action">事件处理方法</param>
    /// <param name="clientId">客户标识。每个客户只能订阅一次，重复订阅将会挤掉前一次订阅</param>
    public static void Subscribe<TEvent>(this IEventBus<TEvent> bus, Func<TEvent, Task> action, String clientId = "") => bus.Subscribe(new DelegateEventHandler<TEvent>(action), clientId);

    /// <summary>订阅事件</summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="bus">事件总线</param>
    /// <param name="action">事件处理方法</param>
    /// <param name="clientId">客户标识。每个客户只能订阅一次，重复订阅将会挤掉前一次订阅</param>
    public static void Subscribe<TEvent>(this IEventBus<TEvent> bus, Func<TEvent, IEventContext<TEvent>, CancellationToken, Task> action, String clientId = "") => bus.Subscribe(new DelegateEventHandler<TEvent>(action), clientId);
}

/// <summary>事件上下文接口</summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventContext<TEvent>
{
    /// <summary>事件总线</summary>
    IEventBus<TEvent> EventBus { get; }
}

/// <summary>事件上下文</summary>
/// <typeparam name="TEvent"></typeparam>
public class EventContext<TEvent> : IEventContext<TEvent>, IExtend
{
    /// <summary>事件总线</summary>
    public IEventBus<TEvent> EventBus { get; set; } = null!;

    /// <summary>数据项</summary>
    public IDictionary<String, Object?> Items { get; } = new NullableDictionary<String, Object?>();

    /// <summary>设置 或 获取 数据项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Object? this[String key] { get => Items.TryGetValue(key, out var obj) ? obj : null; set => Items[key] = value; }

    /// <summary>重置上下文，便于放入对象池</summary>
    public void Reset()
    {
        // 清空上下文数据
        EventBus = null!;
        Items.Clear();
    }
}

/// <summary>Action事件处理器</summary>
/// <typeparam name="TEvent"></typeparam>
/// <param name="method"></param>
public class DelegateEventHandler<TEvent>(Delegate method) : IEventHandler<TEvent>
{
    /// <summary>处理事件</summary>
    /// <param name="event">事件</param>
    /// <param name="context">上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public Task HandleAsync(TEvent @event, IEventContext<TEvent>? context, CancellationToken cancellationToken = default)
    {
        if (method is Func<TEvent, Task> func) return func(@event);
        if (method is Func<TEvent, IEventContext<TEvent>?, CancellationToken, Task> func2) return func2(@event, context, cancellationToken);

        if (method is Action<TEvent> act)
            act(@event);
        else if (method is Action<TEvent, IEventContext<TEvent>?> act2)
            act2(@event, context);
        else
            throw new NotSupportedException();

        return TaskEx.CompletedTask;
    }
}