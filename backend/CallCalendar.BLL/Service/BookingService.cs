using System.Net.Mail;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;

namespace CallCalendar.BLL.Service;

public sealed class BookingService(
    IEventTypeRepository eventTypeRepository,
    IBookingRepository bookingRepository,
    OwnerOptions options,
    TimeProvider timeProvider) : IBookingService
{
    private const int MaxGuestNameLength = 100;
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
        if (!SlotGrid.IsSlot(start, eventType.DurationMinutes, options, timeProvider))
        {
            return CreateBookingResult.SlotUnavailable();
        }

        var entity = new BookingEntity
        {
            Id = Guid.NewGuid(),
            EventTypeId = eventType.Id,
            Start = start,
            End = start.AddMinutes(eventType.DurationMinutes),
            GuestName = command.GuestName!.Trim(),
            GuestEmail = command.GuestEmail!.Trim(),
            Comment = string.IsNullOrWhiteSpace(command.Comment) ? null : command.Comment.Trim(),
            CreatedAt = timeProvider.GetUtcNow(),
        };

        return bookingRepository.TryAdd(entity)
            ? CreateBookingResult.Success(ToModel(entity))
            : CreateBookingResult.SlotAlreadyTaken();
    }

    public IReadOnlyList<Booking> GetUpcoming()
    {
        var now = timeProvider.GetUtcNow();

        return bookingRepository.GetAll()
            .Where(booking => booking.End > now)
            .OrderBy(booking => booking.Start)
            .Select(ToModel)
            .ToList();
    }

    private Booking ToModel(BookingEntity entity) =>
        new(
            entity.Id,
            entity.EventTypeId,
            eventTypeRepository.GetById(entity.EventTypeId)?.Title ?? entity.EventTypeId,
            entity.Start,
            entity.End,
            entity.GuestName,
            entity.GuestEmail,
            entity.Comment,
            entity.CreatedAt);

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
        else if (guestName.Length > MaxGuestNameLength)
        {
            errors["guestName"] = [$"Имя не длиннее {MaxGuestNameLength} символов."];
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
