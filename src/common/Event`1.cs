// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity;

internal struct Event<T>
{
    private EventHandlerList<Action<T>> _handlers = new();

    public Event()
    {
    }

    public void Add(Action<T>? handler)
    {
        _handlers.Add(handler);
    }

    public void Remove(Action<T>? handler)
    {
        _handlers.Remove(handler);
    }

    public readonly void Raise(T arg)
    {
        _handlers.Raise(arg, static (handler, arg) => handler(arg));
    }
}
