using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;

namespace CallCalendar.DAL.Repository;

/// <summary>
/// Типы встреч в памяти процесса: регистрируется синглтоном и сбрасывается при рестарте.
/// </summary>
public sealed class InMemoryEventTypeRepository : IEventTypeRepository
{
    private readonly Lock _lock = new();
    private readonly List<EventTypeEntity> _eventTypes = [];

    public IReadOnlyList<EventTypeEntity> GetAll()
    {
        lock (_lock)
        {
            return [.. _eventTypes];
        }
    }

    public EventTypeEntity? GetById(string id)
    {
        lock (_lock)
        {
            return _eventTypes.Find(eventType => eventType.Id == id);
        }
    }

    public bool TryAdd(EventTypeEntity eventType)
    {
        lock (_lock)
        {
            if (_eventTypes.Exists(existing => existing.Id == eventType.Id))
            {
                return false;
            }

            _eventTypes.Add(eventType);
            return true;
        }
    }
}
