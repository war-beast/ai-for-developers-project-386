import "@typespec/http";

using Http;

/** API сервиса записи на звонок. Эндпоинты появятся на шаге бронирования. */
@service(#{ title: "Call Calendar API" })
namespace CallCalendarApi;
