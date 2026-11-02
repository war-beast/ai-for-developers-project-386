using System.Net.Mail;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;

namespace CallCalendar.BLL.Service;

public sealed class BookingService(
    IEventTypeRepository eventTypeRepository,
    IBookingRepository bookingRepository,
    ISlotEngine slotEngine,
    TimeProvider timeProvider) : IBookingService
{
    private const int MaxGuestEmailLength = 254;
    private const int MaxCommentLength = 500;

    public CreateBookingResult Create(CreateBookingCommand command)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return CreateBookingResult.Invalid(errors);
        }

        var eventType = eventTypeRepository.GetById(command.EventTypeId!.Trim());
        if (eventType is null)
        {
            return CreateBookingResult.EventTypeMissing();
        }

        var start = command.Start!.Value;
        if (!slotEngine.IsValidStart(start, eventType.DurationMinutes))
        {
            return CreateBookingResult.SlotUnavailable();
        }

        var end = start.AddMinutes(eventType.DurationMinutes);
        var booking = Booking.TryCreate(
            eventType,
            start,
            command.GuestName!,
            command.GuestEmail!,
            command.Comment,
            bookingRepository.GetOverlapping(start, end),
            timeProvider.GetUtcNow());
        if (booking is null)
        {
            return CreateBookingResult.SlotAlreadyTaken();
        }

        return bookingRepository.Add(ToEntity(booking))
            ? CreateBookingResult.Success(booking)
            : CreateBookingResult.SlotAlreadyTaken();
    }

    public IReadOnlyList<Booking> GetUpcoming()
    {
        var now = timeProvider.GetUtcNow();
        var titles = eventTypeRepository
            .GetAll()
            .ToDictionary(type => type.Id, type => type.Title);

        // Луч (now, ∞): пересечение с ним равнозначно End > сейчас,
        // включая идущую встречу.
        return bookingRepository
            .GetOverlapping(now, DateTimeOffset.MaxValue)
            .OrderBy(booking => booking.Start)
            .Select(booking => ToModel(booking, titles.GetValueOrDefault(booking.EventTypeId)))
            .ToList();
    }

    private static Booking ToModel(BookingEntity entity, string? eventTypeTitle) =>
        new(
            entity.Id,
            entity.EventTypeId,
            eventTypeTitle ?? entity.EventTypeId,
            entity.Start,
            entity.End,
            entity.GuestName,
            entity.GuestEmail,
            entity.Comment,
            entity.CreatedAt);

    private static BookingEntity ToEntity(Booking booking) =>
        new()
        {
            Id = booking.Id,
            EventTypeId = booking.EventTypeId,
            Start = booking.Start,
            End = booking.End,
            GuestName = booking.GuestName,
            GuestEmail = booking.GuestEmail,
            Comment = booking.Comment,
            CreatedAt = booking.CreatedAt,
        };

    private static Dictionary<string, string[]> Validate(CreateBookingCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(command.EventTypeId))
        {
            errors["eventTypeId"] = ["Тип встречи обязателен."];
        }

        if (command.Start is null)
        {
            errors["start"] = ["Время начала обязательно."];
        }

        var guestName = command.GuestName?.Trim() ?? string.Empty;
        if (guestName.Length == 0)
        {
            errors["guestName"] = ["Имя обязательно."];
        }

        var guestEmail = command.GuestEmail?.Trim() ?? string.Empty;
        if (guestEmail.Length == 0)
        {
            errors["guestEmail"] = ["Email обязателен."];
        }
        else if (guestEmail.Length > MaxGuestEmailLength
                 || !MailAddress.TryCreate(guestEmail, out var address)
                 || address.Address != guestEmail)
        {
            errors["guestEmail"] = [$"Укажите корректный email не длиннее {MaxGuestEmailLength} символов."];
        }

        if ((command.Comment?.Trim().Length ?? 0) > MaxCommentLength)
        {
            errors["comment"] = [$"Комментарий не длиннее {MaxCommentLength} символов."];
        }

        return errors;
    }
}
