import "@typespec/http";

using Http;

@service(#{ title: "Call Calendar API" })
namespace CallCalendarApi;

/** Слот звонка в расписании. */
model CallSlot {
  @minValue(1)
  id: int32;

  title: string;

  startsAt: utcDateTime;
}

@route("api/calendar")
@tag("Calendar")
interface Calendar {
  /** Список слотов календаря. */
  @route("slots")
  @get
  listSlots(): CallSlot[];
}
