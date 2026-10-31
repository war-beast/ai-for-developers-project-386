using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Interface;

public interface ISlotEngine
{
    /// <summary>Свободные слоты типа встречи на всё окно записи.</summary>
    SlotWindowResult GetWindow(string eventTypeId);

    /// <summary>Совпадает ли начало со слотом сетки текущего окна (без учёта записей).</summary>
    bool IsValidStart(DateTimeOffset start, int durationMinutes);
}
