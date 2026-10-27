using CallCalendar.DAL.Entity;

namespace CallCalendar.DAL.Interface;

public interface IEventTypeRepository
{
    IReadOnlyList<EventTypeEntity> GetAll();

    EventTypeEntity? GetById(string id);

    /// <summary>Добавляет тип, если slug ещё не занят; проверка и вставка атомарны.</summary>
    bool TryAdd(EventTypeEntity eventType);
}
