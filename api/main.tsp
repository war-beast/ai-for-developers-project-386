import "@typespec/http";

using Http;

/** API записи на звонок: типы встреч, слоты и бронирования. */
@service(#{ title: "Call Calendar API" })
namespace CallCalendarApi;

/** Тип встречи, который создаёт владелец. */
model EventType {
  /** Slug, заданный владельцем. */
  id: string;
  title: string;
  description: string;
  /** Длительность в минутах, 15–240, кратно 15. */
  durationMinutes: int32;
}

model CreateEventTypeRequest {
  id: string;
  title: string;
  description: string;
  durationMinutes: int32;
}

/** Свободный слот: начало и конец в поясе владельца. */
model Slot {
  start: offsetDateTime;
  end: offsetDateTime;
}

model SlotDay {
  /** Календарная дата в поясе владельца. */
  date: plainDate;
  slots: Slot[];
}

/** Окно записи на 14 дней со свободными слотами. */
model SlotWindow {
  /** IANA-пояс, в котором заданы рабочие часы. */
  timeZone: string;
  windowStart: plainDate;
  windowEnd: plainDate;
  stepMinutes: int32;
  days: SlotDay[];
}

model CreateBookingRequest {
  eventTypeId: string;
  start: offsetDateTime;
  guestName: string;
  guestEmail: string;
  comment?: string;
}

/** Созданная запись. */
model Booking {
  id: string;
  eventTypeId: string;
  eventTypeTitle: string;
  start: offsetDateTime;
  end: offsetDateTime;
  guestName: string;
  guestEmail: string;
  comment?: string;
  createdAt: utcDateTime;
}

/** Тело ошибки в формате ProblemDetails. */
model ProblemDetails {
  @header("Content-Type") contentType: "application/problem+json";
  type: string;
  title: string;
  status: int32;
  detail?: string;
  code: string;
  errors?: Record<string[]>;
}

@error
model ValidationProblem extends ProblemDetails {
  @statusCode statusCode: 400;
  code: "validation_failed";
}

@error
model EventTypeExistsProblem extends ProblemDetails {
  @statusCode statusCode: 409;
  code: "event_type_exists";
}

@error
model EventTypeNotFoundProblem extends ProblemDetails {
  @statusCode statusCode: 404;
  code: "event_type_not_found";
}

@error
model SlotTakenProblem extends ProblemDetails {
  @statusCode statusCode: 409;
  code: "slot_taken";
}

@error
model SlotNotAvailableProblem extends ProblemDetails {
  @statusCode statusCode: 400;
  code: "slot_not_available";
}

/** Успешное создание: 201 и созданный ресурс. */
model Created<T> {
  @statusCode statusCode: 201;
  @body body: T;
}

@route("/api/event-types")
@tag("Event types")
interface EventTypes {
  /** Список типов встреч. */
  @get
  list(): EventType[];

  /** Создать тип встречи. */
  @post
  create(@body body: CreateEventTypeRequest): Created<EventType> | ValidationProblem | EventTypeExistsProblem;

  /** Свободные слоты типа встречи на всё окно записи. */
  @get
  @route("/{eventTypeId}/slots")
  slots(@path eventTypeId: string): SlotWindow | EventTypeNotFoundProblem;
}

@route("/api/bookings")
@tag("Bookings")
interface Bookings {
  /** Предстоящие записи владельца. */
  @get
  list(): Booking[];

  /** Забронировать слот. */
  @post
  create(
    @body body: CreateBookingRequest,
  ): Created<Booking> | ValidationProblem | EventTypeNotFoundProblem | SlotTakenProblem | SlotNotAvailableProblem;
}
