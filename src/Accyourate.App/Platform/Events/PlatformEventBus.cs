namespace Accyourate.App.Platform.Events;

public sealed class PlatformEventBus
{
    private readonly List<Action<PlatformEvent>> _handlers = new();

    public void Subscribe(Action<PlatformEvent> handler)
    {
        _handlers.Add(handler);
    }

    public void Publish(PlatformEvent platformEvent)
    {
        foreach (var handler in _handlers.ToArray())
            handler(platformEvent);
    }
}
