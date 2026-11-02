using CallCalendar.DAL.Entity;

namespace CallCalendar.DAL.Interface;

public interface IBookingRepository
{
    /// <summary>Записи, пересекающие полуинтервал [start, end).</summary>
    IReadOnlyList<BookingEntity> GetOverlapping(DateTimeOffset start, DateTimeOffset end);

    /// <summary>
    /// Атомарная вставка под блокировкой; false — если поверх записи уже
    /// лежит пересекающая. Guard хранилища, аналог exclusion-constraint
    /// в БД: решение об отклонении принимает агрегат «Запись».
    /// </summary>
    bool Add(BookingEntity booking);
}
