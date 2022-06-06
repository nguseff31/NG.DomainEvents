using System.Collections.Concurrent;

namespace NG.DomainEvents.Data;

public class DomainEventRelayService {
    readonly ConcurrentBag<List<(long eventId, string handlerType)>?> _events = new();

    DomainEventRelayService() { }

    static DomainEventRelayService? _instance;
    static readonly object Lock = new();

    public static DomainEventRelayService GetInstance() {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (_instance == null) {
            lock (Lock) {
                if (_instance == null) {
                    _instance = new DomainEventRelayService();
                }
            }
        }

        return _instance;
    }

    public IEnumerable<(long eventId, string handlerType)> Take() {
        _events.TryTake(out var result);
        return result ?? Enumerable.Empty<(long eventId, string handlerType)>();
    }

    public void Push(IEnumerable<(long eventId, string handlerType)> events) {
        _events.Add(events.ToList());
    }
}
