/** Пояс владельца: время везде показывается с этой подписью. */
export const OWNER_TIME_ZONE = 'Europe/Moscow'

const months = [
  'янв',
  'фев',
  'мар',
  'апр',
  'май',
  'июн',
  'июл',
  'авг',
  'сен',
  'окт',
  'ноя',
  'дек',
]

/** Подпись дня: «3 ноя». */
export function formatDayLabel(date: string) {
  const [, month, day] = date.split('-')

  return `${Number(day)} ${months[Number(month) - 1] ?? ''}`
}

/** Время слота: бекенд уже отдаёт его в поясе владельца. */
export function formatSlotTime(start: string) {
  return start.slice(11, 16)
}

/** Номер дня недели с понедельника: 0 — понедельник, 6 — воскресенье. */
export function weekdayIndex(date: string) {
  return (new Date(`${date}T00:00:00Z`).getUTCDay() + 6) % 7
}

/** Дата и время записи: «3 ноя, 10:00». */
export function formatDateTime(start: string) {
  return `${formatDayLabel(start.slice(0, 10))}, ${formatSlotTime(start)}`
}
