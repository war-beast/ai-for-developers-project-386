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

const weekdays = ['вс', 'пн', 'вт', 'ср', 'чт', 'пт', 'сб']

/** Подпись дня в ленте: «ср, 28 окт». */
export function formatDayLabel(date: string) {
  const weekday = weekdays[new Date(`${date}T12:00:00Z`).getUTCDay()] ?? ''
  const [, month, day] = date.split('-')

  return `${weekday}, ${Number(day)} ${months[Number(month) - 1] ?? ''}`
}

/** Время слота: бекенд уже отдаёт его в поясе владельца. */
export function formatSlotTime(start: string) {
  return start.slice(11, 16)
}
