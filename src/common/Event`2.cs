namespace Vezel.Celerity;

internal struct Event<T1, T2>
{
    private EventHandlerList<Action<T1, T2>> _handlers = new();

    public Event()
    {
    }

    public void Add(Action<T1, T2>? handler)
    {
        _handlers.Add(handler);
    }

    public void Remove(Action<T1, T2>? handler)
    {
        _handlers.Remove(handler);
    }

    public readonly void Raise(T1 arg1, T2 arg2)
    {
        _handlers.Raise((arg1, arg2), static (handler, t) => handler(t.arg1, t.arg2));
    }
}
