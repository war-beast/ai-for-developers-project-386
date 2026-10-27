import { useQuery } from '@tanstack/react-query'

import { fetchBookings } from '@/api/bookings'
import { formatDayLabel, formatSlotTime } from '@/lib/dates'

export function MeetingsPage() {
  const { data: bookings = [], isPending } = useQuery({
    queryKey: ['bookings'],
    queryFn: fetchBookings,
    // Владелец возвращается на вкладку — список должен быть свежим.
    refetchOnWindowFocus: true,
  })

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-4 px-4 py-10">
      <h1 className="text-4xl font-light tracking-tight">Встречи</h1>
      <p className="text-sm text-muted-foreground">
        Предстоящие встречи всех типов · Europe/Moscow
      </p>

      {isPending ? (
        <p className="text-muted-foreground">Загрузка…</p>
      ) : bookings.length === 0 ? (
        <p className="text-muted-foreground">Предстоящих встреч пока нет</p>
      ) : (
        <ul aria-label="Предстоящие встречи" className="grid gap-2">
          {bookings.map((booking) => (
            <li
              key={booking.id}
              className="border border-border bg-card px-4 py-3 text-sm"
            >
              <p className="font-medium">
                {formatDayLabel(booking.start.slice(0, 10))},{' '}
                {formatSlotTime(booking.start)} — {booking.eventTypeTitle}
              </p>
              <p className="text-muted-foreground">
                {booking.guestName} — {booking.guestEmail}
                {booking.comment ? ` — ${booking.comment}` : ''}
              </p>
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}
