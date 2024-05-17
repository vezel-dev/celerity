// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity;

internal struct EventHandlerList<T>
    where T : Delegate
{
    public Delegate[]? Handlers { get; private set; }

    private readonly object _lock = new();

    private T? _event;

    public EventHandlerList()
    {
    }

    public void Add(T? handler)
    {
        lock (_lock)
        {
            _event = Unsafe.As<T>(Delegate.Combine(_event, handler));
            Handlers = _event?.GetInvocationList();
        }
    }

    public void Remove(T? handler)
    {
        lock (_lock)
        {
            _event = Unsafe.As<T>(Delegate.Remove(_event, handler));
            Handlers = _event?.GetInvocationList();
        }
    }

    [SuppressMessage("", "CA1031")]
    public readonly void Raise<TState>(TState state, Action<T, TState> raiser)
    {
        // No need for locking; we want a snapshot anyway.
        if (Handlers is { } handlers)
            _ = ThreadPool.UnsafeQueueUserWorkItem(
                static tup =>
                {
                    var exceptions = default(List<Exception>);

                    foreach (var handler in tup.Handlers)
                    {
                        try
                        {
                            tup.Raiser(Unsafe.As<T>(handler), tup.State);
                        }
                        catch (Exception ex)
                        {
                            (exceptions ??= []).Add(ex);
                        }
                    }

                    if (exceptions != null)
                        throw new AggregateException(exceptions);
                },
                (Handlers: handlers, State: state, Raiser: raiser),
                preferLocal: true);
    }
}
